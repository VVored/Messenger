using Messenger.API.Data;
using Messenger.API.Services;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Messenger.API.Controllers
{
    [Route("api/authorization")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        public AuthController(IJwtTokenService jwtTokenService, ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequst loginRequst)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequst.Username);
            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }
            if (!_passwordHasher.VerifyPassword(loginRequst.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password");
            }

            var token = _jwtTokenService.GenerateToken(user.UserId.ToString(), user.Username);

            return Ok(new {Token = token});
        }
        public class LoginRequst 
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
