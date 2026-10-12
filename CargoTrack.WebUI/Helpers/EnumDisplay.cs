using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.WebUI.Helpers
{
    // Neden: Ekranda "AtOriginBranch" yerine Türkçe metin ve duruma uygun renk göstermek için.
    public static class EnumDisplay
    {
        public static string Status(CargoStatus status) => status switch
        {
            CargoStatus.Created => "Oluşturuldu",
            CargoStatus.AtOriginBranch => "Gönderici Şubesinde",
            CargoStatus.InTransferCenter => "Transfer Merkezinde",
            CargoStatus.AtDestinationBranch => "Varış Şubesinde",
            CargoStatus.OutForDelivery => "Dağıtıma Çıktı",
            CargoStatus.Delivered => "Teslim Edildi",
            CargoStatus.DeliveryFailed => "Teslim Edilemedi",
            CargoStatus.ReturnInProgress => "İade Sürecinde",
            CargoStatus.ReturnedToSender => "Göndericiye İade Edildi",
            _ => status.ToString()
        };

        public static string StatusBadge(CargoStatus status) => status switch
        {
            CargoStatus.Created => "bg-secondary",
            CargoStatus.AtOriginBranch => "bg-info text-dark",
            CargoStatus.InTransferCenter => "bg-primary",
            CargoStatus.AtDestinationBranch => "bg-primary",
            CargoStatus.OutForDelivery => "bg-warning text-dark",
            CargoStatus.Delivered => "bg-success",
            CargoStatus.DeliveryFailed => "bg-danger",
            CargoStatus.ReturnInProgress => "bg-danger",
            CargoStatus.ReturnedToSender => "bg-dark",
            _ => "bg-secondary"
        };

        public static string Type(CargoType type) => type switch
        {
            CargoType.Standard => "Standart",
            CargoType.Urgent => "Acil",
            CargoType.Fragile => "Kırılabilir",
            CargoType.Heavy => "Ağır",
            _ => type.ToString()
        };

        public static string Delivery(DeliveryType type) => type switch
        {
            DeliveryType.Standard => "Standart",
            DeliveryType.Express => "Ekspres",
            _ => type.ToString()
        };

        // Neden: "Teslim edilemedi" formundaki neden listesi Türkçe görünsün.
        public static string FailureReason(DeliveryExceptionReason reason) => reason switch
        {
            DeliveryExceptionReason.WrongAddress => "Adres hatalı",
            DeliveryExceptionReason.ReceiverNotFound => "Alıcı yok",
            DeliveryExceptionReason.DamagedCargo => "Hasarlı kargo",
            DeliveryExceptionReason.Refused => "Reddedildi",
            _ => reason.ToString()
        };

        // Neden: Fiyat kuralları ekranında bölge adı Türkçe görünsün.
        public static string Zone(PriceZone zone) => zone switch
        {
            PriceZone.SameCity => "Aynı şehir içi",
            PriceZone.Domestic => "Şehirler arası",
            _ => zone.ToString()
        };

        // Neden: Audit Log ekranında işlem türü Türkçe ve renkli görünsün.
        public static string AuditAction(AuditActionType action) => action switch
        {
            AuditActionType.Create => "Oluşturma",
            AuditActionType.Update => "Güncelleme",
            AuditActionType.Delete => "Silme",
            AuditActionType.StatusChange => "Durum Değişikliği",
            _ => action.ToString()
        };

        public static string AuditBadge(AuditActionType action) => action switch
        {
            AuditActionType.Create => "bg-success",
            AuditActionType.Update => "bg-info text-dark",
            AuditActionType.Delete => "bg-danger",
            AuditActionType.StatusChange => "bg-warning text-dark",
            _ => "bg-secondary"
        };

        // Neden: Audit kayıtlarında varlık adı sınıf adıyla tutulur (Cargo, Branch...). Ekranda Türkçesi gösterilir.
        public static string EntityName(string entityName) => entityName switch
        {
            "Cargo" => "Kargo",
            "Branch" => "Şube",
            "TransferCenter" => "Transfer Merkezi",
            "Employee" => "Personel",
            "CargoPrice" => "Fiyat Kuralı",
            "City" => "Şehir",
            "About" => "Hakkımızda",
            "Delivery" => "Teslimat",
            "DeliveryException" => "Teslimat Hatası",
            "Address" => "Adres",
            "ContactInfo" => "İletişim Bilgisi",
            _ => entityName
        };
    }
}