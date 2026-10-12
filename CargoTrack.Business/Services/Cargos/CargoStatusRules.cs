using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Business.Services.Cargos
{
    // Neden: Durum geçiş kuralları tek yerde dursun. Hem CargoService hem de
    // "hangi aksiyonlar yapılabilir" hesabı aynı tabloyu kullanır.
    public static class CargoStatusRules
    {
        // Neden: "Hangi durumdan hangisine geçilebilir" kuralı tek tabloda durur.
        // Tabloda olmayan geçiş yapılamaz (örn. Created -> Delivered).
        private static readonly Dictionary<CargoStatus, CargoStatus[]> AllowedTransitions = new()
        {
            [CargoStatus.Created] = [CargoStatus.AtOriginBranch],
            [CargoStatus.AtOriginBranch] = [CargoStatus.InTransferCenter, CargoStatus.AtDestinationBranch], // Transfer merkezi atlanabilir
            [CargoStatus.InTransferCenter] = [CargoStatus.InTransferCenter, CargoStatus.AtDestinationBranch], // Birden fazla merkezden geçebilir
            [CargoStatus.AtDestinationBranch] = [CargoStatus.OutForDelivery],
            [CargoStatus.OutForDelivery] = [CargoStatus.Delivered, CargoStatus.DeliveryFailed],
            [CargoStatus.DeliveryFailed] = [CargoStatus.OutForDelivery, CargoStatus.ReturnInProgress],
            [CargoStatus.ReturnInProgress] = [CargoStatus.ReturnedToSender]
            // Delivered ve ReturnedToSender'dan çıkış yok (son durumlar)
        };

        // Neden: Bu durumlara geçerken ek kurallar var (kod doğrulama, hata kaydı, deneme sayısı).
        // Bu yüzden genel ChangeStatusAsync ile geçilemez, sadece özel metotlarla geçilir.
        private static readonly CargoStatus[] OnlyViaDedicatedMethods =
        [
            CargoStatus.Delivered,          // VerifyDeliveryAsync
            CargoStatus.DeliveryFailed,     // ReportDeliveryFailureAsync
            CargoStatus.ReturnInProgress    // ReportDeliveryFailureAsync (3. denemede otomatik)
        ];

        // Neden: "Bu geçiş tabloda var mı?" sorusuna tek cevap veren metot.
        public static bool IsTransitionAllowed(CargoStatus from, CargoStatus to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
        }

        // Neden: Bu duruma sadece özel metotla (teslim doğrula / teslim edilemedi) geçilebilir mi?
        public static bool IsDedicatedOnly(CargoStatus status)
        {
            return OnlyViaDedicatedMethods.Contains(status);
        }

        // Neden: Durum değiştir formundaki dropdown'a sadece genel yolla geçilebilen durumlar konur.
        // (Delivered/DeliveryFailed/ReturnInProgress burada çıkmaz, onların kendi butonu olacak.)
        public static CargoStatus[] GetGeneralNextStatuses(CargoStatus from)
        {
            if (!AllowedTransitions.TryGetValue(from, out var allowed))
            {
                return [];
            }

            return allowed.Where(s => !IsDedicatedOnly(s)).ToArray();
        }
    }
}