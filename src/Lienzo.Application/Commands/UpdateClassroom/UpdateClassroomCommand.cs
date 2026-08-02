using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.UpdateClassroom;

public record UpdateClassroomCommand(Guid Id, UpdateClassroomRequest Request) : IRequest<Result<ClassroomDto>>;

public class UpdateClassroomCommandHandler : IRequestHandler<UpdateClassroomCommand, Result<ClassroomDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateClassroomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClassroomDto>> Handle(UpdateClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await _unitOfWork.Classrooms.GetByIdAsync(command.Id);
        if (classroom is null)
            return Result<ClassroomDto>.Failure("Classroom not found", "NOT_FOUND");

        var request = command.Request;
        var name = request.Name ?? classroom.Name;
        var floor = request.Floor ?? classroom.Floor;
        var capacity = request.Capacity ?? classroom.Capacity;
        var type = request.Type is not null
            ? Enum.Parse<ClassroomType>(request.Type, true)
            : classroom.Type;
        var features = request.Features ?? classroom.Features;

        classroom.UpdateDetails(name, floor, capacity, type, features, request.ImageUrl ?? classroom.ImageUrl);
        _unitOfWork.Classrooms.Update(classroom);

        if (request.AccessoryIds is not null)
        {
            var existing = await _unitOfWork.ClassroomAccessories.Query()
                .Where(ca => ca.ClassroomId == command.Id)
                .ToListAsync(cancellationToken);
            foreach (var item in existing)
                _unitOfWork.ClassroomAccessories.Delete(item);

            var accessoryIds = request.AccessoryIds.Distinct().ToList();
            var validAccessories = await _unitOfWork.Accessories.Query()
                .Where(a => accessoryIds.Contains(a.Id) && a.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var accessory in validAccessories)
                await _unitOfWork.ClassroomAccessories.AddAsync(new ClassroomAccessory(command.Id, accessory.Id));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ClassroomDto>(classroom);
        return Result<ClassroomDto>.Success(dto);
    }
}
