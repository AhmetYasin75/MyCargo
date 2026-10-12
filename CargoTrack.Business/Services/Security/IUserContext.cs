namespace CargoTrack.Business.Services.Security
{
    // Neden: Business katmanı giriş yapan kullanıcıyı bilmek ister ama HttpContext'e bağlanmamalı.
    // Arayüz burada, gerçek kodu WebUI'da yazılır (katman kuralı bozulmaz).
    public interface IUserContext
    {
        Guid? UserId { get; }
        bool IsInRole(string role);
    }
}