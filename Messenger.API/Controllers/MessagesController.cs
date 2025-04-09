using Messenger.API.Data;
using Messenger.API.DTOs;
using Messenger.API.Hubs;
using Messenger.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
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
                .Include(m => m.Attachments)
                .Include(m => m.RepliableMessage)
                .Include(m => m.RepliableMessage.Sender)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
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
                    Attachments = m.Attachments.Select(a => new AttachmentDto { MessageId = a.MessageId, AttachmentId = a.AttachmentId, FileSize = a.FileSize, FileType = a.FileType, FileUrl = a.FileUrl }).ToList(),
                    RepliableMessage = m.RepliableMessage != null ? new RepliableMessageDto 
                    {
                        Content = m.RepliableMessage.Content.IsNullOrEmpty() ? m.RepliableMessage.Attachments.Count.ToString() + " вложений" : m.RepliableMessage.Content,
                        MessageId = m.RepliableMessage.MessageId,
                        Sender = new UserDto 
                        {
                            UserId = m.RepliableMessage.Sender.UserId,
                            AvatarUrl = m.RepliableMessage.Sender.AvatarUrl,
                            CreatedAt = m.RepliableMessage.Sender.CreatedAt,
                            Email = m.RepliableMessage.Sender.Email, 
                            FirstName = m.RepliableMessage.Sender.FirstName,
                            LastName = m.RepliableMessage.Sender.LastName,
                            LastSeen = m.RepliableMessage.Sender.LastSeen,
                            PasswordHash = m.RepliableMessage.Sender.PasswordHash,
                            Username = m.RepliableMessage.Sender.Username,
                        },
                        SenderId = m.RepliableMessage.Sender.UserId
                    } : null,
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
            var message = await _context.Messages.Where(m => m.MessageId == messageId).Include(m => m.Sender).Include(m => m.Chat).Include(m => m.Chat.GroupChatInfo).Include(m => m.RepliableMessage).FirstOrDefaultAsync();

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
                    CreatedAt = message.Chat.CreatedAt,
                    GroupName = message.Chat.GroupChatInfo.GroupName,
                    Description = message.Chat.GroupChatInfo.Description,
                    AvatarUrl = message.Chat.GroupChatInfo.AvatarUrl,
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
                Attachments = message.Attachments.Select(a => new AttachmentDto { MessageId = a.MessageId, FileUrl = a.FileUrl, FileType = a.FileType, AttachmentId = a.AttachmentId, FileSize = a.FileSize }).ToList(),
                RepliableMessage = message.RepliableMessage != null ? new RepliableMessageDto
                {
                    Content = message.RepliableMessage.Content.IsNullOrEmpty() ? message.RepliableMessage.Attachments.Count.ToString() + " вложений" : message.RepliableMessage.Content,
                    MessageId = message.RepliableMessage.MessageId,
                    Sender = new UserDto
                    {
                        UserId = message.RepliableMessage.Sender.UserId,
                        AvatarUrl = message.RepliableMessage.Sender.AvatarUrl,
                        CreatedAt = message.RepliableMessage.Sender.CreatedAt,
                        Email = message.RepliableMessage.Sender.Email,
                        FirstName = message.RepliableMessage.Sender.FirstName,
                        LastName = message.RepliableMessage.Sender.LastName,
                        LastSeen = message.RepliableMessage.Sender.LastSeen,
                        PasswordHash = message.RepliableMessage.Sender.PasswordHash,
                        Username = message.RepliableMessage.Sender.Username,
                    },
                    SenderId = message.RepliableMessage.Sender.UserId
                } : null,
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
            var chat = await _context.Chats.Where(c => c.ChatId == messageForCreationDto.ChatId).Include(c => c.GroupChatInfo).FirstOrDefaultAsync();

            if (chat == null || user == null)
            {
                return NotFound();
            }

            var repliableMessage = await _context.Messages.Include(m => m.Sender).Include(m => m.Attachments).Where(m => m.MessageId == messageForCreationDto.RepliableMessageId).FirstOrDefaultAsync();

            var message = new Message
            {
                Chat = chat,
                Content = messageForCreationDto.Content,
                Sender = user,
                RepliableMessageId = repliableMessage?.MessageId,
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var attachments = messageForCreationDto.Attachments.Select(a => new Attachment { FileSize = a.FileSize, FileType = a.FileType, FileUrl = a.FileUrl, Message = message, MessageId = message.MessageId });

            await _context.Attachments.AddRangeAsync(attachments);
            await _context.SaveChangesAsync();

            var response = new MessageDto
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                ChatDto = new ChatDto
                {
                    ChatId = chat.ChatId,
                    ChatType = chat.ChatType,
                    CreatedAt = chat.CreatedAt,
                    GroupName = chat.GroupChatInfo.GroupName,
                    Description = chat.GroupChatInfo.Description,
                    AvatarUrl = chat.GroupChatInfo.AvatarUrl,
                },
                SenderId = message.SenderId,
                Sender = new UserDto
                {
                    UserId = message.SenderId,
                    Username = user.Username,
                    CreatedAt = user.CreatedAt,
                    AvatarUrl = user.AvatarUrl,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    LastSeen = user.LastSeen,
                    PasswordHash = user.PasswordHash
                },
                Attachments = attachments.Select(a => new AttachmentDto { MessageId = a.MessageId, AttachmentId = a.AttachmentId, FileSize = a.FileSize, FileType = a.FileType, FileUrl = a.FileUrl }).ToList(),
                RepliableMessage = repliableMessage != null ? new RepliableMessageDto
                {
                    Content = repliableMessage.Content.IsNullOrEmpty() ? repliableMessage.Attachments.Count.ToString() + " вложений" : repliableMessage.Content,
                    MessageId = repliableMessage.MessageId,
                    Sender = new UserDto
                    {
                        UserId = repliableMessage.Sender.UserId,
                        AvatarUrl = repliableMessage.Sender.AvatarUrl,
                        CreatedAt = repliableMessage.Sender.CreatedAt,
                        Email = repliableMessage.Sender.Email,
                        FirstName = repliableMessage.Sender.FirstName,
                        LastName = repliableMessage.Sender.LastName,
                        LastSeen = repliableMessage.Sender.LastSeen,
                        PasswordHash = repliableMessage.Sender.PasswordHash,
                        Username = repliableMessage.Sender.Username,
                    },
                    SenderId = repliableMessage.Sender.UserId
                } : null,
                Content = message.Content,
                SentAt = message.SentAt,
                IsDeleted = message.IsDeleted,
                IsEdited = message.IsEdited,
            };

            await _hubContext.Clients.Group(messageForCreationDto.ChatId.ToString()).SendAsync("newMessage", response);

            return CreatedAtRoute(routeName: "GetMessage", routeValues: new { message.MessageId }, value: response);
        }
        [HttpPut("{messageId}")]
        public async Task<IActionResult> EditMessage([FromBody] MessageForEditDto messageForEditDto)
        {
            var message = await _context.Messages.Where(m => m.MessageId == messageForEditDto.MessageId).Include(m => m.Chat).Include(m => m.Sender).FirstOrDefaultAsync();
            if (message == null)
            {
                return NotFound();
            }
            message.Content = messageForEditDto.Content;
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

            var attachments = await _context.Attachments.Where(a => a.MessageId == messageId).ToListAsync();

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
                Attachments = attachments.Select(a => new AttachmentDto { AttachmentId = a.AttachmentId, MessageId = a.MessageId, FileSize = a.FileSize, FileType = a.FileType, FileUrl = a.FileUrl }).ToList(),
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
