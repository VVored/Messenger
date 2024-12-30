using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Messenger.API.Models
{
    public class BlockedUser
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("Blocker")]
        public int BlockerId { get; set; }
        public User Blocker { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("Blocked")]
        public int BlockedId { get; set; }
        public User Blocked { get; set; }

        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
    }
}
