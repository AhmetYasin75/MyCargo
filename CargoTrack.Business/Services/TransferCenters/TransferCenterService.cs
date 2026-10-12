using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.TransferCenters;
using CargoTrack.DTO.DTOs.Common;
using CargoTrack.DTO.DTOs.TransferCenterDtos;
using CargoTrack.Entity.Entities;
using Mapster;
using System.ComponentModel.DataAnnotations;

namespace CargoTrack.Business.Services.TransferCenters
{
    // Neden: Her domain için özel servis (case kuralı). Yetki kontrolü de burada: Controller'daki [Authorize]
    // atlansa bile servis Admin olmayana işlem yaptırmaz.
    public class TransferCenterService(
        ITransferCenterRepository _repository,
        ICargoAccessService _accessService) : ITransferCenterService
    {
        // Neden: URL'den pageSize=100000 yazılıp sistemin yorulmasını önler.
        private const int MaxPageSize = 50;

        public async Task<PagedResult<ResultTransferCenterDto>> GetPagedAsync(TransferCenterFilterDto filter)
        {
            _accessService.EnsureAdmin();

            filter ??= new TransferCenterFilterDto();
            int page = Math.Max(filter.Page, 1);
            int pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

            var (items, totalCount) = await _repository.GetPagedAsync(filter.Search?.Trim(), page, pageSize);

            return new PagedResult<ResultTransferCenterDto>
            {
                Items = items.Adapt<List<ResultTransferCenterDto>>(),
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UpdateTransferCenterDto> GetByIdAsync(Guid id)
        {
            _accessService.EnsureAdmin();

            var center = await _repository.GetByIdAsync(id);
            if (center is null)
            {
                throw new ValidationException("Transfer merkezi bulunamadı.");
            }

            return center.Adapt<UpdateTransferCenterDto>();
        }

        public async Task CreateAsync(CreateTransferCenterDto dto)
        {
            _accessService.EnsureAdmin();

            // Neden: Validator boş olmadığını garanti etti, burada baştaki/sondaki boşlukları temizliyoruz.
            var name = dto.Name!.Trim();

            if (await _repository.NameExistsAsync(name))
            {
                throw new ValidationException("Bu isimde bir transfer merkezi zaten var.");
            }

            var center = dto.Adapt<TransferCenter>();
            center.Name = name;

            await _repository.CreateAsync(center);
        }

        public async Task UpdateAsync(UpdateTransferCenterDto dto)
        {
            _accessService.EnsureAdmin();

            // Neden: Yeni nesne üretmiyoruz, mevcut kaydı çekip sadece alanları değiştiriyoruz (CreatedDate korunur).
            var center = await _repository.GetByIdAsync(dto.Id);
            if (center is null)
            {
                throw new ValidationException("Transfer merkezi bulunamadı.");
            }

            var name = dto.Name!.Trim();

            if (await _repository.NameExistsAsync(name, dto.Id))
            {
                throw new ValidationException("Bu isimde bir transfer merkezi zaten var.");
            }

            center.Name = name;
            center.CityId = dto.CityId;

            await _repository.UpdateAsync(center);
        }

        public async Task DeleteAsync(Guid id)
        {
            _accessService.EnsureAdmin();

            var center = await _repository.GetByIdAsync(id);
            if (center is null)
            {
                throw new ValidationException("Transfer merkezi bulunamadı.");
            }

            // Neden: Case "hareket geçmişi silinmemeli" diyor. Bir kargo bu merkezden geçtiyse
            // merkez silinirse geçmişte adı kaybolur.
            if (await _repository.HasMovementsAsync(id))
            {
                throw new ValidationException("Bu transfer merkezi kargo hareket geçmişinde kullanıldığı için silinemez.");
            }

            await _repository.DeleteAsync(center);
        }
    }
}