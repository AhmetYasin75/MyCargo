namespace CargoTrack.Entity.Entities.Common
{
    public class BaseEntity
    {
        public Guid Id { get; set; }

        // Neden: Kayıt ne zaman oluştu / güncellendi bilgisi her tabloda lazım (case zorunlu kılıyor).
        // Değerleri DbContext otomatik dolduracak, elle atamayacağız.
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Neden: Soft Delete. Kayıt fiziksel silinmez, sadece "silindi" işaretlenir.
        public bool IsDeleted { get; set; }

        public BaseEntity()
        {
            Id = Guid.NewGuid();
        }
    }
}