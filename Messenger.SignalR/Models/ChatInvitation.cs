using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Messenger.API.Models
{
    public class ChatInvitation
    {
        [Key]
        public int InvitationId { get; set; }

        [ForeignKey("Chat")]
        public int ChatId { get; set; }
        public Chat Chat { get; set; }

        [ForeignKey("Inviter")]
        public int InviterId { get; set; }
        public User Inviter { get; set; }

        [ForeignKey("Invitee")]
        public int InviteeId { get; set; }
        public User Invitee { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // 'pending', 'accepted', 'declined'

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
