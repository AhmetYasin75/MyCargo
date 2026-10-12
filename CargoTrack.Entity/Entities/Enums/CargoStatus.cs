namespace CargoTrack.Entity.Entities.Enums
{
    // Neden: Case'teki tüm durum akışı + alternatif akışlar (teslim edilemedi, iade) burada olmalı.
    public enum CargoStatus
    {
        Created = 1,            // Oluşturuldu
        AtOriginBranch = 2,     // Gönderici şubesinde
        InTransferCenter = 3,   // Transfer merkezinde
        AtDestinationBranch = 4,// Varış şubesinde
        OutForDelivery = 5,     // Dağıtıma çıktı
        Delivered = 6,          // Teslim edildi
        DeliveryFailed = 7,     // Teslim edilemedi
        ReturnInProgress = 8,   // İade sürecinde
        ReturnedToSender = 9    // Göndericiye iade edildi
    }
}