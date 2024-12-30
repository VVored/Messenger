using Messenger.API.Data;
using Messenger.API.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Messenger.API.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }
    }
}
