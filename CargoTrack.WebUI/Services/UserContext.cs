using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Context;
using System.Security.Claims;

namespace CargoTrack.WebUI.Services
{
    // Neden: Tek sınıf iki işi yapar. IUserContext (Business için) ve IAuditUserProvider (DbContext için).
    // İkisi de giriş yapan kullanıcıyı cookie'deki claim'lerden okur.
    public class UserContext(IHttpContextAccessor _accessor) : IUserContext, IAuditUserProvider
    {
        private ClaimsPrincipal User => _accessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                var id = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(id, out var guid) ? guid : null;
            }
        }

        public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

        // IAuditUserProvider
        public Guid? GetUserId() => UserId;
    }
}