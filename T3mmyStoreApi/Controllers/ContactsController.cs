using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T3mmyStoreApi.Models;
using T3mmyStoreApi.Services;

namespace T3mmyStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly List<string> listSubjects = new List<string>()
        {
            "Order Status","Refund Request", "Job Application", "Other"
        };

        public ContactsController(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetContacts(int? page)
        {
            if (page == null || page < 1)
            {
                page = 1;
            }
            int pageSize = 5;
            int totalPages = 0;

            decimal count = _context.Contacts.Count();
            totalPages = (int)Math.Ceiling(count / pageSize);
            var contacts = _context.Contacts
                .Include(c => c.Subject)
                .OrderByDescending(c => c.Id)
                .Skip((int)(page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Contacts = contacts,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };
            return Ok(response);
        }

        [HttpGet("subjects")]
        public IActionResult GetSubjects()
        {
            var listSubjects = _context.Subjects.ToList();

            return Ok(listSubjects);

        }

        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public IActionResult GetContact(int id)
        {
            var contact = _context.Contacts.Include(c => c.Subject).FirstOrDefault(c => c.Id == id);

            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [HttpPost]
        public IActionResult CreateContact(ContactDto contactDto, [FromServices] EmailSender emailSender)
        {
            var subject = _context.Subjects.Find(contactDto.SubjectId);
            if (subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a valid subject");
                return BadRequest(ModelState);
            }
            Contact contact = new Contact()
            {
                FirstName = contactDto.FirstName,
                LastName = contactDto.LastName,
                Email = contactDto.Email,
                Phone = contactDto.Phone ?? "",
                Subject = subject,
                Message = contactDto.Message,
                CreatedAt = DateTime.Now,

            };
            _context.Contacts.Add(contact);
            _context.SaveChanges();


            string emailSubject = subject.Name;
            string userName = $"{contactDto.FirstName} {contactDto.LastName}";
            string emailMessage = $"Dear {userName}<br>" +
                $"We received your message. Thank you for contacting us.<br>" +
                $"Our team will contact you very soon.<br>" +
                $"Best Regards<br><br>" +
                $"Your Message:<br>{contactDto.Message.Replace("\n", "<br>")}";


            emailSender.SendEmail(emailSubject, contact.Email, userName, emailMessage).Wait();
            return Ok(contact);
        }
        /*
        [HttpPut("{id}")]
        public IActionResult UpdateContact(ContactDto contactDto,int id) {

            var subject = _context.Subjects.Find(contactDto.SubjectId);
            if (subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a valid subject");
                return BadRequest(ModelState);
            }
            var contact = _context.Contacts.Find(id);
            if (contact == null)
            {
                return NotFound();
            }

            contact.FirstName = contactDto.FirstName;
            contact.LastName = contactDto.LastName;
            contact.Email = contactDto.Email;
            contact.Phone = contactDto.Phone ?? "";
            contact.Subject = subject;
            contact.Message = contactDto.Message;

            _context.SaveChanges();

            return Ok(contact);
        }
        */
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteContact(int id)
        {
            //Method 1

            //var contact = _context.Contacts.Find(id);
            //if (contact == null)
            //{
            //    return NotFound();
            //}
            //_context.Contacts.Remove(contact);
            //_context.SaveChanges();

            //return Ok();

            //Method 2
            try
            {
                var contact = new Contact() { Id = id, Subject = new Subject() };
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return NotFound();
            }
            return Ok();


        }
    }
}
