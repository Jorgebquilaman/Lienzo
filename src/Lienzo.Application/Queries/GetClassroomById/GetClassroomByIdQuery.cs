using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetClassroomById;

public record GetClassroomByIdQuery(Guid Id) : IRequest<Result<ClassroomDetailDto>>;

public class GetClassroomByIdQueryHandler : IRequestHandler<GetClassroomByIdQuery, Result<ClassroomDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClassroomByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClassroomDetailDto>> Handle(GetClassroomByIdQuery query, CancellationToken cancellationToken)
    {
        var classroom = await _unitOfWork.Classrooms.GetWithReservationsAsync(query.Id);
        if (classroom is null || classroom.IsDeleted)
            return Result<ClassroomDetailDto>.Failure("Classroom not found", "NOT_FOUND");

        var dto = _mapper.Map<ClassroomDetailDto>(classroom);
        return Result<ClassroomDetailDto>.Success(dto);
    }
}
