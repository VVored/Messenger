using System.ComponentModel.DataAnnotations;

namespace Messenger.API.DTOs
{
    public class ChatForCreationDto
    {
        [MaxLength(10)]
        public string ChatType { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string AvatarUrl { get; set; }
    }
}
