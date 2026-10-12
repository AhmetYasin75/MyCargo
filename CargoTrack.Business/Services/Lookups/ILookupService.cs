using CargoTrack.DTO.DTOs.Common;

namespace CargoTrack.Business.Services.Lookups
{
    public interface ILookupService
    {
        Task<List<LookupItemDto>> GetCustomersAsync();
        Task<List<LookupItemDto>> GetBranchesAsync();
        Task<List<LookupItemDto>> GetOriginBranchesAsync();
        Task<List<LookupItemDto>> GetTransferCentersAsync();   // Faz 8C: Durum formundaki merkez listesi
    }
}