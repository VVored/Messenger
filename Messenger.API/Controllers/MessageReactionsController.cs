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

namespace Messenger.API.Controllers
{
    [Authorize]
    [Route("api/messages/{messageId}/reactions")]
    [ApiController]
    public class MessageReactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageReactionsController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageReactionDto>>> GetMessageReactions(int messageId)
        {
            var message = await _context.Messages.Where(m => m.MessageId == messageId).Include(m => m.Chat).Include(m => m.Sender).FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            var messageReactions = await _context.MessageReactions.Where(mr => mr.MessageId == messageId).Include(mr => mr.User).ToListAsync();

            if (messageReactions == null)
            {
                return NotFound();
            }

            var response = messageReactions.Select(mr => new MessageReactionDto
            {
                ReactionId = mr.MessageId,
                Reaction = mr.Reaction,
                MessageId = messageId,
                Message = new MessageDto
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
                },
                UserId = mr.UserId,
                User = new UserDto
                {
                    UserId = mr.UserId,
                    AvatarUrl = mr.User.AvatarUrl,
                    Email = mr.User.Email,
                    FirstName = mr.User.FirstName,
                    LastName = mr.User.LastName,
                    LastSeen = mr.User.LastSeen,
                    PasswordHash = mr.User.PasswordHash
                }
            });

            return Ok(messageReactions);
        }
        [HttpGet("{reactionId}", Name = "GetMessageReaction")]
        public async Task<ActionResult<MessageReactionDto>> GetMessageReaction(int messageId, int reactionId)
        {
            var message = await _context.Messages.Where(m => m.MessageId == messageId).FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            var messageReaction = await _context.MessageReactions.Where(mr => mr.MessageId == messageId && mr.ReactionId == reactionId).FirstOrDefaultAsync();

            if (messageReaction == null)
            {
                return NotFound();
            }

            var response = new MessageReactionDto
            {
                ReactionId = messageReaction.MessageId,
                Reaction = messageReaction.Reaction,
                MessageId = messageId,
                Message = new MessageDto
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
                },
                UserId = messageReaction.UserId,
                User = new UserDto
                {
                    UserId = messageReaction.UserId,
                    AvatarUrl = messageReaction.User.AvatarUrl,
                    Email = messageReaction.User.Email,
                    FirstName = messageReaction.User.FirstName,
                    LastName = messageReaction.User.LastName,
                    LastSeen = messageReaction.User.LastSeen,
                    PasswordHash = messageReaction.User.PasswordHash
                }
            };

            return Ok(response);
        }
        [HttpPost("{reactionId}")]
        public async Task<ActionResult<MessageReactionDto>> AddReaction(int messageId, [FromBody] MessageReactionForAddDto messageReactionForAdd)
        {
            var message = await _context.Messages.Where(m => m.MessageId == messageId).FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Where(u => u.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var newReaction = new MessageReaction
            {
                Message = message,
                MessageId = messageId,
                User = user,
                UserId = user.UserId,
                Reaction = messageReactionForAdd.Reaction
            };

            await _context.MessageReactions.AddAsync(newReaction);
            await _context.SaveChangesAsync();

            var response = new MessageReactionDto
            {
                ReactionId = newReaction.MessageId,
                Reaction = newReaction.Reaction,
                MessageId = message.MessageId,
                Message = new MessageDto
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
                },
                UserId = user.UserId,
                User = new UserDto
                {
                    UserId = user.UserId,
                    AvatarUrl = user.AvatarUrl,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    LastSeen = user.LastSeen,
                    PasswordHash = user.PasswordHash
                }
            };

            await _hubContext.Clients.Group(message.ChatId.ToString()).SendAsync("newReaction", response);

            return CreatedAtRoute("GetMessageReaction", new { messageId, response.ReactionId }, response);
        }
        [HttpDelete("{reactionId}")]
        public async Task<IActionResult> RemoveMessageReaction(int messageId, int reactionId)
        {
            var message = await _context.Messages.Where(m => m.MessageId == messageId).FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            var reaction = await _context.MessageReactions.Where(mr => mr.MessageId == messageId && mr.ReactionId == reactionId).FirstOrDefaultAsync();

            if (reaction == null)
            {
                return NotFound();
            }

            _context.MessageReactions.Remove(reaction);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(message.ChatId.ToString()).SendAsync("removeReaction", reactionId); // потом продумаю когда буду дома че на клиент хуйню отправлять я рот ебал дто вручную мапить

            return Ok();
        }
    }
}
