using Messenger.API.Data;
using Messenger.API.DTOs;
using Messenger.API.Hubs;
using Messenger.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Permissions;

namespace Messenger.API.Controllers
{
    [Authorize]
    [Route("api/chats")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ChatsController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatDto>>> GetChats(string searchQuery)
        {
            var chats = await _context.GroupChatInfos
                .Where(info => info.GroupName.ToLower().Contains(searchQuery.ToLower()) || info.Description.ToLower().Contains(searchQuery.ToLower()))
                .Include(info => info.Chat)
                .Select(info => new ChatDto { ChatId = info.Chat.ChatId, ChatType = info.Chat.ChatType, CreatedAt = info.Chat.CreatedAt})
                .ToListAsync();
            return Ok(chats);
        }
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ChatDto>>> GetMyChats() 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            var query = _context.ChatMembers.Where(cm => cm.UserId == int.Parse(userId)).Include(cm => cm.Chat);
            var chats = await query.Select(cm => new ChatDto { ChatId = cm.ChatId, ChatType = cm.Chat.ChatType, CreatedAt = cm.Chat.CreatedAt }).ToListAsync();
            return Ok(chats);
        }
        [HttpGet("{chatId}", Name = "GetChat")]
        public async Task<ActionResult<ChatDto>> GetChat(int chatId)
        {
            var chat = await _context.Chats.Where(c => c.ChatId == chatId).FirstOrDefaultAsync();
            if (chat == null)
            {
                return NotFound();
            }
            var chatDto = new ChatDto
            {
                ChatId = chat.ChatId,
                ChatType = chat.ChatType,
                CreatedAt = chat.CreatedAt,
            };
            return Ok(chatDto);
        }
        [HttpPost]
        public async Task<ActionResult<ChatDto>> CreateChat([FromBody] ChatForCreationDto chatForCreationDto)
        {
            var createdChat = new Chat { ChatType = chatForCreationDto.ChatType};
            await _context.Chats.AddAsync(createdChat);
            await _context.SaveChangesAsync();
            var chatInfo = new GroupChatInfo { AvatarUrl = chatForCreationDto.AvatarUrl, Chat = createdChat, ChatId = createdChat.ChatId, Description = chatForCreationDto.Description, GroupName = chatForCreationDto.GroupName };
            await _context.GroupChatInfos.AddAsync(chatInfo);
            await _context.SaveChangesAsync();
            var admin = _context.Users.Where(u => u.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefault();
            if (admin == null)
            {
                return BadRequest();
            }
            var chatMember = new ChatMember { Chat = createdChat, ChatId = createdChat.ChatId, User = admin, Role = "Admin", UserId = admin.UserId};
            await _context.ChatMembers.AddAsync(chatMember);
            await _context.SaveChangesAsync();
            var response = new ChatDto
            {
                ChatId = createdChat.ChatId,
                ChatType = createdChat.ChatType,
                CreatedAt = createdChat.CreatedAt
            };
            return CreatedAtRoute(routeName: "GetChat", routeValues: new { response.ChatId }, value: response);
        }
        [HttpPut("{chatId}")]
        public async Task<IActionResult> Edit(int chatId, [FromBody] ChatForEditDto chatForEditDto)
        {
            var editableChat = await _context.Chats.Where(c => c.ChatId == chatId).FirstOrDefaultAsync();
            if (editableChat == null)
            {
                return NotFound();
            }
            editableChat.ChatType = chatForEditDto.ChatType;
            var editableChatGroupInfo = await _context.GroupChatInfos.Where(info => info.ChatId == chatId).FirstOrDefaultAsync();
            if (editableChatGroupInfo == null)
            {
                return NotFound();
            }
            editableChatGroupInfo.Description = chatForEditDto.Description;
            editableChatGroupInfo.GroupName = chatForEditDto.GroupName;
            editableChatGroupInfo.AvatarUrl = chatForEditDto.AvatarUrl;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("{chatId}")]
        public async Task<IActionResult> Delete(int chatId)
        {
            var deletableChat = await _context.Chats.Where(c => c.ChatId == chatId).FirstOrDefaultAsync();
            if (deletableChat == null)
            {
                return NotFound();
            }
            _context.Chats.Remove(deletableChat);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
