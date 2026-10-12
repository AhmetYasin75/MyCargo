namespace CargoTrack.Entity.Entities.Common
{
    // Neden: Rol isimleri her yerde elle string yazılmasın, yazım hatası olmasın.
    // Business katmanı WebUI'daki sabitlere erişemez, o yüzden burada tutuyoruz.
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
    }
}