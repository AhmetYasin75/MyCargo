namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: "Aktif kargolarım" ve "geçmiş teslimatlarım" ayrımı (case'teki User ekranları).
    public enum CargoListScope
    {
        All = 0,      // Hepsi
        Active = 1,   // Yolda olanlar (teslim edilmemiş, iade tamamlanmamış)
        History = 2,  // Tamamlananlar (teslim edilmiş veya göndericiye iade edilmiş)
        Problem = 3   // Sorunlular (teslim edilemedi, iade sürecinde veya başarısız denemesi olan, henüz tamamlanmamış)
    }
}