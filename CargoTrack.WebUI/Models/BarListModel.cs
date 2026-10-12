namespace CargoTrack.WebUI.Models
{
    // Neden: Son 7 gün, durum dağılımı, en yoğun şubeler gibi "etiket + sayı" listeleri tek partial ile çizilir.
    public class BarListModel
    {
        public string Title { get; set; } = string.Empty;
        public List<BarItemModel> Items { get; set; } = new();
        public string EmptyText { get; set; } = "Veri yok.";
    }
}