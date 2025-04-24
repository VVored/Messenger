using Messenger.API.Data;
using Messenger.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Messenger.API.Controllers
{
    [Route("api/block")]
    [ApiController]
    [Authorize]
    public class BlockedUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BlockedUsersController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet("isblocked/{blockedUserId}")]
        public async Task<IActionResult> IsUserBlocked(int blockedUserId)
        {
            var isUserBlocked = await _context.BlockedUsers.Where(b => b.BlockedId == blockedUserId && b.BlockerId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).AnyAsync();

            return Ok(isUserBlocked);
        }

        [HttpGet("amiblocked/{blockerUserId}")]
        public async Task<IActionResult> AmIBlocked(int blockerUserId)
        {
            var isUserBlocked = await _context.BlockedUsers.Where(b => b.BlockerId == blockerUserId && b.BlockedId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).AnyAsync();

            return Ok(isUserBlocked);
        }

        [HttpPost("{blockedUserId}")]   
        public async Task<IActionResult> BlockUser(int blockedUserId)
        {
            var blocker = await _context.Users.Where(u => u.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();

            if (blocker == null)
            {
                return NotFound();
            }

            var blockedUser = await _context.Users.Where(u => u.UserId == blockedUserId).FirstOrDefaultAsync();

            if (blockedUser == null) 
            {
                return NotFound();
            }

            var block = new BlockedUser {BlockedId = blockedUserId, Blocked = blockedUser, BlockedAt = DateTime.UtcNow, Blocker = blocker, BlockerId = blocker.UserId};

            await _context.BlockedUsers.AddAsync(block);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{blockedUserId}")]
        public async Task<IActionResult> UnblockUser(int blockedUserId)
        {
            var blocker = await _context.Users.Where(u => u.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();

            if (blocker == null)
            {
                return NotFound();
            }

            var blockedUser = await _context.Users.Where(u => u.UserId == blockedUserId).FirstOrDefaultAsync();

            if (blockedUser == null)
            {
                return NotFound();
            }

            var block = await _context.BlockedUsers.Where(b => b.BlockerId == blocker.UserId && b.BlockedId == blockedUserId).FirstOrDefaultAsync();

            if (block == null)
            {
                return NotFound();
            }

            _context.BlockedUsers.Remove(block);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
