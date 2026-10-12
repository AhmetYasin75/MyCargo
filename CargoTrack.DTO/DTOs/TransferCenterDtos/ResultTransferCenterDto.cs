namespace CargoTrack.DTO.DTOs.TransferCenterDtos
{
    // Neden: Listede şehir adı da gösterilir. Mapster City.Name alanını CityName'e otomatik düzleştirir.
    public class ResultTransferCenterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
    }
}