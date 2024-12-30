using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Messenger.API.Models
{
    public class GroupChatInfo
    {
        [Key]
        [ForeignKey("Chat")]
        public int ChatId { get; set; }
        public Chat Chat { get; set; }

        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string AvatarUrl { get; set; }
    }
}
