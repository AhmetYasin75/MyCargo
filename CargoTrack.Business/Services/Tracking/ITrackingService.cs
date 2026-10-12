using CargoTrack.DTO.DTOs.Tracking;

namespace CargoTrack.Business.Services.Tracking
{
    public interface ITrackingService
    {
        // Bulunamazsa null döner.
        Task<TrackingResultDto?> GetByTrackCodeAsync(string? trackCode);
    }
}