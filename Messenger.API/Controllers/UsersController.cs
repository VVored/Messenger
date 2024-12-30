using Messenger.API.Data;
using Messenger.API.Models;
using Messenger.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("register")]
        public async Task<IActionResult> Reqister([FromBody] RegisterRequest registerRequest)
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
