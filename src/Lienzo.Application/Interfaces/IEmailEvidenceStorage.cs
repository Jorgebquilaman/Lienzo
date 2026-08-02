namespace Lienzo.Application.Interfaces;

public interface IEmailEvidenceStorage
{
    Task<string> SaveAsync(byte[] rawEmail, string fileName, CancellationToken ct = default);
    Task<byte[]?> ReadAsync(string path, CancellationToken ct = default);
}
