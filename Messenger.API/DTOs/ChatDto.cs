using Messenger.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Messenger.API.DTOs
{
    public class ChatDto
    {
        public int ChatId { get; set; }

        public string ChatType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string AvatarUrl { get; set; }
        public List<ChatMemberDto> ChatMembers { get; set; } = new List<ChatMemberDto>();
    }
}
