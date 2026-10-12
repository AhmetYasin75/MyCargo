namespace CargoTrack.DataAccess.Repositories.Reports
{
    public interface IReportRepository
    {
        // from: dahil, toExclusive: hariç (ikisi de opsiyonel)
        Task<List<BranchReportRow>> GetBranchReportAsync(DateTime? from, DateTime? toExclusive);
        Task<List<EmployeeReportRow>> GetEmployeeReportAsync(DateTime? from, DateTime? toExclusive);
    }
}