namespace CargoTrack.DTO.DTOs.EmployeeDtos
{
    // Neden: Listede kişi bilgisi ve şube adı gösterilir. Mapster User.FirstName -> UserFirstName,
    // Branch.Name -> BranchName gibi alanları otomatik düzleştirir.
    public class ResultEmployeeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
    }
}