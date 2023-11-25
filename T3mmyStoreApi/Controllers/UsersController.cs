using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using T3mmyStoreApi.Models;
using T3mmyStoreApi.Services;

namespace T3mmyStoreApi.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }
        [HttpGet("/GetUsers")]
        public IActionResult GetUsers(int? page)
        {
            if (page == null || page < 1)
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;
            var count = _context.Users.Count();
            totalPages = (int)Math.Ceiling(count / (decimal)pageSize);

            var users = _context.Users.
                OrderByDescending(u => u.Id).
                Skip((int)(page - 1) * pageSize).
                Take(pageSize).
                ToList();



            List<UserProfileDto> userProfiles = (from user in users
                                                 let userProfile = new UserProfileDto()
                                                 {
                                                     Id = user.Id,
                                                     FirstName = user.FirstName,
                                                     LastName = user.LastName,
                                                     Email = user.Email,
                                                     Role = user.Role,
                                                 }
                                                 select userProfile).ToList();
            var response = new
            {
                Users = userProfiles,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            var userProfile = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
            };
            return Ok(userProfile);
        }
    }
}
