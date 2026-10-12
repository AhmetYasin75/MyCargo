using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.AuditLogs;
using CargoTrack.DTO.DTOs.AuditLogDtos;
using CargoTrack.DTO.DTOs.Common;
using Mapster;

namespace CargoTrack.Business.Services.AuditLogs
{
    // Neden: Audit kayıtlarını sadece Admin görebilir. Kayıtlar AppDbContext tarafından otomatik üretilir,
    // bu serviste ekleme/güncelleme/silme bilinçli olarak yok (denetim kaydı değiştirilemez olmalı).
    public class AuditLogService(
        IAuditLogRepository _repository,
        ICargoAccessService _accessService) : IAuditLogService
    {
        // Neden: URL'den pageSize=100000 yazılıp sistemin yorulmasını önler.
        private const int MaxPageSize = 50;

        public async Task<PagedResult<ResultAuditLogDto>> GetPagedAsync(AuditLogFilterDto filter)
        {
            _accessService.EnsureAdmin();

            filter ??= new AuditLogFilterDto();
            int page = Math.Max(filter.Page, 1);
            int pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

            // Neden: Bitiş günü dahil olsun diye ertesi günün başlangıcından küçük aranır.
            DateTime? from = filter.DateFrom?.Date;
            DateTime? toExclusive = filter.DateTo?.Date.AddDays(1);

            var (rows, totalCount) = await _repository.GetPagedAsync(
                filter.Search?.Trim(), filter.ActionType, filter.EntityName,
                from, toExclusive, page, pageSize);

            var items = rows.Select(r =>
            {
                var dto = r.Log.Adapt<ResultAuditLogDto>();
                dto.UserName = r.UserName;
                return dto;
            }).ToList();

            return new PagedResult<ResultAuditLogDto>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<string>> GetEntityNamesAsync()
        {
            _accessService.EnsureAdmin();
            return await _repository.GetEntityNamesAsync();
        }
    }
}