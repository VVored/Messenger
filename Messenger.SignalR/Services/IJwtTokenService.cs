namespace Messenger.API.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(string userId, string userName);
    }
}
