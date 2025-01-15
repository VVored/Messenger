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
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        [HttpGet("chats/{chatId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesFromChat(int chatId)
        {
            var chat = await _context.Chats.Where(c => c.ChatId == chatId).FirstOrDefaultAsync();

            if (chat == null)
            {
                return NotFound();
            }

            var response = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .Include(m => m.Chat)
                .Include(m => m.Sender)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    ChatId = m.ChatId,
                    ChatDto = new ChatDto
                    {
                        ChatId = m.ChatId,
                        ChatType = m.Chat.ChatType,
                        CreatedAt = m.Chat.CreatedAt
                    },
                    SenderId = m.SenderId,
                    Sender = new UserDto
                    {
                        UserId = m.SenderId,
                        Username = m.Sender.Username,
                        CreatedAt = m.Sender.CreatedAt,
                        AvatarUrl = m.Sender.AvatarUrl,
                        Email = m.Sender.Email,
                        FirstName = m.Sender.FirstName,
                        LastName = m.Sender.LastName,
                        LastSeen = m.Sender.LastSeen,
                        PasswordHash = m.Sender.PasswordHash
                    },
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsDeleted = m.IsDeleted,
                    IsEdited = m.IsEdited,
                })
                .ToListAsync();

            return Ok(response);
        }

        [HttpGet("{messageId}", Name = "GetMessage")]
        public async Task<ActionResult<MessageDto>> GetMessage(int messageId)
        {
            var message = await _context.Messages.Where(m => m.MessageId == messageId).Include(m => m.Sender).Include(m => m.Chat).FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            var response = new MessageDto
            {
                MessageId = messageId,
                ChatId = message.ChatId,
                ChatDto = new ChatDto
                {
                    ChatId = message.ChatId,
                    ChatType = message.Chat.ChatType,
                    CreatedAt = message.Chat.CreatedAt
                },
                SenderId = message.SenderId,
                Sender = new UserDto
                {
                    UserId = message.SenderId,
                    Username = message.Sender.Username,
                    CreatedAt = message.Sender.CreatedAt,
                    AvatarUrl = message.Sender.AvatarUrl,
                    Email = message.Sender.Email,
                    FirstName = message.Sender.FirstName,
                    LastName = message.Sender.LastName,
                    LastSeen = message.Sender.LastSeen,
                    PasswordHash = message.Sender.PasswordHash
                },
                Content = message.Content,
                SentAt = message.SentAt,
                IsDeleted = message.IsDeleted,
                IsEdited = message.IsEdited,
            };

            return Ok(response);
        }
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage([FromBody] MessageForCreationDto messageForCreationDto)
        {
            var user = await _context.Users.Where(u => u.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();
            var chat = await _context.Chats.Where(c => c.ChatId == messageForCreationDto.ChatId).FirstOrDefaultAsync();

            if (chat == null)
            {
                return NotFound();
            }

            var message = new Message
            {
                Chat = chat,
                Content = messageForCreationDto.Content,
                Sender = user
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var response = new MessageDto
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                ChatDto = new ChatDto
                {
                    ChatId = message.ChatId,
                    ChatType = message.Chat.ChatType,
                    CreatedAt = message.Chat.CreatedAt
                },
                SenderId = message.SenderId,
                Sender = new UserDto
                {
                    UserId = message.SenderId,
                    Username = message.Sender.Username,
                    CreatedAt = message.Sender.CreatedAt,
                    AvatarUrl = message.Sender.AvatarUrl,
                    Email = message.Sender.Email,
                    FirstName = message.Sender.FirstName,
                    LastName = message.Sender.LastName,
                    LastSeen = message.Sender.LastSeen,
                    PasswordHash = message.Sender.PasswordHash
                },
                Content = message.Content,
                SentAt = message.SentAt,
                IsDeleted = message.IsDeleted,
                IsEdited = message.IsEdited,
            };

            await _hubContext.Clients.Group(messageForCreationDto.ChatId.ToString()).SendAsync("newMessage", response);

            return CreatedAtRoute(routeName: "GetMessage", routeValues: new { message.MessageId }, value: response);
        }
        [HttpPut("{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId, string content)
        {
            var message = await _context.Messages.Where(m => m.MessageId == messageId).Include(m => m.Chat).Include(m => m.Sender).FirstOrDefaultAsync();
            if (message == null)
            {
                return NotFound();
            }
            message.Content = content;
            message.IsEdited = true;

            await _context.SaveChangesAsync();

            var messageDto = new MessageDto
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                ChatDto = new ChatDto
                {
                    ChatId = message.ChatId,
                    ChatType = message.Chat.ChatType,
                    CreatedAt = message.Chat.CreatedAt
                },
                SenderId = message.SenderId,
                Sender = new UserDto
                {
                    UserId = message.SenderId,
                    Username = message.Sender.Username,
                    CreatedAt = message.Sender.CreatedAt,
                    AvatarUrl = message.Sender.AvatarUrl,
                    Email = message.Sender.Email,
                    FirstName = message.Sender.FirstName,
                    LastName = message.Sender.LastName,
                    LastSeen = message.Sender.LastSeen,
                    PasswordHash = message.Sender.PasswordHash
                },
                Content = message.Content,
                SentAt = message.SentAt,
                IsDeleted = message.IsDeleted,
                IsEdited = message.IsEdited
            };

            await _hubContext.Clients.Group(messageDto.ChatId.ToString()).SendAsync("messageEdit", messageDto);

            return Ok();
        }
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var message = await _context.Messages.Where(m => m.MessageId == messageId).FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            message.IsDeleted = true;

            await _context.SaveChangesAsync();

            var messageDto = new MessageDto
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                ChatDto = new ChatDto
                {
                    ChatId = message.ChatId,
                    ChatType = message.Chat.ChatType,
                    CreatedAt = message.Chat.CreatedAt
                },
                SenderId = message.SenderId,
                Sender = new UserDto
                {
                    UserId = message.SenderId,
                    Username = message.Sender.Username,
                    CreatedAt = message.Sender.CreatedAt,
                    AvatarUrl = message.Sender.AvatarUrl,
                    Email = message.Sender.Email,
                    FirstName = message.Sender.FirstName,
                    LastName = message.Sender.LastName,
                    LastSeen = message.Sender.LastSeen,
                    PasswordHash = message.Sender.PasswordHash
                },
                Content = message.Content,
                SentAt = message.SentAt,
                IsDeleted = message.IsDeleted,
                IsEdited = message.IsEdited
            };

            await _hubContext.Clients.Group(messageDto.ChatId.ToString()).SendAsync("messageDelete", messageDto);

            return Ok();
        }
    }
}
