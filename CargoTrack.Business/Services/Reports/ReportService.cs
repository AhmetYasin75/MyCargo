using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.Reports;
using CargoTrack.DTO.DTOs.ReportDtos;

namespace CargoTrack.Business.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ICargoAccessService _accessService;

        public ReportService(IReportRepository reportRepository, ICargoAccessService accessService)
        {
            _reportRepository = reportRepository;
            _accessService = accessService;
        }

        public async Task<ReportDto> GetReportAsync(ReportFilterDto filter)
        {
            // Neden: Raporlar tüm şubeleri kapsar, sadece Admin görebilir.
            _accessService.EnsureAdmin();

            var from = filter.From?.Date;
            var to = filter.To?.Date;

            // Neden: Tarihler ters girilirse hata vermek yerine yerlerini değiştiriyoruz.
            if (from.HasValue && to.HasValue && from > to)
            {
                (from, to) = (to, from);
            }

            // Neden: Bitiş günü dahil olsun diye repository'ye "ertesi gün 00:00 (hariç)" gönderilir.
            DateTime? toExclusive = to?.AddDays(1);

            var branchRows = await _reportRepository.GetBranchReportAsync(from, toExclusive);
            var employeeRows = await _reportRepository.GetEmployeeReportAsync(from, toExclusive);

            var branches = branchRows.Select(r => new BranchReportDto
            {
                BranchName = r.BranchName,
                SentCount = r.SentCount,
                ReceivedCount = r.ReceivedCount,
                DeliveredCount = r.DeliveredCount,
                DelayedCount = r.DelayedCount,
                ReturnedCount = r.ReturnedCount,
                AvgDeliveryHours = r.DeliveredCount == 0 ? 0 : Math.Round(r.DeliveryMinutesSum / 60.0 / r.DeliveredCount, 1),
                DeliveryRatePercent = Percent(r.DeliveredCount, r.ReceivedCount),
                OnTimeRatePercent = Percent(r.OnTimeCount, r.DeliveredCount)
            }).ToList();

            var employees = employeeRows.Select(r => new EmployeeReportDto
            {
                FullName = r.FullName,
                BranchName = r.BranchName,
                DeliveredCount = r.DeliveredCount,
                FailedCount = r.FailedCount,
                TotalOperations = r.TotalOperations,
                TodayOperations = r.TodayOperations,
                SuccessRatePercent = Percent(r.DeliveredCount, r.DeliveredCount + r.FailedCount)
            }).ToList();

            // Neden: Genel toplamlar şube satırlarından türetilir (her kargo varış tarafında bir kez sayılır).
            var totalCargo = branchRows.Sum(r => r.ReceivedCount);
            var totalDelivered = branchRows.Sum(r => r.DeliveredCount);
            var totalOnTime = branchRows.Sum(r => r.OnTimeCount);
            var totalReturned = branchRows.Sum(r => r.ReturnedCount);
            var totalMinutes = branchRows.Sum(r => r.DeliveryMinutesSum);

            return new ReportDto
            {
                From = from,
                To = to,
                TotalCargoCount = totalCargo,
                DeliveredCount = totalDelivered,
                AvgDeliveryHours = totalDelivered == 0 ? 0 : Math.Round(totalMinutes / 60.0 / totalDelivered, 1),
                OnTimeRatePercent = Percent(totalOnTime, totalDelivered),
                ReturnRatePercent = Percent(totalReturned, totalCargo),
                MostSendingBranch = branches.Where(b => b.SentCount > 0).OrderByDescending(b => b.SentCount).Select(b => b.BranchName).FirstOrDefault(),
                MostDeliveringBranch = branches.Where(b => b.DeliveredCount > 0).OrderByDescending(b => b.DeliveredCount).Select(b => b.BranchName).FirstOrDefault(),
                Branches = branches.OrderByDescending(b => b.SentCount + b.ReceivedCount).ToList(),
                MostDelayedBranches = branches.Where(b => b.DelayedCount > 0).OrderByDescending(b => b.DelayedCount).Take(5).ToList(),
                Employees = employees
            };
        }

        // Neden: Sıfıra bölmeyi tek yerde engeller; veri yoksa %0 döner.
        private static double Percent(int part, int total)
        {
            return total == 0 ? 0 : Math.Round(part * 100.0 / total, 1);
        }
    }
}