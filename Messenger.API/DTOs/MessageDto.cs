using Messenger.API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Messenger.API.DTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public ChatDto ChatDto { get; set; }
        public int SenderId { get; set; }
        public UserDto Sender { get; set; }
        public string Content { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
        public RepliableMessageDto? RepliableMessage { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsEdited { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
    }
}
