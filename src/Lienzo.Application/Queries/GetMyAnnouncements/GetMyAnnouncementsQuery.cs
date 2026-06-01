using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetMyAnnouncements;

public record GetMyAnnouncementsQuery : IRequest<Result<List<AnnouncementListItemDto>>>;

public class GetMyAnnouncementsQueryHandler : IRequestHandler<GetMyAnnouncementsQuery, Result<List<AnnouncementListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public GetMyAnnouncementsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<List<AnnouncementListItemDto>>> Handle(GetMyAnnouncementsQuery query, CancellationToken cancellationToken)
    {
        var announcements = await _unitOfWork.Announcements.GetByTeacherIdAsync(_currentUser.UserId);

        var usersResult = await _authService.GetAllUsersAsync();
        var userMap = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<Guid, string>();

        var dtos = announcements.Select(a => new AnnouncementListItemDto
        {
            Id = a.Id,
            Title = a.Title,
            Body = a.Body,
            Type = a.Type.ToString(),
            CreatedAt = a.CreatedAt,
            IsRead = false,
            UserName = userMap.GetValueOrDefault(a.TeacherId),
        }).ToList();
        return Result<List<AnnouncementListItemDto>>.Success(dtos);
    }
}
