using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.MarkAnnouncementAsRead;

public record MarkAnnouncementAsReadCommand(Guid AnnouncementId) : IRequest<Result<bool>>;

public class MarkAnnouncementAsReadCommandHandler : IRequestHandler<MarkAnnouncementAsReadCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkAnnouncementAsReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(MarkAnnouncementAsReadCommand command, CancellationToken cancellationToken)
    {
        await _unitOfWork.Announcements.MarkAsReadAsync(command.AnnouncementId, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
