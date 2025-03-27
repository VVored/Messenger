using Messenger.API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Messenger.API.DTOs
{
    public class ChatMemberDto
    {
        public UserDto User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public string Role { get; set; } = "member"; // 'admin', 'member' и т.д.
    }
}
