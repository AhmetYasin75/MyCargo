namespace CargoTrack.DataAccess.Repositories.Reports
{
    // Neden: Şube bazlı rapor satırı. DB'de gruplanıp sayılmış hâlde gelir.
    public class BranchReportRow
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int SentCount { get; set; }        // Bu şubeden çıkan kargo
        public int ReturnedCount { get; set; }    // Bu şubeden çıkıp iade edilen
        public int ReceivedCount { get; set; }    // Bu şubeye varış adresli kargo
        public int DeliveredCount { get; set; }   // Teslim edilen
        public int OnTimeCount { get; set; }      // Zamanında teslim
        public int DelayedCount { get; set; }     // Geç teslim + hâlâ yolda olup tarihi geçen
        public long DeliveryMinutesSum { get; set; } // Ortalama süre için toplam dakika
    }
}