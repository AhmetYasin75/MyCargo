using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CargoTrack.DataAccess.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        private readonly IAuditUserProvider _auditUserProvider;

        // Neden: Audit kaydı için giriş yapan kullanıcıyı öğrenmemiz gerekiyor.
        // Parametre opsiyonel: kullanıcı yoksa (seed gibi sistem işlemleri) UserId boş kalır.
        public AppDbContext(DbContextOptions options, IAuditUserProvider auditUserProvider = null) : base(options)
        {
            _auditUserProvider = auditUserProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Neden: Cargo'nun AppUser ve Branch'e ikişer bağı var. EF hangi navigation hangisine ait
            // karıştırmasın diye bunları elle belirtiyoruz.
            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.Sender).WithMany(m => m.SentCargos)
                .HasForeignKey(c => c.SenderId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.Receiver).WithMany(m => m.ReceivedCargos)
                .HasForeignKey(c => c.ReceiverId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.OriginBranch).WithMany(m => m.OriginCargos)
                .HasForeignKey(c => c.OriginBranchId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.DestinationBranch).WithMany(m => m.DestinationCargos)
                .HasForeignKey(c => c.DestinationBranchId).OnDelete(DeleteBehavior.Restrict);

            // Neden: Bir kargonun tek Delivery kaydı olur (bire-bir).
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Cargo).WithOne(c => c.Delivery)
                .HasForeignKey<Delivery>(d => d.CargoId);

            // Neden: Takip numarası benzersiz olmalı (MYC-2026-849251).
            modelBuilder.Entity<Cargo>().HasIndex(c => c.TrackCode).IsUnique();

            // Neden: Para alanları 18,2 olmalı, yoksa EF uyarı verir ve kuruş kaybı olabilir.
            foreach (var prop in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetProperties())
                         .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                prop.SetPrecision(18);
                prop.SetScale(2);
            }

            // Neden: BaseEntity'den türeyen tüm tablolarda ilişkiler Restrict olsun.
            // Böylece hareket geçmişi gibi kayıtlar zincirleme silinemez.
            // Identity tablolarına dokunmuyoruz (onlar BaseEntity değil).
            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                         .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType))
                         .SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Neden: Soft Delete. Tüm sorgulara otomatik "IsDeleted == false" filtresi eklenir,
            // tek tek Where yazmaya gerek kalmaz. (Silinenleri görmek için .IgnoreQueryFilters())
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeleted = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var body = Expression.Equal(isDeleted, Expression.Constant(false));
                entityType.SetQueryFilter(Expression.Lambda(body, parameter));
            }
        }

        // Neden: Her kayıt/güncelleme/silmede audit, tarih ve soft delete işini tek yerde otomatik yapıyoruz.
        // Audit kayıtları da aynı SaveChanges içinde kaydolur: işlem ve log ya birlikte kaydolur ya hiçbiri.
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AuditLogs.AddRange(BuildAuditLogs());
            ApplyBaseEntityRules();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            AuditLogs.AddRange(BuildAuditLogs());
            ApplyBaseEntityRules();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyBaseEntityRules()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        break;

                    case EntityState.Modified:
                        // Neden: Güncellemede oluşturulma tarihi asla değişmemeli.
                        entry.Property(x => x.CreatedDate).IsModified = false;
                        entry.Entity.UpdatedDate = DateTime.Now;
                        break;

                    case EntityState.Deleted:
                        // Fiziksel silme yerine işaretleme
                        entry.State = EntityState.Modified;
                        entry.Property(x => x.CreatedDate).IsModified = false;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.UpdatedDate = DateTime.Now;
                        break;
                }
            }
        }

        // ---------------- FAZ 7: AUDIT LOG ----------------

        // Neden: JSON'da Türkçe karakterler \u00FC gibi kodlanmasın, veritabanında okunabilir kalsın.
        private static readonly JsonSerializerOptions AuditJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Neden: Bu alanlar her kayıtta zaten otomatik değişir, audit'e yazılırsa sadece gürültü olur.
        private static readonly string[] AuditIgnoredProperties =
        [
            nameof(BaseEntity.Id),
            nameof(BaseEntity.CreatedDate),
            nameof(BaseEntity.UpdatedDate),
            nameof(BaseEntity.IsDeleted)
        ];

        // Kaydedilmek üzere olan değişiklikleri tarar ve her biri için AuditLog satırı üretir.
        // ÖNEMLİ: Soft delete dönüşümünden (ApplyBaseEntityRules) ÖNCE çağrılmalı ki "Deleted" durumunu görebilelim.
        private List<AuditLog> BuildAuditLogs()
        {
            var logs = new List<AuditLog>();
            var userId = _auditUserProvider?.GetUserId();

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                // Neden: AuditLog'u loglamak sonsuz döngü olur. CargoMovement zaten kendi geçmiş kaydı.
                if (entry.Entity is AuditLog || entry.Entity is CargoMovement)
                {
                    continue;
                }

                // Lazy loading proxy kullandığımız için entry.Entity.GetType() "CargoProxy" döner,
                // gerçek sınıf adını Metadata'dan alıyoruz.
                var entityName = entry.Metadata.ClrType.Name;
                var entityId = entry.Entity.Id;

                var props = entry.Properties
                    .Where(p => !AuditIgnoredProperties.Contains(p.Metadata.Name))
                    .ToList();

                switch (entry.State)
                {
                    case EntityState.Added:
                        logs.Add(CreateLog(userId, AuditActionType.Create, entityName, entityId,
                            null,
                            ToJson(props.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),
                            $"{entityName} oluşturuldu."));
                        break;

                    case EntityState.Modified:
                        // Kargo durumu değiştiyse özel kayıt: eski ve yeni durum açıkça görünsün
                        if (entry.Entity is Cargo)
                        {
                            var status = entry.Property(nameof(Cargo.CargoStatus));
                            if (!Equals(status.OriginalValue, status.CurrentValue))
                            {
                                logs.Add(CreateLog(userId, AuditActionType.StatusChange, entityName, entityId,
                                    status.OriginalValue?.ToString(),
                                    status.CurrentValue?.ToString(),
                                    $"Kargo durumu değişti: {status.OriginalValue} → {status.CurrentValue}"));
                                break;
                            }
                        }

                        // Sadece gerçekten değişen alanlar
                        var changed = props.Where(p => !Equals(p.OriginalValue, p.CurrentValue)).ToList();

                        if (changed.Count > 0)
                        {
                            logs.Add(CreateLog(userId, AuditActionType.Update, entityName, entityId,
                                ToJson(changed.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)),
                                ToJson(changed.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),
                                $"{entityName} güncellendi."));
                        }
                        else
                        {
                            // DTO'dan yeni nesne üretilip Update() ile kaydedilen durumlarda (örn. Branch)
                            // EF eski değerleri bilmez. O zaman yeni değerleri olduğu gibi kaydederiz.
                            logs.Add(CreateLog(userId, AuditActionType.Update, entityName, entityId,
                                null,
                                ToJson(props.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),
                                $"{entityName} güncellendi."));
                        }
                        break;

                    case EntityState.Deleted:
                        logs.Add(CreateLog(userId, AuditActionType.Delete, entityName, entityId,
        ToJson(props.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)),
        null,
        $"{entityName} silindi."));
    break;
                }
            }

            return logs;
        }

        private static AuditLog CreateLog(
            Guid? userId, AuditActionType actionType, string entityName, Guid entityId,
            string oldValue, string newValue, string description)
        {
            return new AuditLog
            {
                UserId = userId,
                ActionType = actionType,
                EntityName = entityName,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                Description = description
            };
        }

        private static string ToJson(Dictionary<string, object> values)
        {
            return JsonSerializer.Serialize(values, AuditJsonOptions);
        }

        public DbSet<About> Abouts { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Cargo> Cargos { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public DbSet<TransferCenter> TransferCenters { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<CargoMovement> CargoMovements { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryException> DeliveryExceptions { get; set; }
        public DbSet<CargoPrice> CargoPrices { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}