using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Messenger.SignalR.Models
{
    public class UserStatus
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "offline"; // 'online', 'offline', 'away', 'do_not_disturb'

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
