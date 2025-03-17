using Messenger.API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Messenger.API.DTOs
{
    public class AttachmentDto
    {
        public int AttachmentId { get; set; }

        public int MessageId { get; set; }

        public string FileUrl { get; set; }

        public string FileType { get; set; }

        public int FileSize { get; set; }
    }
}
