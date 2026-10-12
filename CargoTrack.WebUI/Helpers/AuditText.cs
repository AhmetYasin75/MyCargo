using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.WebUI.Helpers
{
    // Neden: Audit açıklamaları kayıt anında sistem adlarıyla (Cargo, AtOriginBranch) saklanır ve değişmez.
    // Ekranda gösterirken Türkçeye çeviririz; eski kayıtlar da otomatik Türkçe görünür.
    public static class AuditText
    {
        private static readonly string[] Suffixes = { " oluşturuldu.", " güncellendi.", " silindi." };

        public static string Description(string? description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return string.Empty;
            }

            // "Kargo durumu değişti: Created → AtOriginBranch"
            const string statusPrefix = "Kargo durumu değişti: ";
            if (description.StartsWith(statusPrefix))
            {
                var parts = description[statusPrefix.Length..].Split(" → ");
                if (parts.Length == 2)
                {
                    return $"{statusPrefix}{Status(parts[0])} → {Status(parts[1])}";
                }
            }

            // "Cargo oluşturuldu." / "Branch güncellendi." / "TransferCenter silindi."
            foreach (var suffix in Suffixes)
            {
                if (description.EndsWith(suffix))
                {
                    var entity = description[..^suffix.Length];
                    return EnumDisplay.EntityName(entity) + suffix;
                }
            }

            return description;
        }

        private static string Status(string raw)
        {
            return Enum.TryParse<CargoStatus>(raw, out var status) ? EnumDisplay.Status(status) : raw;
        }
    }
}