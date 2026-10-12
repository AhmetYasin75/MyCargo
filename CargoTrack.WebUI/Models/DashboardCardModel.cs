namespace CargoTrack.WebUI.Models
{
    // Neden: _StatCard partial'ı tek bir sayı kartını çizer. Tüm dashboard'lar aynı kartı kullanır.
    public class DashboardCardModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;

        // Bootstrap renk sınıfı (örn. "text-bg-primary", "text-bg-danger")
        public string CssClass { get; set; } = "text-bg-light";

        // Doluysa kart tıklanınca bu adrese gider (örn. filtrelenmiş kargo listesi)
        public string? Url { get; set; }
    }
}