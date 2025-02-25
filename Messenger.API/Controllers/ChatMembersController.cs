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
        [HttpPost]
        public async Task<ActionResult<UserDto>> JoinChat(int chatId)
        {
            var chat = await _context.Chats.Where(c => c.ChatId == chatId).FirstOrDefaultAsync();
            var user = await _context.Users.Where(u => u.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();
            
            if (chat == null) 
            { 
                return NotFound();
            }

            var chatMember = new ChatMember
            {
                Chat = chat,
                ChatId = chatId,
                JoinedAt = DateTime.UtcNow,
                Role = "Member",
                User = user,
                UserId = user.UserId
            };

            await _context.ChatMembers.AddAsync(chatMember);
            await _context.SaveChangesAsync();

            var response = new UserDto
            {
                AvatarUrl = user.AvatarUrl,
                LastSeen = user.LastSeen,
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PasswordHash = user.PasswordHash,
                UserId = user.UserId,
                Username = user.Username
            };

            await _hubContext.Clients.Group(chatId.ToString()).SendAsync("newChatMember", response);

            return Ok(response);
        }
        [HttpDelete] 
        public async Task<ActionResult<UserDto>> LeaveChat(int chatId)
        {
            var chat = await _context.Chats.Include(c => c.Members).FirstOrDefaultAsync(c => c.ChatId == chatId);

            if (chat == null)
            {
                return NotFound();
            }

            var member = await _context.ChatMembers.Include(cm => cm.User).FirstOrDefaultAsync(cm => cm.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

            if (member == null)
            {
                return NotFound();
            }

            var response = new UserDto
            {
                AvatarUrl = member.User.AvatarUrl,
                CreatedAt = member.User.CreatedAt,
                Email = member.User.Email,
                FirstName = member.User.FirstName,
                LastName = member.User.LastName,
                LastSeen = member.User.LastSeen,
                PasswordHash = member.User.PasswordHash,
                UserId = member.UserId,
                Username = member.User.Username
            };

            _context.ChatMembers.Remove(member);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(chatId.ToString()).SendAsync("leaveChatMember", response);

            return Ok();
        }
    }
}
