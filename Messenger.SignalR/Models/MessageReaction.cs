using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Messenger.SignalR.Models
{
    public class MessageReaction
    {
        [Key]
        public int ReactionId { get; set; }

        [ForeignKey("Message")]
        public int MessageId { get; set; }
        public Message Message { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [MaxLength(50)]
        public string Reaction { get; set; }

        public DateTime ReactedAt { get; set; } = DateTime.UtcNow;
    }
}
