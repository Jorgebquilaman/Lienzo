using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAnnouncements;

public record GetAnnouncementsQuery : IRequest<Result<List<AnnouncementListItemDto>>>;

public class GetAnnouncementsQueryHandler : IRequestHandler<GetAnnouncementsQuery, Result<List<AnnouncementListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public GetAnnouncementsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<List<AnnouncementListItemDto>>> Handle(GetAnnouncementsQuery query, CancellationToken cancellationToken)
    {
        var announcements = await _unitOfWork.Announcements.GetByStudentIdAsync(_currentUser.UserId);

        var usersResult = await _authService.GetAllUsersAsync();
        var userMap = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<Guid, string>();

        var dtos = announcements.Select(a =>
        {
            var isRead = a.Recipients.Any(r => r.StudentId == _currentUser.UserId && r.IsRead);
            return new AnnouncementListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                Type = a.Type.ToString(),
                CreatedAt = a.CreatedAt,
                IsRead = isRead,
                UserName = userMap.GetValueOrDefault(a.TeacherId),
            };
        }).ToList();
        return Result<List<AnnouncementListItemDto>>.Success(dtos);
    }
}
