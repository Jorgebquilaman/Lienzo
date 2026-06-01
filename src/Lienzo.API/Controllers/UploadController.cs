using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class UploadController : BaseApiController
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, [FromServices] IWebHostEnvironment env)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails { Title = "No se envió ningún archivo.", Status = 400 });

        if (file.Length > MaxFileSize)
            return BadRequest(new ProblemDetails { Title = "El archivo supera los 5 MB.", Status = 400 });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new ProblemDetails { Title = "Formato no permitido. Usa JPG, PNG o WebP.", Status = 400 });

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "classrooms");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"/uploads/classrooms/{fileName}";
        return Ok(url);
    }
}
