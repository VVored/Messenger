namespace Messenger.API.DTOs
{
    public class PrivateChatDto
    {
        public int ChatId { get; set; }
        public UserDto FirstUser { get; set; }
        public UserDto SecondUser { get; set; }
    }
}
