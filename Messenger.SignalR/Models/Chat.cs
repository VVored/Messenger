using System;
using System.ComponentModel.DataAnnotations;

namespace Messenger.SignalR.Models
{
    public class Chat
    {
        [Key]
        public int ChatId { get; set; }

        [Required]
        [MaxLength(10)]
        public string ChatType { get; set; } // 'private' или 'group'

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
