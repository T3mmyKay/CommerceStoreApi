using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using T3mmyStoreApi.Models;
using T3mmyStoreApi.Services;

namespace T3mmyStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext _context;
        private readonly EmailSender emailSender;

        public AccountController(IConfiguration configuration, ApplicationDbContext applicationDbContext, EmailSender emailSender)
        {
            this.configuration = configuration;
            this._context = applicationDbContext;
            this.emailSender = emailSender;
        }
        [HttpPost("/register")]
        public IActionResult Register(UserDto userDto)
        {
            var emailCount = _context.Users.Where(u => u.Email == userDto.Email).Count();
            if (emailCount > 0)
            {
                ModelState.AddModelError("Email", "This Email address is already used");
                return BadRequest(ModelState);
            }

            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

            User user = new User()
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone ?? "",
                Address = userDto.Address,
                Password = encryptedPassword,
                Role = "client",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var jwt = CreateJWToken(user);

            UserProfileDto userProfile = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
            };

            var response = new
            {
                token = jwt,
                user = userProfile,

            };

            return Ok(response);


        }

        [HttpPost("/login")]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid Email");
                return BadRequest(ModelState);
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(new User(), user.Password, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return BadRequest(ModelState);
            }

            var jwt = CreateJWToken(user);

            UserProfileDto userProfile = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
            };

            var response = new
            {
                token = jwt,
                user = userProfile,

            };

            return Ok(response);
        }
        [HttpPost("ForgotPassword")]
        public IActionResult PasswordReset(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid Email");
                return BadRequest(ModelState);
            }

            var oldPasswordReset = _context.PasswordResets.FirstOrDefault(p => p.Email == email);
            if (oldPasswordReset != null)
            {
                _context.PasswordResets.Remove(oldPasswordReset);
                _context.SaveChanges();
            }

            var token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();
            PasswordReset passwordReset = new PasswordReset()
            {
                Email = email,
                Token = token,
                CreatedAt = DateTime.Now,
            };

            _context.PasswordResets.Add(passwordReset);
            _context.SaveChanges();

            string subject = "Password Reset";
            string username = user.FirstName + " " + user.LastName;
            string message = "Hi " + username + ",<br><br>" + "We recieved your password reset request. <br>" + "Please copy the following token and and paste it in the Password Reset Form: <br>" + token + "<br><br>" + "Best Regards <br>";

            emailSender.SendEmail(subject, email, username, message).Wait();
            return Ok();

        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(string email, string token, string password)
        {
            var passwordReset = _context.PasswordResets.FirstOrDefault(p => p.Email == email && p.Token == token);
            if (passwordReset == null)
            {
                ModelState.AddModelError("Token", "Invalid Token");
                return BadRequest(ModelState);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid Email");
                return BadRequest(ModelState);
            }

            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), password);

            user.Password = encryptedPassword;
            _context.Users.Update(user);
            _context.PasswordResets.Remove(passwordReset);
            _context.SaveChanges();

            _context.PasswordResets.Remove(passwordReset);
            _context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpGet("Profile")]
        public IActionResult GetProfile()
        {

            int id = JWTReader.GetUserId(User);

            var user = _context.Users.Find(id);

            if (user == null)
            {
                return Unauthorized();
            }

            var userProfile = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
            };
            return Ok(userProfile);
        }

        [Authorize]
        [HttpPut("UpdateProfile")]
        public IActionResult UpdateProfile(UserProfileUpdateDto userProfileDto)
        {
            int id = JWTReader.GetUserId(User);

            var user = _context.Users.Find(id);

            if (user == null)
            {
                return Unauthorized();
            }

            user.FirstName = userProfileDto.FirstName;
            user.LastName = userProfileDto.LastName;
            user.Email = userProfileDto.Email;
            user.Phone = userProfileDto.Phone ?? "";
            user.Address = userProfileDto.Address;

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(userProfileDto);
        }

        [Authorize]
        [HttpPut("UpdatePassword")]
        public IActionResult UpdatePassword(PasswordUpdateDto passwordUpdateDto)
        {
            int id = JWTReader.GetUserId(User);

            var user = _context.Users.Find(id);

            if (user == null)
            {
                return Unauthorized();
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(new User(), user.Password, passwordUpdateDto.OldPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("OldPassword", "Invalid Old Password");
                return BadRequest(ModelState);
            }

            var encryptedPassword = passwordHasher.HashPassword(new User(), passwordUpdateDto.NewPassword);
            user.Password = encryptedPassword;

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok();
        }

        /*
        [Authorize]
        [HttpGet("GetTokenClaims")]
        public IActionResult GetTokenClaims()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                Dictionary<string, string> claims = new Dictionary<string, string>();
                foreach (var claim in identity.Claims)
                {
                    claims.Add(claim.Type, claim.Value);
                }
                return Ok(claims);
            }
            return Ok();
        }

        [Authorize]
        [HttpGet("AuthenticatedUsers")]
        public IActionResult AuthorizeAuthenticatedUsers()
        {
            return Ok("You are authorized");
        } 
        
        [Authorize(Roles = "admin")]
        [HttpGet("AuthorizedAdmin")]
        public IActionResult AuthorizeAdmin()
        {
            return Ok("You are authorized");
        } 
        
        [Authorize(Roles = "admin,seller")]
        [HttpGet("AuthorizedAdminAndSeller")]
        public IActionResult AuthorizeAdminAndSeller()
        {
            return Ok("You are authorized");
        }
        */
        /*
        [HttpGet("TestToken")]
        public IActionResult TestToken()
        {
            User user = new User()
            {
                Id = 2,
                Role = "Admin"
            };
            string jwt = CreateJWToken(user);
            var response = new { JWToken = jwt };

            return Ok(response);
        }
        */
        private string CreateJWToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id", "" + user.Id),
                new Claim("role", user.Role)
            };


            string strKey = configuration["JwtSettings:Key"]!;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
