using CargoTrack.DTO.DTOs.ReportDtos;

namespace CargoTrack.Business.Services.Reports
{
    public interface IReportService
    {
        Task<ReportDto> GetReportAsync(ReportFilterDto filter);
    }
}