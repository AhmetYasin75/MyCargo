using System.Globalization;

namespace CargoTrack.WebUI.Helpers
{
    // Neden: Uygulama kültürü en-US (form girişinde nokta ondalık). Ekranda para Türkçe görünsün: 1.234,50 ₺
    public static class MoneyFormat
    {
        private static readonly CultureInfo Tr = new("tr-TR");

        public static string TL(decimal value) => value.ToString("N2", Tr) + " ₺";
    }
}