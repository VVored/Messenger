using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Messenger.SignalR.Models
{
    public class ChatMember
    {
        [Key]
        public int ChatMemberId { get; set; }

        [ForeignKey("Chat")]
        public int ChatId { get; set; }
        public Chat Chat { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Role { get; set; } = "member"; // 'admin', 'member' и т.д.
    }
}
