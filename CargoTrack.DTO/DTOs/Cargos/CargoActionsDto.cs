using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Details sayfasında hangi butonların/formların gösterileceğini servis belirler.
    // View sadece bu DTO'ya bakar, kendi başına kural yürütmez.
    public class CargoActionsDto
    {
        public Guid CargoId { get; set; }

        // Kullanıcı bu kargo üzerinde işlem yapabilir mi? (Admin veya kargonun şubesindeki Manager)
        public bool CanOperate { get; set; }

        // Durum değiştir formundaki dropdown'a konacak durumlar (teslim/hata/iade hariç)
        public List<CargoStatus> NextStatuses { get; set; } = new();

        // "Teslimat kodunu doğrula" formu gösterilsin mi?
        public bool CanVerifyDelivery { get; set; }

        // "Teslim edilemedi" formu gösterilsin mi?
        public bool CanReportFailure { get; set; }
    }
}