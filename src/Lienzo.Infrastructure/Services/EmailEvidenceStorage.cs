using Lienzo.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Lienzo.Infrastructure.Services;

public class EmailEvidenceStorage : IEmailEvidenceStorage
{
    private readonly IWebHostEnvironment _env;

    public EmailEvidenceStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(byte[] rawEmail, string fileName, CancellationToken ct)
    {
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "emails");
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, fileName);
        await File.WriteAllBytesAsync(fullPath, rawEmail, ct);
        return Path.Combine("uploads", "emails", fileName).Replace("\\", "/");
    }

    public async Task<byte[]?> ReadAsync(string path, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", path);
        if (!File.Exists(fullPath))
            return null;
        return await File.ReadAllBytesAsync(fullPath, ct);
    }
}
