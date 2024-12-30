using System.ComponentModel.DataAnnotations;

namespace Messenger.API.DTOs
{
    public class ChatDto
    {
        public int ChatId { get; set; }

        public string ChatType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
