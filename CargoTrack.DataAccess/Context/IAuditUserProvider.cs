namespace CargoTrack.DataAccess.Context
{
    // Neden: DbContext audit kaydında işlemi yapan kullanıcıyı bilmek ister,
    // ama DataAccess katmanı WebUI'ya (HttpContext'e) bağlanamaz. Arayüz burada, kodu WebUI'da.
    public interface IAuditUserProvider
    {
        Guid? GetUserId();
    }
}