namespace Messenger.API.DTOs
{
    public class RepliableMessageDto
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public UserDto Sender { get; set; }
        public string Content { get; set; }
    }
}
