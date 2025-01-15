using Messenger.API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Messenger.API.DTOs
{
    public class MessageReactionDto
    {
        public int ReactionId { get; set; }

        public int MessageId { get; set; }
        public MessageDto Message { get; set; }

        public int UserId { get; set; }
        public UserDto User { get; set; }
        public string Reaction { get; set; }

        public DateTime ReactedAt { get; set; }
    }
}
