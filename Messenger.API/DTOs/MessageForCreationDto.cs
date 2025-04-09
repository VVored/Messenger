namespace Messenger.API.DTOs
{
    public class MessageForCreationDto
    {
        public int ChatId { get; set; }
        public string Content { get; set; }
        public List<AttachmentForCreationDto>? Attachments { get; set; }
        public int? RepliableMessageId { get; set; }
    }
}
