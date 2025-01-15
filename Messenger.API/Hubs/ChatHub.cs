using Messenger.API.Data;
using Messenger.API.DTOs;
using Messenger.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Messenger.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly static Dictionary<string, string> _connections = new Dictionary<string, string>();

        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task JoinChat(string chatId)
        {
            var user = await _context.Users.Where(u => u.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();
            var chat = await _context.Chats.Where(c => c.ChatId == int.Parse(chatId)).FirstOrDefaultAsync();

            if (user != null && chat != null)
            {
                await _context.ChatMembers.AddAsync(new ChatMember
                {
                    Chat = chat,
                    User = user,
                    JoinedAt = DateTime.UtcNow,
                    Role = "User"
                });
                await _context.SaveChangesAsync();

                var userResponse = new UserDto
                {
                    AvatarUrl = user.AvatarUrl,
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = user.PasswordHash,
                    CreatedAt = user.CreatedAt,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    LastSeen = user.LastSeen
                };

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);

                await Clients.OthersInGroup(chatId).SendAsync("newMember", userResponse);
            }
        }
        public async Task LeaveChat(string chatId)
        {
            var chatMember = await _context.ChatMembers.Where(cm => cm.ChatId == int.Parse(chatId) && cm.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).Include(cm => cm.User).FirstOrDefaultAsync();

            if (chatMember != null)
            {
                var userResponse = new UserDto
                {
                    AvatarUrl = chatMember.User.AvatarUrl,
                    UserId = chatMember.User.UserId,
                    Username = chatMember.User.Username,
                    Email = chatMember.User.Email,
                    PasswordHash = chatMember.User.PasswordHash,
                    CreatedAt = chatMember.User.CreatedAt,
                    FirstName = chatMember.User.FirstName,
                    LastName = chatMember.User.LastName,
                    LastSeen = chatMember.User.LastSeen
                };

                _context.ChatMembers.Remove(chatMember);
                await _context.SaveChangesAsync();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
                
                await Clients.OthersInGroup(chatId).SendAsync("memberLeave", userResponse);
            }
        }
    }
}
