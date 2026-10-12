namespace CargoTrack.Entity.Entities.Enums
{
    // Neden: Fiyat şehir/bölgeye göre değişiyor. Şimdilik iki bölge, ileride genişletilebilir.
    public enum PriceZone
    {
        SameCity = 1,   // Aynı şehir içi
        Domestic = 2    // Şehirler arası
    }
}