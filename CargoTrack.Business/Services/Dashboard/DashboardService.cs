using CargoTrack.Business.Services.Employees;
using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.Branches;
using CargoTrack.DataAccess.Repositories.Cargos;
using CargoTrack.DataAccess.Repositories.Dashboard;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.DTO.DTOs.DashboardDtos;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;
using Mapster;
using System.Linq.Expressions;

namespace CargoTrack.Business.Services.Dashboard
{
    // Neden: Dashboard hesapları (aktif, geciken, oran...) iş kuralıdır, Controller'da değil burada.
    public class DashboardService(
        IDashboardRepository _repository,
        ICargoRepository _cargoRepository,
        ICargoAccessService _accessService,
        IUserContext _userContext,
        IEmployeeService _employeeService,
        IBranchRepository _branchRepository) : IDashboardService
    {
        // Neden: "Son 7 gün", "en yoğun 5 şube", "son 10 hareket", "son 5 aktif kargo" sayıları sabitte, değişirse tek yerden değişir.
        private const int TrafficDays = 7;
        private const int TopBranchCount = 5;
        private const int RecentMovementCount = 10;
        private const int ActiveCargoListCount = 5;

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            _accessService.EnsureAdmin();

            Expression<Func<Cargo, bool>> all = c => true;
            var now = DateTime.Now;

            var statusCounts = await _repository.GetStatusCountsAsync(all);

            int total = statusCounts.Values.Sum();
            int delivered = statusCounts.GetValueOrDefault(CargoStatus.Delivered);
            int returned = statusCounts.GetValueOrDefault(CargoStatus.ReturnedToSender);

            // Aktif = tamamlanmamış (teslim edilmemiş ve iadesi bitmemiş)
            int active = total - delivered - returned;

            // Geciken = aktif ve tahmini teslim tarihi geçmiş
            int overdue = await _repository.CountCargosAsync(c =>
                c.CargoStatus != CargoStatus.Delivered &&
                c.CargoStatus != CargoStatus.ReturnedToSender &&
                c.EstimatedArrivalDate < now);

            var dto = new AdminDashboardDto
            {
                TotalCargoCount = total,
                ActiveCargoCount = active,
                OverdueCargoCount = overdue,
                DeliveredCargoCount = delivered,
                DeliveryRatePercent = total == 0 ? 0 : Math.Round(delivered * 100.0 / total, 1),
                ActiveBranchCount = await _repository.CountBranchesAsync(),
                EmployeeCount = await _repository.CountEmployeesAsync()
            };

            // Son 7 gün: kargo olmayan günler 0 olarak da gösterilsin diye her gün için satır üretilir
            var fromDate = now.Date.AddDays(-(TrafficDays - 1));
            var daily = await _repository.GetDailyShipmentCountsAsync(all, fromDate);

            for (int i = 0; i < TrafficDays; i++)
            {
                var day = fromDate.AddDays(i);
                dto.Last7Days.Add(new DailyCountDto
                {
                    Date = day,
                    Count = daily.GetValueOrDefault(day)
                });
            }

            // Durum dağılımı: enum sırasıyla, kargosu olan durumlar
            dto.StatusDistribution = Enum.GetValues<CargoStatus>()
                .Select(s => new StatusCountDto { Status = s, Count = statusCounts.GetValueOrDefault(s) })
                .Where(x => x.Count > 0)
                .ToList();

            var topBranches = await _repository.GetTopBranchesAsync(TopBranchCount);
            dto.TopBranches = topBranches
                .Select(b => new BranchLoadDto { BranchName = b.BranchName, CargoCount = b.CargoCount })
                .ToList();

            return dto;
        }

