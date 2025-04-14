namespace Messenger.API.DTOs
{
    public class LastMessageDto
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int ChatId { get; set; }
        public UserDto Sender { get; set; }
        public string Content { get; set; }
    }
}
