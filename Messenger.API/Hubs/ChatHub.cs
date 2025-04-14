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
        private readonly static List<UserDto> _connections = new List<UserDto>();
        private readonly static Dictionary<string, string> _connectionsMap = new Dictionary<string, string>();

        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task SendNotificationSound(string groupName)
        {
            await Clients.OthersInGroup(groupName).SendAsync("notificationSound");
        }

        public async Task JoinBasicGroups()
        {
            await _context.ChatMembers.Where(cm => cm.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).ForEachAsync(async cm => { await Groups.RemoveFromGroupAsync(Context.ConnectionId, cm.ChatId + "Notification"); Console.WriteLine(cm.ChatId); });
            await _context.ChatMembers.Where(cm => cm.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).ForEachAsync(async cm => { await Groups.AddToGroupAsync(Context.ConnectionId, cm.ChatId + "Notification"); Console.WriteLine(cm.ChatId); });
        }

        public async Task JoinGroup(string chatId)
        {
            /*            var user = await _context.Users.Where(u => u.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();
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
                            };*/

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            /*
                            await Clients.OthersInGroup(chatId).SendAsync("newMember", userResponse);
                        }*/
        }
        public async Task LeaveGroup(string chatId)
        {
            /*            var chatMember = await _context.ChatMembers.Where(cm => cm.ChatId == int.Parse(chatId) && cm.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).Include(cm => cm.User).FirstOrDefaultAsync();

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
            */
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);

            /*                await Clients.OthersInGroup(chatId).SendAsync("memberLeave", userResponse);
                        }*/
        }
        public async Task JoinChat(string chatId)
        {
            var chat = await _context.Chats.Where(c => c.ChatId == int.Parse(chatId)).FirstOrDefaultAsync();
            var user = await _context.Users.Where(u => u.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();

            if (chat != null && user != null)
            {
                var chatMember = new ChatMember
                {
                    Chat = chat,
                    ChatId = int.Parse(chatId),
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

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId + "Notification");
                await Clients.Client(Context.ConnectionId).SendAsync("userJoinChat", response);
            }
        }
        public async Task LeaveChat(string chatId)
        {
            var chat = await _context.Chats.Include(c => c.Members).FirstOrDefaultAsync(c => c.ChatId == int.Parse(chatId));
            var member = await _context.ChatMembers.Include(cm => cm.User).FirstOrDefaultAsync(cm => cm.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value) & cm.ChatId == int.Parse(chatId));


            if (chat != null && member != null)
            {
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

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId + "Notification");
                await Clients.Client(Context.ConnectionId).SendAsync("leaveChatMember", response);
            }
        }

        public async Task CreatePrivateChat(string secondUserId)
        {
            var createdChat = new Chat { ChatType = "private" };
            var firstUser = _context.Users.Where(u => u.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefault();
            var firstChatMember = new ChatMember { Chat = createdChat, ChatId = createdChat.ChatId, User = firstUser, Role = "Admin", UserId = firstUser.UserId, JoinedAt = createdChat.CreatedAt };
            var secondUser = _context.Users.Where(u => u.UserId == int.Parse(secondUserId)).FirstOrDefault();
            var secondChatMember = new ChatMember { User = secondUser, UserId = secondUser.UserId, Chat = createdChat, ChatId = createdChat.ChatId, JoinedAt = createdChat.CreatedAt, Role = "Admin" };
            var chatInfo = new GroupChatInfo { AvatarUrl = "", Chat = createdChat, ChatId = createdChat.ChatId, Description = "", GroupName = firstUser.UserId + " " + secondUser.UserId };

            if (firstUser != null && secondUser != null)
            {
                await _context.Chats.AddAsync(createdChat);
                await _context.SaveChangesAsync();
                await _context.ChatMembers.AddAsync(firstChatMember);
                await _context.SaveChangesAsync();
                await _context.ChatMembers.AddAsync(secondChatMember);
                await _context.SaveChangesAsync();
                await _context.GroupChatInfos.AddAsync(chatInfo);
                await _context.SaveChangesAsync();

                var response = new ChatDto
                {
                    Description = "",
                    AvatarUrl = "",
                    ChatId = createdChat.ChatId,
                    ChatType = createdChat.ChatType,
                    CreatedAt = createdChat.CreatedAt,
                    GroupName = firstUser.UserId + " " + secondUser.UserId,
                    ChatMembers = new List<ChatMemberDto>
                        {
                            new ChatMemberDto
                            {
                                JoinedAt = firstChatMember.JoinedAt,
                                Role = firstChatMember.Role,
                                User = new UserDto
                                {
                                    AvatarUrl = firstUser.AvatarUrl,
                                    CreatedAt = firstUser.CreatedAt,
                                    Email = firstUser.Email,
                                    FirstName = firstUser.FirstName,
                                    LastName = firstUser.LastName,
                                    LastSeen = firstUser.LastSeen,
                                    PasswordHash = firstUser.PasswordHash,
                                    UserId = firstUser.UserId,
                                    Username = firstUser.Username,
                                }
                            },
                            new ChatMemberDto
                            {
                                JoinedAt = secondChatMember.JoinedAt,
                                Role = secondChatMember.Role,
                                User = new UserDto
                                {
                                    AvatarUrl = secondUser.AvatarUrl,
                                    CreatedAt = secondUser.CreatedAt,
                                    Email = secondUser.Email,
                                    FirstName = secondUser.FirstName,
                                    LastName = secondUser.LastName,
                                    LastSeen = secondUser.LastSeen,
                                    PasswordHash = secondUser.PasswordHash,
                                    UserId = secondUser.UserId,
                                    Username = secondUser.Username,
                                }
                            }
                        }
                };
                await Clients.Caller.SendAsync("createAndSetPrivateChat", response);
                await Groups.AddToGroupAsync(Context.ConnectionId, response.ChatId + "Notification");
                if (_connectionsMap.TryGetValue(secondUserId, out var secondUserConnectionId))
                {
                    await Groups.AddToGroupAsync(secondUserConnectionId, response.ChatId + "Notification");
                    await Clients.Client(secondUserConnectionId).SendAsync("createPrivateChat", response);
                }
            }
        }
        public override Task OnConnectedAsync()
        {
            var user = _context.Users.Where(u => u.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefault();
            var userDto = new UserDto
            {
                UserId = user.UserId,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LastSeen = user.LastSeen,
                PasswordHash = user.PasswordHash,
                Username = user.Username
            };

            if (!_connections.Any(u => u.UserId == userDto.UserId))
            {
                _connections.Add(userDto);
                _connectionsMap.Add(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value, Context.ConnectionId);
            }

            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _connections.Where(u => u.UserId == int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value)).First();

            _connections.Remove(user);

            _connectionsMap.Remove(user.UserId.ToString());
            return base.OnDisconnectedAsync(exception);
        }
    }
}
