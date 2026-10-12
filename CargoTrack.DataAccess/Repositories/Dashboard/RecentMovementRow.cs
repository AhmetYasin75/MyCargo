using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DataAccess.Repositories.Dashboard
{
    // Neden: "Son hareketler" listesi için kargonun takip no'su, hareket bilgisi ve işlemi yapanın adı birlikte döner.
    public record RecentMovementRow(
        string TrackCode,
        DateTime MovementDate,
        CargoStatus OldStatus,
        CargoStatus NewStatus,
        string Description,
        string? UserName);
}