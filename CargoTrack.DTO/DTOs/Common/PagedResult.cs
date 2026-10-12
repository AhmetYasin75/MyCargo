namespace CargoTrack.DTO.DTOs.Common
{
    // Neden: Sayfadaki kayıtlar (Items) + sayfalama bilgisi birlikte taşınır.
    public class PagedResult<T> : PagedResultBase
    {
        public List<T> Items { get; set; } = new();
    }
}