using CargoTrack.DTO.DTOs.Common;
using CargoTrack.DTO.DTOs.TransferCenterDtos;

namespace CargoTrack.Business.Services.TransferCenters
{
    public interface ITransferCenterService
    {
        Task<PagedResult<ResultTransferCenterDto>> GetPagedAsync(TransferCenterFilterDto filter);
        Task<UpdateTransferCenterDto> GetByIdAsync(Guid id);
        Task CreateAsync(CreateTransferCenterDto dto);
        Task UpdateAsync(UpdateTransferCenterDto dto);
        Task DeleteAsync(Guid id);
    }
}