using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Messenger.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;
        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

        [HttpGet("{fileUrl}")]
        public IActionResult GetFile(string fileUrl)
        {
            if (!System.IO.File.Exists("D:/petProjects/Messenger/Messenger.API/Uploads/" + fileUrl))
            {
                return NotFound();
            }
            if (!_fileExtensionContentTypeProvider.TryGetContentType("D:/petProjects/Messenger/Messenger.API/Uploads/" + fileUrl, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = System.IO.File.ReadAllBytes("D:/petProjects/Messenger/Messenger.API/Uploads/" + fileUrl);
            return File(bytes, contentType, fileUrl);
        }
        [HttpPost]
        public IActionResult UpdloadFile(IFormFile file)
        {
            List<string> validExtensions = new List<string>() { ".jpg", ".png", ".gif", ".jpeg", ".JPG", ".PNG", ".GIF", ".JPEG" };
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