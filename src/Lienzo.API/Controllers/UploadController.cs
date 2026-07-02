using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class UploadController : BaseApiController
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 10 * 1024 * 1024;

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, [FromServices] IWebHostEnvironment env)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails { Title = "No se envió ningún archivo.", Status = 400 });

        if (file.Length > MaxFileSize)
            return BadRequest(new ProblemDetails { Title = "El archivo supera los 10 MB.", Status = 400 });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageExtensions.Contains(ext))
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

    [HttpPost("floorplan")]
    public async Task<IActionResult> UploadFloorPlan(IFormFile file, [FromServices] IWebHostEnvironment env)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails { Title = "No se envió ningún archivo.", Status = 400 });

        if (file.Length > MaxFileSize)
            return BadRequest(new ProblemDetails { Title = "El archivo supera los 10 MB.", Status = 400 });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isPdf = ext == ".pdf";

        if (!isPdf && !ImageExtensions.Contains(ext))
            return BadRequest(new ProblemDetails { Title = "Formato no permitido. Usa PDF, JPG, PNG o WebP.", Status = 400 });

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "floorplans");
        Directory.CreateDirectory(uploadsDir);

        if (isPdf)
        {
            var pdfPath = Path.Combine(uploadsDir, $"{Guid.NewGuid()}.pdf");
            await using (var stream = new FileStream(pdfPath, FileMode.Create))
                await file.CopyToAsync(stream);

            var pngName = $"{Path.GetFileNameWithoutExtension(pdfPath)}.png";
            var pngPath = Path.Combine(uploadsDir, pngName);

            try
            {
                var converted = await ConvertPdfToPngAsync(pdfPath, pngPath);
                System.IO.File.Delete(pdfPath);
                if (!converted)
                    return StatusCode(500, new ProblemDetails
                    {
                        Title = "No se pudo convertir el PDF a imagen.",
                        Detail = "Asegúrate de que poppler-utils esté instalado en el servidor.",
                        Status = 500
                    });
            }
            catch
            {
                System.IO.File.Delete(pdfPath);
                throw;
            }

            var url = $"/uploads/floorplans/{pngName}";
            return Ok(url);
        }
        else
        {
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/uploads/floorplans/{fileName}";
            return Ok(url);
        }
    }

    private static async Task<bool> ConvertPdfToPngAsync(string pdfPath, string pngPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "pdftoppm",
            ArgumentList = { "-png", "-r", "150", pdfPath, Path.Combine(Path.GetDirectoryName(pngPath)!, Path.GetFileNameWithoutExtension(pngPath)) },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(cts.Token);

        if (process.ExitCode != 0)
            return false;

        return System.IO.File.Exists(pngPath);
    }
}
