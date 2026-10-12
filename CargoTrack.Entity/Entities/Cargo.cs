using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Entity.Entities;

public class Cargo : BaseEntity
{
    public string TrackCode { get; set; }
    public DateTime ShipmentDate { get; set; }
    public DateTime EstimatedArrivalDate { get; set; }   // Case'teki EstimatedDeliveryDate
    public DateTime? DeliveredDate { get; set; }          // Neden: "Zamanında teslimat oranı" hesabı için gerçek teslim tarihi lazım
    public double Weight { get; set; }

    // Neden: Desi = En x Boy x Yükseklik / 3000 formülü için ölçüler saklanmalı
    public double Width { get; set; }
    public double Length { get; set; }
    public double Height { get; set; }
    public double Desi { get; set; }

    public decimal Price { get; set; }           // Neden: Hesaplanan ücret kargoda saklanır, fiyat kuralı sonradan değişse de eski kargo etkilenmez
    public bool HasInsurance { get; set; }       // Ek hizmet örneği
    public int FailedDeliveryAttempts { get; set; } // Neden: 3 başarısız denemede iade başlayacak

    public CargoType CargoType { get; set; }
    public DeliveryType DeliveryType { get; set; }
    public CargoStatus CargoStatus { get; set; }

    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid OriginBranchId { get; set; }
    public Guid DestinationBranchId { get; set; }

    //Navigation Properties
    public virtual AppUser Sender { get; set; }
    public virtual AppUser Receiver { get; set; }
    public virtual Branch OriginBranch { get; set; }
    public virtual Branch DestinationBranch { get; set; }

    public virtual IList<CargoMovement> Movements { get; set; }            // Bire-çok: hareket geçmişi
    public virtual Delivery Delivery { get; set; }                         // Bire-bir: teslimat kodu bilgisi
    public virtual IList<DeliveryException> DeliveryExceptions { get; set; }
}