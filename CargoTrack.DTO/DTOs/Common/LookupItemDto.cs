namespace CargoTrack.DTO.DTOs.Common
{
    // Neden: Açılır listelerde (dropdown) gösterilecek "Id + yazı" çifti. Listeler için ortak model.
    public class LookupItemDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
    }
}