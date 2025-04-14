using Messenger.API.Data;
using Messenger.API.DTOs;
using Messenger.API.Hubs;
using Messenger.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
            var chats = await _context.Chats
                .Include(c => c.GroupChatInfo)
                .Where(c => (c.GroupChatInfo.GroupName.ToLower().Contains(searchQuery.ToLower()) || c.GroupChatInfo.Description.ToLower().Contains(searchQuery.ToLower())) && c.GroupChatInfo.Chat.ChatType == "public")
                .Select(c => new ChatDto
                {
                    ChatId = c.ChatId,
                    ChatType = c.ChatType,
                    CreatedAt = c.CreatedAt,
                    GroupName = c.GroupChatInfo.GroupName,
                    AvatarUrl = c.GroupChatInfo.AvatarUrl,
                    Description = c.GroupChatInfo.Description
                })
                .ToListAsync();
            foreach (var i in chats)
            {
                var lastMessage = await _context.Messages.Include(c => c.Attachments).Include(c => c.Sender).Where(c => c.ChatId == i.ChatId).OrderBy(c => c.SentAt).LastOrDefaultAsync();
                var chatMembers = await _context.ChatMembers
                    .Include(cm => cm.User)
                    .Where(cm => cm.ChatId == i.ChatId)
                    .Select(cm => new ChatMemberDto
                    {
                        JoinedAt = cm.JoinedAt,
                        Role = cm.Role,
                        User = new UserDto
                        {
                            UserId = cm.UserId,
                            LastSeen = cm.User.LastSeen,
                            AvatarUrl = cm.User.AvatarUrl,
                            CreatedAt = cm.User.CreatedAt,
                            Email = cm.User.Email,
                            FirstName = cm.User.FirstName,
                            LastName = cm.User.LastName,
                            PasswordHash = cm.User.PasswordHash,
                            Username = cm.User.Username
                        }
                    }
                    ).ToListAsync();
                i.ChatMembers = chatMembers;
                i.LastMessage = lastMessage != null ? new LastMessageDto
                {
                    Content = lastMessage.Content.IsNullOrEmpty() ? lastMessage.Attachments.Count + " вложений" : lastMessage.Content,
                    MessageId = lastMessage.MessageId,
                    SenderId = lastMessage.SenderId,
                    Sender = new UserDto
                    {
                        AvatarUrl = lastMessage.Sender.AvatarUrl,
                        CreatedAt = lastMessage.Sender.CreatedAt,
                        Email = lastMessage.Sender.Email,
                        FirstName = lastMessage.Sender.FirstName,
                        LastName = lastMessage.Sender.LastName,
                        LastSeen = lastMessage.Sender.LastSeen,
                        PasswordHash = lastMessage.Sender.PasswordHash,
                        UserId = lastMessage.Sender.UserId,
                        Username = lastMessage.Sender.Username
                    },
                    ChatId = lastMessage.ChatId
                } : null;
            }
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
            foreach (var i in chats)
            {
                var lastMessage = await _context.Messages.Include(c => c.Attachments).Include(c => c.Sender).Where(c => c.ChatId == i.ChatId).OrderBy(c => c.SentAt).LastOrDefaultAsync();
                var chatMembers = await _context.ChatMembers
                    .Include(cm => cm.User)
                    .Where(cm => cm.ChatId == i.ChatId)
                    .Select(cm => new ChatMemberDto
                    {
                        JoinedAt = cm.JoinedAt,
                        Role = cm.Role,
                        User = new UserDto
                        {
                            UserId = cm.UserId,
                            LastSeen = cm.User.LastSeen,
                            AvatarUrl = cm.User.AvatarUrl,
                            CreatedAt = cm.User.CreatedAt,
                            Email = cm.User.Email,
                            FirstName = cm.User.FirstName,
                            LastName = cm.User.LastName,
                            PasswordHash = cm.User.PasswordHash,
                            Username = cm.User.Username
                        }
                    }
                    ).ToListAsync();
                i.ChatMembers = chatMembers;
                i.LastMessage = lastMessage != null ? new LastMessageDto
                {
                    Content = lastMessage.Content.IsNullOrEmpty() ? lastMessage.Attachments.Count + " вложений" : lastMessage.Content,
                    MessageId = lastMessage.MessageId,
                    SenderId = lastMessage.SenderId,
                    Sender = new UserDto
                    {
                        AvatarUrl = lastMessage.Sender.AvatarUrl,
                        CreatedAt = lastMessage.Sender.CreatedAt,
                        Email = lastMessage.Sender.Email,
                        FirstName = lastMessage.Sender.FirstName,
                        LastName = lastMessage.Sender.LastName,
                        LastSeen = lastMessage.Sender.LastSeen,
                        PasswordHash = lastMessage.Sender.PasswordHash,
                        UserId = lastMessage.Sender.UserId,
                        Username = lastMessage.Sender.Username
                    },
                    ChatId = lastMessage.ChatId
                } : null;
            }
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
            var lastMessage = await _context.Messages.Include(c => c.Attachments).Include(c => c.Sender).Where(c => c.ChatId == chatId).OrderBy(c => c.SentAt).LastOrDefaultAsync();
            var chatMembers = await _context.ChatMembers.Where(cm => cm.ChatId == chatId).Include(cm => cm.User).Select(cm => new ChatMemberDto
            {
                JoinedAt = cm.JoinedAt,
                Role = cm.Role,
                User = new UserDto
                {
                    UserId = cm.UserId,
                    Username = cm.User.Username,
                    AvatarUrl = cm.User.AvatarUrl,
                    CreatedAt = cm.User.CreatedAt,
                    Email = cm.User.Email,
                    FirstName = cm.User.FirstName,
                    LastName = cm.User.LastName,
                    LastSeen = cm.User.LastSeen,
                    PasswordHash = cm.User.PasswordHash,
                }
            }).ToListAsync();

            var chatDto = new ChatDto
            {
                ChatId = chat.ChatId,
                ChatType = chat.ChatType,
                CreatedAt = chat.CreatedAt,
                AvatarUrl = chat.GroupChatInfo.AvatarUrl,
                GroupName = chat.GroupChatInfo.GroupName,
                Description = chat.GroupChatInfo.Description,
                ChatMembers = chatMembers,
                LastMessage = lastMessage != null ? new LastMessageDto
                {
                    Content = lastMessage.Content.IsNullOrEmpty() ? lastMessage.Attachments.Count + " вложений" : lastMessage.Content,
                    MessageId = lastMessage.MessageId,
                    SenderId = lastMessage.SenderId,
                    Sender = new UserDto
                    {
                        AvatarUrl = lastMessage.Sender.AvatarUrl,
                        CreatedAt = lastMessage.Sender.CreatedAt,
                        Email = lastMessage.Sender.Email,
                        FirstName = lastMessage.Sender.FirstName,
                        LastName = lastMessage.Sender.LastName,
                        LastSeen = lastMessage.Sender.LastSeen,
                        PasswordHash = lastMessage.Sender.PasswordHash,
                        UserId = lastMessage.Sender.UserId,
                        Username = lastMessage.Sender.Username
                    },
                    ChatId = lastMessage.ChatId
                } : null
            };

            return Ok(chatDto);
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
                ChatMembers = new List<ChatMemberDto>
                {
                    new ChatMemberDto
                    {
                        JoinedAt = chatMember.JoinedAt,
                        Role = chatMember.Role,
                        User = new UserDto
                        {
                            UserId = admin.UserId,
                            LastSeen = admin.LastSeen,
                            AvatarUrl = admin.AvatarUrl,
                            CreatedAt = admin.CreatedAt,
                            Email = admin.Email,
                            FirstName = admin.FirstName,
                            LastName = admin.LastName,
                            PasswordHash = admin.PasswordHash,
                            Username = admin.Username
                        }
                    }
                },
                LastMessage = null
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
