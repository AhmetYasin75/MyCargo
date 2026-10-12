namespace CargoTrack.DTO.DTOs.TransferCenterDtos
{
    // Neden: Name string? çünkü boş kontrolünü Türkçe mesajlı FluentValidation yapar (MVC'nin İngilizce zorunlu mesajı çıkmasın).
    public class CreateTransferCenterDto
    {
        public string? Name { get; set; }
        public Guid CityId { get; set; }
    }
}