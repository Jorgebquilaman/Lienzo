using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateMaintenanceBlock;

public record CreateMaintenanceBlockCommand(CreateMaintenanceBlockRequest Request) : IRequest<Result<MaintenanceBlockDto>>;

public class CreateMaintenanceBlockCommandHandler : IRequestHandler<CreateMaintenanceBlockCommand, Result<MaintenanceBlockDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public CreateMaintenanceBlockCommandHandler(
        IUnitOfWork unitOfWork, IMapper mapper,
        ICurrentUserService currentUser, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<Result<MaintenanceBlockDto>> Handle(CreateMaintenanceBlockCommand command, CancellationToken ct)
    {
        var req = command.Request;

        if (req.StartTime >= req.EndTime)
            return Result<MaintenanceBlockDto>.Failure("Start time must be before end time", "VALIDATION");

        if (string.IsNullOrWhiteSpace(req.Reason))
            return Result<MaintenanceBlockDto>.Failure("Reason is required", "VALIDATION");

        var classroom = (await _unitOfWork.Classrooms.GetAllAsync())
            .FirstOrDefault(c => c.Id == req.ClassroomId);
        if (classroom is null)
            return Result<MaintenanceBlockDto>.Failure("Classroom not found", "NOT_FOUND");

        var block = new MaintenanceBlock(req.ClassroomId, req.StartTime.ToUniversalTime(), req.EndTime.ToUniversalTime(), req.Reason, _currentUser.UserId);
        await _unitOfWork.MaintenanceBlocks.AddAsync(block);

        // Auto-cancel overlapping reservations and notify affected users
        var blockDate = DateOnly.FromDateTime(req.StartTime);
        var blockStart = TimeOnly.FromDateTime(req.StartTime);
        var blockEnd = TimeOnly.FromDateTime(req.EndTime);

        var overlapping = (await _unitOfWork.Reservations.GetByDateRangeAsync(blockDate, blockDate, classroomId: req.ClassroomId))
            .Where(r => r.Status == ReservationStatus.Approved
                && r.StartTime < blockEnd
                && r.EndTime > blockStart)
            .ToList();

        foreach (var reservation in overlapping)
        {
            try
            {
                reservation.Cancel();
                _unitOfWork.Reservations.Update(reservation);

                await _notificationService.SendAsync(
                    reservation.UserId,
                    "Reserva cancelada por mantenimiento",
                    $"Tu reserva del {reservation.Date:dd/MM/yyyy} ({reservation.StartTime:hh\\:mm}-{reservation.EndTime:hh\\:mm}) en {classroom.Name} fue cancelada por mantenimiento: {req.Reason}",
                    "Error",
                    reservation.Id,
                    "Reservation");
            }
            catch (InvalidOperationException)
            {
                // skip if cannot be cancelled
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = new MaintenanceBlockDto(
            block.Id, block.ClassroomId, classroom.Name,
            classroom.Building?.Name,
            block.StartTime, block.EndTime, block.Reason,
            _currentUser.Email, block.CreatedAt);

        return Result<MaintenanceBlockDto>.Success(dto);
    }
}
