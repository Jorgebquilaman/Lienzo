using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Interfaces;

public interface IReportService
{
    Task<Result<UsageReportResponse>> GetUsageReportAsync(UsageReportFilter filter);
    Task<Result<DemandMetricsResponse>> GetDemandMetricsAsync(DateOnly? fromDate, DateOnly? toDate);
    Task<Result<UsageByProposalResponse>> GetUsageByProposalAsync(UsageByProposalFilter filter);
    Task<Result<DocenteCargaHorariaResponse>> GetDocenteCargaHorariaAsync(DocenteCargaHorariaFilter filter);
    Task<Result<ClassroomTimelineResponse>> GetClassroomTimelineAsync(ClassroomTimelineFilter filter);
}
