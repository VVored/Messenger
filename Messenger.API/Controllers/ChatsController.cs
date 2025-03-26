using Messenger.API.Data;
using Messenger.API.DTOs;
using Messenger.API.Hubs;
using Messenger.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Messenger.API.Controllers
{
    [Authorize]
    [Route("api/chats")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        public ChatsController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatDto>>> GetChats(string searchQuery)
        {
            var chats = await _context.GroupChatInfos
                .Include(info => info.Chat)
                .Where(info => (info.GroupName.ToLower().Contains(searchQuery.ToLower()) || info.Description.ToLower().Contains(searchQuery.ToLower())) && info.Chat.ChatType == "public")
                .OrderBy(info => info.ChatId)
                .Select(info => new ChatDto { ChatId = info.Chat.ChatId, ChatType = info.Chat.ChatType, CreatedAt = info.Chat.CreatedAt, GroupName = info.GroupName, AvatarUrl = info.AvatarUrl, Description = info.Description })
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
            var query = _context.ChatMembers.Where(cm => cm.UserId == int.Parse(userId)).Include(cm => cm.Chat).Include(cm => cm.Chat.GroupChatInfo);
            var chats = await query.Select(cm => new ChatDto { ChatId = cm.ChatId, ChatType = cm.Chat.ChatType, CreatedAt = cm.Chat.CreatedAt, AvatarUrl = cm.Chat.GroupChatInfo.AvatarUrl, Description = cm.Chat.GroupChatInfo.Description, GroupName = cm.Chat.GroupChatInfo.GroupName }).ToListAsync();
            return Ok(chats);
        }
        [HttpGet("{chatId}", Name = "GetChat")]
        public async Task<IActionResult> GetChat(int chatId)
        {
            var chat = await _context.Chats.Where(c => c.ChatId == chatId).FirstOrDefaultAsync();
            if (chat == null)
            {
                return NotFound();
            }
            if (chat.ChatType == "public")
            {
                var chatDto = new ChatDto
                {
                    ChatId = chat.ChatId,
                    ChatType = chat.ChatType,
                    CreatedAt = chat.CreatedAt,
                    AvatarUrl = chat.GroupChatInfo.AvatarUrl,
                    GroupName = chat.GroupChatInfo.GroupName,
                    Description = chat.GroupChatInfo.Description,
                };

                return Ok(chatDto);
            }
            else
            {
                var chatMembers = await _context.ChatMembers.Include(cm => cm.User).Where(cm => cm.ChatId == chatId).ToListAsync();
                var firstUser = chatMembers.First().User;
                var secondUser = chatMembers.Last().User;
                var privateChatDto = new PrivateChatDto
                {
                    ChatId = chat.ChatId,
                    FirstUser = new UserDto
                    {
                        UserId = firstUser.UserId,
                        FirstName = firstUser.FirstName,
                        LastName = firstUser.LastName,
                        Username = firstUser.Username,
                        AvatarUrl = firstUser.AvatarUrl,
                        CreatedAt = firstUser.CreatedAt,
                        Email = firstUser.Email,
                        LastSeen = firstUser.LastSeen,
                        PasswordHash = firstUser.PasswordHash
                    },
                    SecondUser = new UserDto 
                    {
                        UserId = secondUser.UserId,
                        FirstName = secondUser.FirstName,
                        LastName = secondUser.LastName,
                        AvatarUrl= secondUser.AvatarUrl,
                        CreatedAt = secondUser.CreatedAt,
                        Email = secondUser.Email,
                        LastSeen = secondUser.LastSeen,
                        PasswordHash = secondUser.PasswordHash
                    }
                };

                return Ok(privateChatDto);
            }
        }
        [HttpPost]
        public async Task<ActionResult<ChatDto>> CreateChat([FromBody] ChatForCreationDto chatForCreationDto)
        {
            var createdChat = new Chat { ChatType = chatForCreationDto.ChatType };
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
            var chatMember = new ChatMember { Chat = createdChat, ChatId = createdChat.ChatId, User = admin, Role = "Admin", UserId = admin.UserId, JoinedAt = createdChat.CreatedAt };
            await _context.ChatMembers.AddAsync(chatMember);
            await _context.SaveChangesAsync();
            var response = new ChatDto
            {
                ChatId = createdChat.ChatId,
                ChatType = createdChat.ChatType,
                CreatedAt = createdChat.CreatedAt,
                AvatarUrl = chatInfo.AvatarUrl,
                GroupName = chatInfo.GroupName,
                Description = chatInfo.Description,
            };

            return CreatedAtRoute(routeName: "GetChat", routeValues: new { response.ChatId }, value: response);
        }
        [HttpPost]
        public async Task<ActionResult<ChatDto>> CreateChat([FromBody] PrivateChatForCreationDto chatForCreationDto)
        {
            var createdChat = new Chat { ChatType = "private" };
            await _context.Chats.AddAsync(createdChat);
            await _context.SaveChangesAsync();
            var firstUser = _context.Users.Where(u => u.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefault();
            if (firstUser == null)
            {
                return BadRequest();
            }
            var firstChatMember = new ChatMember { Chat = createdChat, ChatId = createdChat.ChatId, User = firstUser, Role = "Admin", UserId = firstUser.UserId, JoinedAt = createdChat.CreatedAt };
            await _context.ChatMembers.AddAsync(firstChatMember);
            await _context.SaveChangesAsync();
            var secondUser = _context.Users.Where(u => u.UserId == chatForCreationDto.SecondUserId).FirstOrDefault();
            if (secondUser == null)
            {
                return BadRequest();
            }
            var secondChatMember = new ChatMember { User = secondUser, UserId = secondUser.UserId, Chat = createdChat, ChatId = createdChat.ChatId, JoinedAt = createdChat.CreatedAt, Role = "Admin" };
            await _context.ChatMembers.AddAsync(secondChatMember);
            await _context.SaveChangesAsync();
            var response = new PrivateChatDto
            {
                ChatId = createdChat.ChatId,
                FirstUser = new UserDto
                {
                    UserId = firstUser.UserId,
                    FirstName = firstUser.FirstName,
                    LastName = firstUser.LastName,
                    LastSeen = firstUser.LastSeen,
                    AvatarUrl = firstUser.AvatarUrl,
                    CreatedAt = createdChat.CreatedAt,
                    Email = firstUser.Email,
                    PasswordHash = firstUser.PasswordHash,
                    Username = firstUser.Username,
                },
                SecondUser = new UserDto
                {
                    UserId = secondUser.UserId,
                    FirstName = secondUser.FirstName,
                    LastName = secondUser.LastName,
                    LastSeen = secondUser.LastSeen,
                    AvatarUrl = secondUser.AvatarUrl,
                    CreatedAt = secondUser.CreatedAt,
                    Email = secondUser.Email,
                    PasswordHash = secondUser.PasswordHash,
                    Username = secondUser.Username,
                }
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
