using Messenger.API.Data;
using Messenger.API.DTOs;
using Messenger.API.Models;
using Messenger.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Messenger.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        public UsersController(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(string searchQuery)
        {
            var users = await _context.Users
                .Where(u => u.Username.ToLower().Contains(searchQuery.ToLower()) || u.FirstName.ToLower().Contains(searchQuery.ToLower()) || u.LastName.ToLower().Contains(searchQuery.ToLower()) || (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(searchQuery.ToLower()))
                .Select(u => new UserDto
                {
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    AvatarUrl = u.AvatarUrl,
                    CreatedAt = u.CreatedAt,
                    Email = u.Email,
                    LastSeen = u.LastSeen,
                    PasswordHash = u.PasswordHash,
                    UserId = u.UserId,
                })
                .ToListAsync();
            if (users == null)
            {
                return NotFound();
            }
            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.Where(u => u.UserId == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            var response = new UserDto
            {
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LastSeen = user.LastSeen,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                UserId = id
            };
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (_context.Users.Any(u => u.Username == registerRequest.Username))
            {
                return BadRequest("Username is already taken");
            }
            var hashedPassword = _passwordHasher.HashPassword(registerRequest.Password);
            var user = new User
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = hashedPassword,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                AvatarUrl = string.Empty
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }
        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
