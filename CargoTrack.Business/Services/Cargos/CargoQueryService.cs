using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.Cargos;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.DTO.DTOs.Common;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using Mapster;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace CargoTrack.Business.Services.Cargos
{
    public class CargoQueryService(
        ICargoRepository _cargoRepository,
        ICargoAccessService _accessService) : ICargoQueryService
    {
        // Neden: URL'den pageSize=100000 yazılıp sistemin yorulmasını önler.
        private const int MaxPageSize = 50;

        public async Task<PagedResult<ResultCargoDto>> GetPagedAsync(CargoFilterDto filter)
        {
            filter ??= new CargoFilterDto();

            // Geçersiz sayfa değerlerini düzelt
            int page = Math.Max(filter.Page, 1);
            int pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

            var filters = new List<Expression<Func<Cargo, bool>>>();

            // 1) Yetki filtresi HER ZAMAN ilk eklenir. Kullanıcı hangi filtreyi seçerse seçsin,
            //    yetkisi dışındaki kargolara ulaşamaz.
            filters.Add(await _accessService.GetVisibilityFilterAsync());

            // 2) Arama: takip no, gönderici/alıcı adı, şube adı
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                filters.Add(c =>
                    c.TrackCode.Contains(search) ||
                    c.Sender.FirstName.Contains(search) ||
                    c.Sender.LastName.Contains(search) ||
                    c.Receiver.FirstName.Contains(search) ||
                    c.Receiver.LastName.Contains(search) ||
                    c.OriginBranch.Name.Contains(search) ||
                    c.DestinationBranch.Name.Contains(search));
            }

            // 3) Durum filtresi
            if (filter.Status is not null)
            {
                var status = filter.Status.Value;
                filters.Add(c => c.CargoStatus == status);
            }

            // 4) Kapsam: aktif (yolda) / geçmiş (tamamlanan) / sorunlu
            if (filter.Scope == CargoListScope.Active)
            {
                filters.Add(c => c.CargoStatus != CargoStatus.Delivered && c.CargoStatus != CargoStatus.ReturnedToSender);
            }
            else if (filter.Scope == CargoListScope.History)
            {
                filters.Add(c => c.CargoStatus == CargoStatus.Delivered || c.CargoStatus == CargoStatus.ReturnedToSender);
            }
            else if (filter.Scope == CargoListScope.Problem)
            {
                // Neden: Tamamlanmış kargolar (teslim edildi / iade edildi) sorun çözülmüş sayılır, listeye girmez.
                // Tamamlanmamış olup teslim edilemeyen, iadeye giden veya daha önce başarısız deneme yapılanlar sorunludur.
                filters.Add(c =>
                    c.CargoStatus != CargoStatus.Delivered &&
                    c.CargoStatus != CargoStatus.ReturnedToSender &&
                    (c.CargoStatus == CargoStatus.DeliveryFailed ||
                     c.CargoStatus == CargoStatus.ReturnInProgress ||
                     c.FailedDeliveryAttempts > 0));
            }

            // 5) Tarih aralığı (bitiş günü dahil olsun diye ertesi günün başlangıcından küçük aranır)
            if (filter.DateFrom is not null)
            {
                var from = filter.DateFrom.Value.Date;
                filters.Add(c => c.ShipmentDate >= from);
            }

            if (filter.DateTo is not null)
            {
                var toExclusive = filter.DateTo.Value.Date.AddDays(1);
                filters.Add(c => c.ShipmentDate < toExclusive);
            }

            var (items, totalCount) = await _cargoRepository.GetPagedAsync(filters, page, pageSize);

            return new PagedResult<ResultCargoDto>
            {
                Items = items.Adapt<List<ResultCargoDto>>(),
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<CargoDetailDto> GetDetailAsync(Guid id)
        {
            var cargo = await _cargoRepository.GetByIdAsync(id);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            // Neden: URL'deki Id değiştirilerek başkasının kargosu açılamasın (Faz 6A kuralı).
            await _accessService.EnsureCanViewAsync(cargo);

            var detail = cargo.Adapt<CargoDetailDto>();

            var movements = await _cargoRepository.GetMovementsByCargoIdAsync(id);
            detail.Timeline = movements.Adapt<List<CargoMovementDto>>();

            return detail;
        }
    }
}