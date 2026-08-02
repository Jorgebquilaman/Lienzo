using System.Text;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Lienzo.Infrastructure.Services;

public class EmailReaderService : IEmailReaderService
{
    private readonly ISystemSettingService _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public EmailReaderService(ISystemSettingService settings, IUnitOfWork unitOfWork, IAuthService authService)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    private async Task<(string Host, int Port, string Username, string Password)> GetImapConfigAsync()
    {
        var host = await _settings.GetValueAsync("EmailImapHost") ?? "imap.gmail.com";
        var portStr = await _settings.GetValueAsync("EmailImapPort") ?? "993";
        var username = await _settings.GetValueAsync("EmailUsername");
        var password = await _settings.GetValueAsync("EmailPassword");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("La configuración de email IMAP está incompleta. Verifica EmailUsername y EmailPassword en Configuración.");

        if (!int.TryParse(portStr, out var port))
            port = 993;

        return (host, port, username, password);
    }

    private static async Task<ImapClient> ConnectAsync(string host, int port, string username, string password, FolderAccess access)
    {
        var client = new ImapClient();
        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(username, password);
        await client.Inbox.OpenAsync(access);
        return client;
    }

    private static string FormatAddress(MailboxAddress address)
    {
        if (string.IsNullOrEmpty(address?.Address))
            return "";
        return string.IsNullOrEmpty(address.Name) ? address.Address : $"{address.Name} <{address.Address}>";
    }

    private static string GetSnippet(string? text, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        var normalized = text.Replace("\r", " ").Replace("\n", " ");
        if (normalized.Length <= maxLength)
            return normalized;
        return normalized[..maxLength] + "...";
    }

    public async Task<Result<PaginatedResult<EmailMessageSummaryDto>>> GetInboxAsync(int page, int pageSize, CancellationToken ct)
    {
        try
        {
            var (host, port, username, password) = await GetImapConfigAsync();
            using var client = await ConnectAsync(host, port, username, password, FolderAccess.ReadOnly);

            var allUids = await client.Inbox.SearchAsync(SearchQuery.All, ct);
            allUids = allUids.Reverse().ToList();
            var total = allUids.Count;
            var skip = (page - 1) * pageSize;
            var pageUids = allUids.Skip(skip).Take(pageSize).ToList();

            if (pageUids.Count == 0)
                return Result<PaginatedResult<EmailMessageSummaryDto>>.Success(
                    PaginatedResult<EmailMessageSummaryDto>.Success([], total, page, pageSize));

            var summaries = await client.Inbox.FetchAsync(pageUids,
                MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags |
                MessageSummaryItems.BodyStructure | MessageSummaryItems.PreviewText, ct);

            var processedUids = await GetProcessedUidSetAsync(ct);

            var items = summaries.Select(m =>
            {
                var sender = m.Envelope?.From?.Mailboxes.FirstOrDefault();
                return new EmailMessageSummaryDto(
                    m.UniqueId.Id.ToString(),
                    sender?.Address ?? "",
                    FormatAddress(sender),
                    m.Envelope?.Subject ?? "",
                    m.Envelope?.Date ?? DateTimeOffset.MinValue,
                    m.Attachments.Any(),
                    m.Flags.HasValue && m.Flags.Value.HasFlag(MessageFlags.Seen),
                    processedUids.Contains(m.UniqueId.Id.ToString()),
                    GetSnippet(m.PreviewText));
            }).ToList();

            return Result<PaginatedResult<EmailMessageSummaryDto>>.Success(
                PaginatedResult<EmailMessageSummaryDto>.Success(items, total, page, pageSize));
        }
        catch (Exception ex)
        {
            return Result<PaginatedResult<EmailMessageSummaryDto>>.Failure($"No se pudo conectar a la bandeja de correo: {ex.Message}");
        }
    }

