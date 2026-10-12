namespace CargoTrack.Entity.Entities.Enums
{
    // Neden: Case'teki sorun nedenleri sabit bir listeden seçilsin, raporlanabilsin.
    public enum DeliveryExceptionReason
    {
        WrongAddress = 1,     // Adres hatalı
        ReceiverNotFound = 2, // Alıcı yok
        DamagedCargo = 3,     // Hasarlı kargo
        Refused = 4           // Reddedildi
    }
}