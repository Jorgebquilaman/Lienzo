using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Interfaces;

public interface IEmailReaderService
{
    Task<Result<PaginatedResult<EmailMessageSummaryDto>>> GetInboxAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Result<EmailMessageDetailDto>> GetMessageAsync(string uid, CancellationToken ct = default);
    Task<Result<byte[]>> DownloadRawAsync(string uid, CancellationToken ct = default);
    Task<Result<List<EmailProcessedInfoDto>>> GetProcessedEmailsAsync(CancellationToken ct = default);
}
