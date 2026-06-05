using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lienzo.Application.Commands.CreateClassroom;

public record CreateClassroomCommand(CreateClassroomRequest Request) : IRequest<Result<ClassroomDto>>;

public class CreateClassroomCommandHandler : IRequestHandler<CreateClassroomCommand, Result<ClassroomDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateClassroomCommandHandler> _logger;

    public CreateClassroomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateClassroomCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ClassroomDto>> Handle(CreateClassroomCommand command, CancellationToken cancellationToken)
    {
        var r = command.Request;
        _logger.LogWarning("CreateClassroom request: Name={Name}, BuildingId={BuildingId}, Floor={Floor}, Capacity={Capacity}, Type={Type}, Features={Features}, ImageUrl={ImageUrl}",
            r.Name, r.BuildingId, r.Floor, r.Capacity, r.Type, r.Features is not null ? string.Join(",", r.Features) : "null", r.ImageUrl);

        var building = await _unitOfWork.Buildings.GetByIdAsync(command.Request.BuildingId);
        if (building is null)
            return Result<ClassroomDto>.Failure("Building not found", "NOT_FOUND");

        if (!Enum.TryParse<ClassroomType>(command.Request.Type, true, out var type))
            return Result<ClassroomDto>.Failure("Invalid classroom type", "INVALID_TYPE");

        var classroom = new Classroom(
            command.Request.Name,
            command.Request.BuildingId,
            command.Request.Floor,
            command.Request.Capacity,
            type,
            command.Request.Features,
            command.Request.ImageUrl);

        await _unitOfWork.Classrooms.AddAsync(classroom);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ClassroomDto>(classroom);
        dto = dto with { BuildingName = building.Name };

        return Result<ClassroomDto>.Success(dto);
    }
}
