namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Teslim edilirken alıcının söylediği kod, teslim alan kişi ve teslim eden personel alınır.
    public class VerifyDeliveryDto
    {
        public Guid CargoId { get; set; }
        public string DeliveryCode { get; set; }
        public string ReceiverName { get; set; }      // Teslim alan kişi
        public Guid PerformedByUserId { get; set; }   // Teslim eden personel (Controller doldurur)
    }
}