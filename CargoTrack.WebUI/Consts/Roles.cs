using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.WebUI.Consts
{
    // Neden: Rol isimleri RoleNames'den gelir, tek kaynak olur (Business katmanı da aynısını kullanıyor).
    public static class Roles
    {
        public const string Admin = RoleNames.Admin;
        public const string Manager = RoleNames.Manager;
        public const string User = RoleNames.User;
    }
}