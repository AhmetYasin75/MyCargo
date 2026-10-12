using CargoTrack.DataAccess.Repositories.Cargos;
using CargoTrack.DTO.DTOs.Tracking;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Business.Services.Tracking
{
    public class TrackingService : ITrackingService
    {
        private readonly ICargoRepository _cargoRepository;

        public TrackingService(ICargoRepository cargoRepository)
        {
            _cargoRepository = cargoRepository;
        }

        public async Task<TrackingResultDto?> GetByTrackCodeAsync(string? trackCode)
        {
            // Neden: Kullanıcı boşluklu/küçük harfle yazabilir. Uzun girdiler sorguya hiç gitmez.
            var code = trackCode?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(code) || code.Length > 30)
            {
                return null;
            }

            var cargo = await _cargoRepository.GetByTrackCodeAsync(code);
            if (cargo is null)
            {
                return null;
            }

            var movements = (await _cargoRepository.GetMovementsByCargoIdAsync(cargo.Id))
                .OrderBy(m => m.MovementDate)
                .Select(m => new TrackingMovementDto
                {
                    Status = m.NewStatus,
                    MovementDate = m.MovementDate,
                    Location = m.Branch?.Name ?? m.TransferCenter?.Name ?? string.Empty
                })
                .ToList();

            var completed = cargo.CargoStatus == CargoStatus.Delivered
                            || cargo.CargoStatus == CargoStatus.ReturnedToSender;

            return new TrackingResultDto
            {
                TrackCode = cargo.TrackCode,
                Status = cargo.CargoStatus,
                ShipmentDate = cargo.ShipmentDate,
                EstimatedArrivalDate = cargo.EstimatedArrivalDate,
                DeliveredDate = cargo.DeliveredDate,
                IsOverdue = !completed && cargo.EstimatedArrivalDate < DateTime.Now,
                OriginCity = cargo.OriginBranch.City.Name,
                DestinationCity = cargo.DestinationBranch.City.Name,
                OriginBranch = cargo.OriginBranch.Name,
                DestinationBranch = cargo.DestinationBranch.Name,
                CargoType = cargo.CargoType,
                DeliveryType = cargo.DeliveryType,
                Weight = cargo.Weight,
                Desi = cargo.Desi,
                ProgressPercent = ProgressOf(cargo.CargoStatus),
                LastLocation = movements.LastOrDefault(m => m.Location != string.Empty)?.Location,
                Movements = movements
            };
        }

        // Neden: İlerleme çubuğu durumdan türetilir; ayrı bir alan tutmaya gerek yok.
        private static int ProgressOf(CargoStatus status) => status switch
        {
            CargoStatus.Created => 10,
            CargoStatus.AtOriginBranch => 25,
            CargoStatus.InTransferCenter => 45,
            CargoStatus.AtDestinationBranch => 65,
            CargoStatus.OutForDelivery => 85,
            CargoStatus.DeliveryFailed => 85,
            CargoStatus.ReturnInProgress => 85,
            CargoStatus.Delivered => 100,
            CargoStatus.ReturnedToSender => 100,
            _ => 0
        };
    }
}