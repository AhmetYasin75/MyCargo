namespace CargoTrack.WebUI.Models
{
    // Neden: _BarList partial'ındaki bir çubuk satırı (etiket + değer).
    public class BarItemModel
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }

        // Çubuğun Bootstrap renk sınıfı (örn. "bg-primary")
        public string CssClass { get; set; } = "bg-primary";
    }
}