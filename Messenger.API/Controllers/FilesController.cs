using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpPost]
        public IActionResult UpdloadFile(IFormFile file)
        {
            List<string> validExtensions = new List<string>() { ".jpg", ".png", ".gif"};
            string extension = Path.GetExtension(file.FileName);
            if (!validExtensions.Contains(extension))
            {
                return BadRequest($"Extension is not valid ({string.Join(',', validExtensions)})");
            }
            long size = file.Length;
            if (size > (15 * 1024 * 1024)) {
                return BadRequest("Maximum size can be 15mb");
            }
            string fileName = Guid.NewGuid().ToString() + extension;
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            using FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create);

            file.CopyTo(stream);

            return Ok(fileName);
        }
    }
}