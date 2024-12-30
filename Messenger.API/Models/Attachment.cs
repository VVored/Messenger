using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Messenger.API.Models
{
    public class Attachment
    {
        [Key]
        public int AttachmentId { get; set; }

        [ForeignKey("Message")]
        public int MessageId { get; set; }
        public Message Message { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileUrl { get; set; }

        [MaxLength(50)]
        public string FileType { get; set; }

        public int FileSize { get; set; }
    }
}
