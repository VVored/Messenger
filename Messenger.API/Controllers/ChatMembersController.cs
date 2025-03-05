using Messenger.API.Data;
using Messenger.API.DTOs;
using Messenger.API.Hubs;
using Messenger.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Mozilla;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Messenger.API.Controllers
{
    [Route("api/chats/{chatId}/members")]
    [ApiController]
    [Authorize]
    public class ChatMembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatMembersController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetChatMembers(int chatId)
        {
            var chat = await _context.Chats.Where(c => c.ChatId == chatId).Include(c => c.Members).FirstOrDefaultAsync();

            if (chat == null)
            {
                return NotFound();
            }

            var chatMembers = await _context.ChatMembers.Where(cm => cm.ChatId == chatId).Include(cm => cm.User).ToListAsync();

            var response = new List<UserDto>();

            foreach (var chatMember in chatMembers)
            {
                response.Add(new UserDto
                {
                    UserId = chatMember.UserId,
                    Username = chatMember.User.Username,
                    AvatarUrl = chatMember.User.AvatarUrl,
                    CreatedAt = chatMember.User.CreatedAt,
                    Email = chatMember.User.Email,
                    FirstName = chatMember.User.FirstName,
                    LastName = chatMember.User.LastName,
                    LastSeen = chatMember.User.LastSeen,
                    PasswordHash = chatMember.User.PasswordHash,
                });
            }

            return Ok(response);
        }
        [HttpGet("isjoined")]
        public async Task<ActionResult<bool>> IsUserJoined(int chatId)
        {
            var chat = await _context.Chats.Where(c => c.ChatId == chatId).FirstOrDefaultAsync();

            if (chat == null)
            {
                return NotFound();
            }

            var member = await _context.ChatMembers.Where(cm => cm.ChatId == chatId & cm.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();

            var response = false;

            if (member != null)
            {
                response = true;
            }

            return Ok(response);
        }
    }
}
