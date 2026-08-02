using Lienzo.Application.DTOs;

namespace Lienzo.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, EmailAttachment? attachment = null);
}