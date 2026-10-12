using CargoTrack.Business.Services.Pricing;
using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.Cargos;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using Mapster;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace CargoTrack.Business.Services.Cargos
{
    public class CargoService(
        ICargoRepository _cargoRepository,
        ICargoPricingService _pricingService,
        ICargoAccessService _accessService) : ICargoService
    {
        // Neden: Case "üç başarısız deneme" diyor. Sayıyı sabitte tutuyoruz, değişirse tek yerden değişir.
        private const int MaxDeliveryAttempts = 3;

        public async Task CreateAsync(CreateCargoDto createCargoDto)
        {
            // Faz 6: Yetki kontrolü. Manager sadece kendi şubesinden kargo oluşturabilir.
            await _accessService.EnsureCanCreateAsync(createCargoDto.OriginBranchId);

            // Neden: Formdan gelen kullanıcı bilgisine güvenmeyiz (değiştirilebilir).
            // Giriş yapan kullanıcıyı biz belirleriz.
            createCargoDto.PerformedByUserId = _accessService.CurrentUserId;

            // Neden: Mapster dönüşümü Controller'da değil serviste (case kuralı).
            var cargo = createCargoDto.Adapt<Cargo>();

            // Neden: Bu alanları kullanıcı belirlememeli, iş kuralı olarak servis atar.
            cargo.TrackCode = await GenerateTrackCodeAsync();
            cargo.CargoStatus = CargoStatus.Created;
            cargo.ShipmentDate = DateTime.Now;
            cargo.Desi = CalculateDesi(cargo.Width, cargo.Length, cargo.Height);

            // Faz 4: Fiyat ve tahmini teslim tarihi otomatik hesaplanır
            var pricing = await _pricingService.CalculateAsync(
                cargo.Weight, cargo.Desi, cargo.DeliveryType,
                cargo.OriginBranchId, cargo.DestinationBranchId, cargo.HasInsurance);

            cargo.Price = pricing.Price;
            cargo.EstimatedArrivalDate = CalculateEstimatedArrival(cargo.ShipmentDate, cargo.DeliveryType, pricing.Zone);

            // Neden: İlk hareket kaydı kargo ile birlikte eklenir (tek SaveChanges, ayrı kayıt hatası olmaz).
            cargo.Movements = new List<CargoMovement>
            {
                new CargoMovement
                {
                    OldStatus = CargoStatus.Created,
                    NewStatus = CargoStatus.Created,
                    MovementDate = DateTime.Now,
                    Description = "Kargo oluşturuldu.",
                    BranchId = cargo.OriginBranchId,
                    PerformedByUserId = createCargoDto.PerformedByUserId
                }
            };

            await _cargoRepository.CreateAsync(cargo);
        }

        public async Task DeleteAsync(Guid id)
        {
            // Faz 6: Silme sadece Admin'e açık
            _accessService.EnsureAdmin();

            var cargo = await _cargoRepository.GetByIdAsync(id);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            // Neden: Repository'deki Remove artık Soft Delete yapıyor (IsDeleted = true).
            await _cargoRepository.DeleteAsync(cargo);
        }

        public async Task<List<ResultCargoDto>> GetAllAsync()
        {
            // Faz 6: Kullanıcının rolüne göre filtre veritabanı sorgusuna eklenir
            var filter = await _accessService.GetVisibilityFilterAsync();
            var cargos = await _cargoRepository.GetAllWithDetailsAsync(filter);
            return cargos.Adapt<List<ResultCargoDto>>();
        }

        public async Task<UpdateCargoDto> GetByIdAsync(Guid id)
        {
            var cargo = await _cargoRepository.GetByIdAsync(id);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            // Faz 6: Bu metot düzenleme formunu doldurur, bu yüzden "işlem yetkisi" aranır
            await _accessService.EnsureCanOperateAsync(cargo);

            return cargo.Adapt<UpdateCargoDto>();
        }

        // Neden: Herkese açık takip için giriş gerekmez. Faz 10'da kişisel veri içermeyen DTO'ya çevrilecek.
        public async Task<ResultCargoDto> GetByTrackCodeAsync(string trackCode)
        {
            var cargo = await _cargoRepository.GetByTrackCodeAsync(trackCode);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            return cargo.Adapt<ResultCargoDto>();
        }

        public async Task UpdateAsync(UpdateCargoDto updateCargoDto)
        {
            // Neden: Branch'teki gibi DTO'dan yeni nesne üretmiyoruz. Kargoda TrackCode, durum, fiyat gibi
            // DTO'da olmayan alanlar var; yeni nesne üretirsek bunlar boşalır. Mevcut kaydı çekip
            // sadece DTO'daki alanları üzerine yazıyoruz.
            var cargo = await _cargoRepository.GetByIdAsync(updateCargoDto.Id);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            // Faz 6: Yetki kontrolü
            await _accessService.EnsureCanOperateAsync(cargo);

            // Faz 4 kuralı: Kargo çıkış şubesinden ayrıldıktan sonra bilgileri değiştirilemez.
            if (cargo.CargoStatus != CargoStatus.Created && cargo.CargoStatus != CargoStatus.AtOriginBranch)
            {
                throw new ValidationException("Kargo yola çıktıktan sonra güncellenemez.");
            }

            updateCargoDto.Adapt(cargo);
            cargo.Desi = CalculateDesi(cargo.Width, cargo.Length, cargo.Height);

            // Neden: Ağırlık, ölçü, teslimat tipi veya varış şubesi değiştiyse fiyat ve tarih de değişir.
            var pricing = await _pricingService.CalculateAsync(
                cargo.Weight, cargo.Desi, cargo.DeliveryType,
                cargo.OriginBranchId, cargo.DestinationBranchId, cargo.HasInsurance);

            cargo.Price = pricing.Price;
            cargo.EstimatedArrivalDate = CalculateEstimatedArrival(cargo.ShipmentDate, cargo.DeliveryType, pricing.Zone);

            await _cargoRepository.UpdateAsync(cargo);
        }

        // ---------------- FAZ 3: DURUM GEÇİŞİ ----------------

        public async Task ChangeStatusAsync(ChangeCargoStatusDto dto)
        {
            var cargo = await _cargoRepository.GetByIdAsync(dto.CargoId);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            // Faz 6: Yetki kontrolü ve işlemi yapan kullanıcının güvenli şekilde belirlenmesi
            await _accessService.EnsureCanOperateAsync(cargo);
            dto.PerformedByUserId = _accessService.CurrentUserId;

            var oldStatus = cargo.CargoStatus;

            // Kural 1: Bu geçiş izinli mi? (Tablo CargoStatusRules'ta)
            if (!CargoStatusRules.IsTransitionAllowed(oldStatus, dto.NewStatus))
            {
                throw new ValidationException($"'{oldStatus}' durumundan '{dto.NewStatus}' durumuna geçilemez.");
            }

            // Kural 2: Teslim/hata/iade-başlatma durumlarına sadece özel metotlarla geçilir
            if (CargoStatusRules.IsDedicatedOnly(dto.NewStatus))
            {
                throw new ValidationException("Bu duruma geçiş için teslimat doğrulama veya teslimat başarısız işlemi kullanılmalıdır.");
            }

            // Kural 3: Transfer merkezine geçerken hangi merkez olduğu zorunlu
            if (dto.NewStatus == CargoStatus.InTransferCenter && dto.TransferCenterId is null)
            {
                throw new ValidationException("Transfer merkezi seçilmelidir.");
            }

            // Faz 5 - Kural 4: Dağıtıma çıkarken 6 haneli teslimat kodu üretilir.
            // Yeniden dağıtımda (teslim edilemedi sonrası) yeni kod üretilir, eski kod geçersiz olur.
            Delivery delivery = null;
            if (dto.NewStatus == CargoStatus.OutForDelivery)
            {
                delivery = cargo.Delivery ?? new Delivery { CargoId = cargo.Id };
                delivery.DeliveryCode = GenerateDeliveryCode();
                delivery.CodeCreatedDate = DateTime.Now;
                delivery.IsVerified = false;
                delivery.VerifiedDate = null;
                delivery.ReceiverName = null;
                delivery.DeliveredByUserId = null;
            }

            cargo.CargoStatus = dto.NewStatus;

            var movement = CreateMovement(
                cargo, oldStatus, dto.NewStatus, dto.Description,
                dto.PerformedByUserId, DateTime.Now,
                dto.NewStatus == CargoStatus.InTransferCenter ? dto.TransferCenterId : null);

            await _cargoRepository.SaveOperationAsync(cargo, [movement], delivery);
        }

        public async Task<List<CargoMovementDto>> GetMovementsAsync(Guid cargoId)
        {
            // Faz 6: Hareket geçmişini sadece o kargoyu görmeye yetkili kullanıcı görebilir
            var cargo = await _cargoRepository.GetByIdAsync(cargoId);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            await _accessService.EnsureCanViewAsync(cargo);

            var movements = await _cargoRepository.GetMovementsByCargoIdAsync(cargoId);
            return movements.Adapt<List<CargoMovementDto>>();
        }

        // ---------------- FAZ 8C: MÜMKÜN İŞLEMLER ----------------

        // Neden: Details sayfası hangi form/butonları göstereceğini buradan öğrenir.
        // Not: Bu sadece GÖSTERİM içindir. Asıl güvenlik, işlem metotlarındaki kontrollerdir.
        public async Task<CargoActionsDto> GetAvailableActionsAsync(Guid cargoId)
        {
            var cargo = await _cargoRepository.GetByIdAsync(cargoId);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            // Önce kargoyu görme yetkisi (başkasının kargosu için aksiyon bilgisi de verilmez)
            await _accessService.EnsureCanViewAsync(cargo);

            var actions = new CargoActionsDto
            {
                CargoId = cargo.Id,
                CanOperate = await _accessService.CanOperateAsync(cargo)
            };

            // Yetkisi yoksa (örn. User) hiçbir aksiyon açılmaz
            if (!actions.CanOperate)
            {
                return actions;
            }

            actions.NextStatuses = CargoStatusRules.GetGeneralNextStatuses(cargo.CargoStatus).ToList();

            // Teslim doğrulama ve "teslim edilemedi" sadece dağıtımdaki kargo için
            var isOutForDelivery = cargo.CargoStatus == CargoStatus.OutForDelivery;
            actions.CanVerifyDelivery = isOutForDelivery;
            actions.CanReportFailure = isOutForDelivery;

            return actions;
        }

        // ---------------- FAZ 5: TESLİMAT ----------------

        // Teslimat kodu doğrulanırsa kargo "Teslim Edildi" olur.
        public async Task VerifyDeliveryAsync(VerifyDeliveryDto dto)
        {
            var cargo = await _cargoRepository.GetByIdAsync(dto.CargoId);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            // Faz 6: Yetki kontrolü ve teslim eden personelin güvenli şekilde belirlenmesi
            await _accessService.EnsureCanOperateAsync(cargo);
            dto.PerformedByUserId = _accessService.CurrentUserId;

            // Kural 1: Sadece dağıtımdaki kargo teslim edilebilir
            if (cargo.CargoStatus != CargoStatus.OutForDelivery)
            {
                throw new ValidationException("Sadece dağıtımdaki kargo teslim edilebilir.");
            }

            // Kural 2: Kod doğrulanmadan teslim edilemez (case'in ana kuralı)
            var delivery = cargo.Delivery;
            if (delivery is null || delivery.DeliveryCode != dto.DeliveryCode?.Trim())
            {
                throw new ValidationException("Teslimat kodu hatalı.");
            }

            var now = DateTime.Now;

            // Teslim alan kişi ve teslim eden personel kaydedilir
            delivery.IsVerified = true;
            delivery.VerifiedDate = now;
            delivery.ReceiverName = dto.ReceiverName;
            delivery.DeliveredByUserId = dto.PerformedByUserId;

            var oldStatus = cargo.CargoStatus;
            cargo.CargoStatus = CargoStatus.Delivered;
            cargo.DeliveredDate = now;   // "Zamanında teslimat oranı" (Faz 9) bu tarihi kullanacak

            var movement = CreateMovement(
                cargo, oldStatus, CargoStatus.Delivered,
                $"Kargo teslim edildi. Teslim alan: {dto.ReceiverName}",
                dto.PerformedByUserId, now);

            await _cargoRepository.SaveOperationAsync(cargo, [movement], delivery);
        }

        // Teslim edilemezse neden kaydedilir. 3. başarısız denemede iade süreci otomatik başlar.
        public async Task ReportDeliveryFailureAsync(ReportDeliveryFailureDto dto)
        {
            var cargo = await _cargoRepository.GetByIdAsync(dto.CargoId);
            if (cargo is null)
            {
                throw new ValidationException("Cargo Not Found");
            }

            // Faz 6: Yetki kontrolü ve işlemi yapanın güvenli şekilde belirlenmesi
            await _accessService.EnsureCanOperateAsync(cargo);
            dto.PerformedByUserId = _accessService.CurrentUserId;

            if (cargo.CargoStatus != CargoStatus.OutForDelivery)
            {
                throw new ValidationException("Sadece dağıtımdaki kargo için teslimat başarısız bildirilebilir.");
            }

            var now = DateTime.Now;
            var oldStatus = cargo.CargoStatus;

            cargo.FailedDeliveryAttempts++;
            cargo.CargoStatus = CargoStatus.DeliveryFailed;

            // Hata kaydı: neden, açıklama, tarih
            var deliveryException = new DeliveryException
            {
                CargoId = cargo.Id,
                Reason = dto.Reason,
                Description = dto.Description,
                OccurredDate = now,
                ReportedByUserId = dto.PerformedByUserId
            };

            var movements = new List<CargoMovement>
            {
                CreateMovement(
                    cargo, oldStatus, CargoStatus.DeliveryFailed,
                    $"Teslim edilemedi ({dto.Reason}). Deneme: {cargo.FailedDeliveryAttempts}/{MaxDeliveryAttempts}. {dto.Description}",
                    dto.PerformedByUserId, now)
            };

            // Kural: 3 başarısız denemeden sonra iade süreci başlar
            if (cargo.FailedDeliveryAttempts >= MaxDeliveryAttempts)
            {
                cargo.CargoStatus = CargoStatus.ReturnInProgress;

                // Neden: İki kaydın tarihi aynı olmasın ki zaman çizelgesinde sıra karışmasın
                movements.Add(CreateMovement(
                    cargo, CargoStatus.DeliveryFailed, CargoStatus.ReturnInProgress,
                    $"{MaxDeliveryAttempts} başarısız teslimat denemesi tamamlandı, iade süreci başlatıldı.",
                    dto.PerformedByUserId, now.AddMilliseconds(1)));
            }

            await _cargoRepository.SaveOperationAsync(cargo, movements, deliveryException: deliveryException);
        }

        // ---------------- YARDIMCI METOTLAR ----------------

        // Neden: Hareket kaydı oluşturma 3 yerde tekrarlanıyordu, tek yere topladık.
        private static CargoMovement CreateMovement(
            Cargo cargo, CargoStatus oldStatus, CargoStatus newStatus, string description,
            Guid performedByUserId, DateTime movementDate, Guid? transferCenterId = null)
        {
            return new CargoMovement
            {
                CargoId = cargo.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                MovementDate = movementDate,
                Description = description,
                BranchId = ResolveBranchId(cargo, newStatus),
                TransferCenterId = transferCenterId,
                PerformedByUserId = performedByUserId
            };
        }

        // Neden: Şubeyi kullanıcı seçmez, duruma göre servis belirler.
        private static Guid? ResolveBranchId(Cargo cargo, CargoStatus status)
        {
            return status switch
            {
                CargoStatus.AtOriginBranch => cargo.OriginBranchId,
                CargoStatus.ReturnedToSender => cargo.OriginBranchId,   // İade, göndericinin şubesine döner
                CargoStatus.AtDestinationBranch => cargo.DestinationBranchId,
                CargoStatus.OutForDelivery => cargo.DestinationBranchId,
                CargoStatus.Delivered => cargo.DestinationBranchId,
                CargoStatus.DeliveryFailed => cargo.DestinationBranchId,
                CargoStatus.ReturnInProgress => cargo.DestinationBranchId,
                _ => null   // Transfer merkezindeyken şube yok
            };
        }

        // Neden: 6 haneli teslimat kodu. Tahmin edilmesin diye güvenli rastgele sayı üreticisi kullanıyoruz.
        private static string GenerateDeliveryCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        // Neden: Desi = En x Boy x Yükseklik / 3000 (case formülü)
        private static double CalculateDesi(double width, double length, double height)
        {
            return Math.Round(width * length * height / 3000, 2);
        }

        // Faz 4 - SLA: Teslimat tipi ve bölgeye göre tahmini teslim tarihi
        private static DateTime CalculateEstimatedArrival(DateTime shipmentDate, DeliveryType deliveryType, PriceZone zone)
        {
            int days = (deliveryType, zone) switch
            {
                (DeliveryType.Express, PriceZone.SameCity) => 1,
                (DeliveryType.Express, PriceZone.Domestic) => 2,
                (DeliveryType.Standard, PriceZone.SameCity) => 2,
                _ => 4   // Standart + şehirler arası
            };

            return shipmentDate.AddDays(days);
        }

        // Neden: Takip no formatı MYC-2026-849251. Aynısı varsa yeniden üretilir.
        private async Task<string> GenerateTrackCodeAsync()
        {
            string code;
            do
            {
                code = $"MYC-{DateTime.Now.Year}-{Random.Shared.Next(100000, 1000000)}";
            }
            while (await _cargoRepository.TrackCodeExistsAsync(code));

            return code;
        }
    }
}