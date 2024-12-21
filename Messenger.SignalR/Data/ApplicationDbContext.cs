using Messenger.SignalR.Models;
using Microsoft.EntityFrameworkCore;

namespace Messenger.SignalR.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<GroupChatInfo> GroupChatInfos { get; set; }
        public DbSet<BlockedUser> BlockedUsers { get; set; }
        public DbSet<MessageReaction> MessageReactions { get; set; }
        public DbSet<ChatInvitation> ChatInvitations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка составных ключей
            modelBuilder.Entity<BlockedUser>()
                .HasKey(bu => new { bu.BlockerId, bu.BlockedId });
        }
    }
}