        public async Task<ManagerDashboardDto> GetManagerDashboardAsync()
        {
            // Neden: Sadece Manager kendi şubesinin panelini görür. Şube, giriş yapan kullanıcıdan bulunur
            // (adresten veya formdan gelmez), böylece başka şubenin verisine ulaşılamaz.
            var userId = _userContext.UserId
                ?? throw new UnauthorizedAccessException("Giriş yapmanız gerekiyor.");

            if (!_userContext.IsInRole(RoleNames.Manager))
            {
                throw new UnauthorizedAccessException("Bu panel sadece şube yöneticilerine açıktır.");
            }

            var branchId = await _employeeService.GetBranchIdAsync(userId)
                ?? throw new UnauthorizedAccessException("Herhangi bir şubeye bağlı değilsiniz.");

            var branch = await _branchRepository.GetByIdAsync(branchId);

            var now = DateTime.Now;
            var todayStart = now.Date;
            var tomorrowStart = todayStart.AddDays(1);

            // Şubenin gördüğü kargolar: çıkış veya varış şubesi bu şube (CargoAccessService ile aynı kural)
            Expression<Func<Cargo, bool>> branchCargos = c =>
                c.OriginBranchId == branchId || c.DestinationBranchId == branchId;

            var dto = new ManagerDashboardDto
            {
                BranchName = branch?.Name ?? string.Empty,

                // Bugün gelen: bugün "Varış Şubesinde" durumuna geçen ve varış şubesi bu şube olan kargolar
                TodayIncomingCount = await _repository.CountMovementCargosAsync(m =>
                    m.NewStatus == CargoStatus.AtDestinationBranch &&
                    m.MovementDate >= todayStart && m.MovementDate < tomorrowStart &&
                    m.Cargo.DestinationBranchId == branchId),

                // Bugün çıkan: bugün "Gönderici Şubesinde" durumundan ayrılan ve çıkış şubesi bu şube olan kargolar
                TodayOutgoingCount = await _repository.CountMovementCargosAsync(m =>
                    m.OldStatus == CargoStatus.AtOriginBranch &&
                    m.NewStatus != CargoStatus.AtOriginBranch &&
                    m.MovementDate >= todayStart && m.MovementDate < tomorrowStart &&
                    m.Cargo.OriginBranchId == branchId),

                OutForDeliveryCount = await _repository.CountCargosAsync(c =>
                    c.DestinationBranchId == branchId &&
                    c.CargoStatus == CargoStatus.OutForDelivery),

                DeliveredTodayCount = await _repository.CountCargosAsync(c =>
                    c.DestinationBranchId == branchId &&
                    c.CargoStatus == CargoStatus.Delivered &&
                    c.DeliveredDate >= todayStart),

                // Geciken: şubenin gördüğü, tamamlanmamış ve tahmini teslim tarihi geçmiş kargolar
                OverdueCount = await _repository.CountCargosAsync(c =>
                    (c.OriginBranchId == branchId || c.DestinationBranchId == branchId) &&
                    c.CargoStatus != CargoStatus.Delivered &&
                    c.CargoStatus != CargoStatus.ReturnedToSender &&
                    c.EstimatedArrivalDate < now),

                // Sorunlu: liste ekranındaki "Sorunlu" kapsamıyla aynı tanım
                ProblemCount = await _repository.CountCargosAsync(c =>
                    (c.OriginBranchId == branchId || c.DestinationBranchId == branchId) &&
                    c.CargoStatus != CargoStatus.Delivered &&
                    c.CargoStatus != CargoStatus.ReturnedToSender &&
                    (c.CargoStatus == CargoStatus.DeliveryFailed ||
                     c.CargoStatus == CargoStatus.ReturnInProgress ||
                     c.FailedDeliveryAttempts > 0))
            };

            var movements = await _repository.GetRecentMovementsAsync(branchCargos, RecentMovementCount);
            dto.RecentMovements = movements
                .Select(m => new RecentMovementDto
                {
                    TrackCode = m.TrackCode,
                    MovementDate = m.MovementDate,
                    OldStatus = m.OldStatus,
                    NewStatus = m.NewStatus,
                    Description = m.Description,
                    UserName = m.UserName
                })
                .ToList();

            return dto;
        }

        public async Task<UserDashboardDto> GetUserDashboardAsync()
        {
            // Neden: Müşteri sadece KENDİ kargolarını görür. Kullanıcı, giriş yapan kişiden bulunur
            // (adresten veya formdan gelmez), başkasının kargosuna ulaşılamaz.
            var userId = _userContext.UserId
                ?? throw new UnauthorizedAccessException("Giriş yapmanız gerekiyor.");

            if (!_userContext.IsInRole(RoleNames.User))
            {
                throw new UnauthorizedAccessException("Bu panel sadece müşterilere açıktır.");
            }

            // Kullanıcının kargoları: gönderen veya alıcı olduğu (CargoAccessService ile aynı kural)
            Expression<Func<Cargo, bool>> mine = c => c.SenderId == userId || c.ReceiverId == userId;

            Expression<Func<Cargo, bool>> active = c =>
                c.CargoStatus != CargoStatus.Delivered &&
                c.CargoStatus != CargoStatus.ReturnedToSender;

            var statusCounts = await _repository.GetStatusCountsAsync(mine);
            int total = statusCounts.Values.Sum();
            int history = statusCounts.GetValueOrDefault(CargoStatus.Delivered)
                        + statusCounts.GetValueOrDefault(CargoStatus.ReturnedToSender);

            var dto = new UserDashboardDto
            {
                ActiveCargoCount = total - history,
                HistoryCargoCount = history
            };

            // Son 5 aktif kargo (en yeni üstte)
            var (activeItems, _) = await _cargoRepository.GetPagedAsync([mine, active], 1, ActiveCargoListCount);
            dto.ActiveCargos = activeItems.Select(c => ToSummary(c, userId)).ToList();

            // Son kargo ve hareket zaman çizelgesi
            var (lastItems, _) = await _cargoRepository.GetPagedAsync([mine], 1, 1);
            var last = lastItems.FirstOrDefault();

            if (last is not null)
            {
                dto.LastCargo = ToSummary(last, userId);

                var movements = await _cargoRepository.GetMovementsByCargoIdAsync(last.Id);
                dto.LastCargoTimeline = movements.Adapt<List<CargoMovementDto>>();
            }

            return dto;
        }

        // Neden: Kargo -> özet dönüşümü iki yerde (aktif liste, son kargo) lazım. Teslimat kodu kuralı da burada,
        // tek yerde: kod sadece alıcıya ve sadece dağıtımdayken verilir.
        private static UserCargoSummaryDto ToSummary(Cargo cargo, Guid userId)
        {
            bool isSender = cargo.SenderId == userId;
            bool isReceiver = cargo.ReceiverId == userId;

            var other = isSender ? cargo.Receiver : cargo.Sender;

            string? deliveryCode = null;
            if (isReceiver && cargo.CargoStatus == CargoStatus.OutForDelivery && cargo.Delivery is not null)
            {
                deliveryCode = cargo.Delivery.DeliveryCode;
            }

            return new UserCargoSummaryDto
            {
                Id = cargo.Id,
                TrackCode = cargo.TrackCode,
                CargoStatus = cargo.CargoStatus,
                EstimatedArrivalDate = cargo.EstimatedArrivalDate,
                OriginBranchName = cargo.OriginBranch?.Name ?? string.Empty,
                DestinationBranchName = cargo.DestinationBranch?.Name ?? string.Empty,
                IsSender = isSender,
                OtherPartyName = $"{other?.FirstName} {other?.LastName}".Trim(),
                DeliveryCode = deliveryCode
            };
        }
    }
}