    public async Task<Result<EmailMessageDetailDto>> GetMessageAsync(string uid, CancellationToken ct)
    {
        try
        {
            var (host, port, username, password) = await GetImapConfigAsync();
            using var client = await ConnectAsync(host, port, username, password, FolderAccess.ReadOnly);

            if (!UniqueId.TryParse(uid, out var uniqueId))
                return Result<EmailMessageDetailDto>.Failure("UID de email inválido.");

            var message = await client.Inbox.GetMessageAsync(uniqueId, ct);
            var summary = await client.Inbox.FetchAsync(new[] { uniqueId },
                MessageSummaryItems.UniqueId | MessageSummaryItems.Flags | MessageSummaryItems.BodyStructure, ct);

            var processedUids = await GetProcessedUidSetAsync(ct);

            var attachments = message.Attachments
                .Select(a => new EmailAttachmentDto(
                    a.ContentType?.Name ?? (a.ContentDisposition?.FileName ?? "adjunto"),
                    a.ContentType?.MimeType ?? "",
                    GetAttachmentSize(a)))
                .ToList();

            var sender = message.From.Mailboxes.FirstOrDefault();

            var reservation = await _unitOfWork.Reservations.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.SourceEmailUid == uid && !r.IsDeleted, ct);

            return Result<EmailMessageDetailDto>.Success(new EmailMessageDetailDto(
                uid,
                sender?.Address ?? "",
                FormatAddress(sender),
                message.Subject ?? "",
                message.Date,
                attachments.Count > 0,
                summary.FirstOrDefault()?.Flags is { } flags && flags.HasFlag(MessageFlags.Seen),
                processedUids.Contains(uid),
                message.TextBody,
                message.HtmlBody,
                attachments,
                reservation?.Id,
                reservation?.RequiresAccessoryConfirmation ?? false,
                reservation?.AccessoriesConfirmedAt.HasValue ?? false));
        }
        catch (Exception ex)
        {
            return Result<EmailMessageDetailDto>.Failure($"No se pudo obtener el email: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> DownloadRawAsync(string uid, CancellationToken ct)
    {
        try
        {
            var (host, port, username, password) = await GetImapConfigAsync();
            using var client = await ConnectAsync(host, port, username, password, FolderAccess.ReadOnly);

            if (!UniqueId.TryParse(uid, out var uniqueId))
                return Result<byte[]>.Failure("UID de email inválido.");

            var message = await client.Inbox.GetMessageAsync(uniqueId, ct);
            using var stream = new MemoryStream();
            await message.WriteToAsync(stream, ct);
            return Result<byte[]>.Success(stream.ToArray());
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"No se pudo descargar el email: {ex.Message}");
        }
    }

    public async Task<Result<List<EmailProcessedInfoDto>>> GetProcessedEmailsAsync(CancellationToken ct)
    {
        var processed = await _unitOfWork.ProcessedEmails.Query()
            .Where(p => !p.IsDeleted)
            .ToListAsync(ct);

        var usersResult = await _authService.GetAllUsersAsync();
        var userNames = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<Guid, string>();

        var items = new List<EmailProcessedInfoDto>();
        foreach (var p in processed)
        {
            string? reservationTitle = null;
            var reservation = await _unitOfWork.Reservations.Query()
                .FirstOrDefaultAsync(r => r.Id == p.ReservationId && !r.IsDeleted, ct);
            if (reservation is not null)
                reservationTitle = reservation.Title;

            items.Add(new EmailProcessedInfoDto(
                p.EmailUid,
                p.ReservationId,
                reservationTitle,
                p.ProcessedAt,
                userNames.TryGetValue(p.ProcessedByUserId, out var name) ? name : null));
        }

        return Result<List<EmailProcessedInfoDto>>.Success(items);
    }

    private async Task<HashSet<string>> GetProcessedUidSetAsync(CancellationToken ct)
    {
        var processed = await _unitOfWork.ProcessedEmails.Query()
            .Where(p => !p.IsDeleted)
            .Select(p => p.EmailUid)
            .ToListAsync(ct);
        return processed.ToHashSet();
    }

    private static long GetAttachmentSize(MimeEntity entity)
    {
        try
        {
            using var stream = new MemoryStream();
            entity.WriteTo(stream);
            return stream.Length;
        }
        catch
        {
            return 0;
        }
    }
}
