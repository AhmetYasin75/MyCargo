namespace CargoTrack.DTO.DTOs.EmployeeDtos
{
    // Neden: Sadece ünvan ve şube değiştirilebilir. Ad/soyad formda sadece bilgi olarak gösterilir.
    public class UpdateEmployeeDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public Guid BranchId { get; set; }

        // Sadece gösterim için (formda hidden olarak taşınır, güncellemede kullanılmaz)
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
    }
}