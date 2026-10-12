namespace CargoTrack.Entity.Entities.Enums
{
    // Neden: Audit Log'da "ne tür işlem yapıldı" bilgisi sabit değerlerden gelsin.
    public enum AuditActionType
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        StatusChange = 4
    }
}