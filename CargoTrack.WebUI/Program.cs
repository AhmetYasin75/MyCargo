using CargoTrack.Business;
using CargoTrack.Business.Services.Abouts;
using CargoTrack.Business.Services.Branches;
using CargoTrack.Business.Services.Cargos;
using CargoTrack.Business.Services.Cities;
using CargoTrack.Business.Services.Employees;
using CargoTrack.Business.Services.Pricing;
using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Context;
using CargoTrack.DataAccess.Repositories.Abouts;
using CargoTrack.DataAccess.Repositories.Branches;
using CargoTrack.DataAccess.Repositories.CargoPrices;
using CargoTrack.DataAccess.Repositories.Cargos;
using CargoTrack.DataAccess.Repositories.Cities;
using CargoTrack.DataAccess.Repositories.Employees;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using CargoTrack.WebUI.Filters;
using CargoTrack.WebUI.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using CargoTrack.Business.Services.Lookups;
using CargoTrack.DataAccess.Repositories.Users;
using Microsoft.AspNetCore.Localization;
using CargoTrack.DataAccess.Repositories.TransferCenters;
using CargoTrack.Business.Services.TransferCenters;
using CargoTrack.DataAccess.Repositories.AuditLogs;
using CargoTrack.Business.Services.AuditLogs;
using CargoTrack.DataAccess.Repositories.Dashboard;
using CargoTrack.Business.Services.Dashboard;
using CargoTrack.Business.Services.CargoPrices;
using CargoTrack.DataAccess.Repositories.Reports;
using CargoTrack.Business.Services.Reports;
using CargoTrack.Business.Services.Tracking;
using System.Globalization;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//IOC Container

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters()
                .AddValidatorsFromAssembly(typeof(BusinessAssembly).Assembly);

// Neden: UserContext, giriþ yapan kullanýcýya HttpContext üzerinden ulaþýr.
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAboutRepository, AboutRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICargoRepository, CargoRepository>();
builder.Services.AddScoped<ICargoPriceRepository, CargoPriceRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransferCenterRepository, TransferCenterRepository>();

builder.Services.AddScoped<IAboutService, AboutService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICargoService, CargoService>();
builder.Services.AddScoped<ICargoQueryService, CargoQueryService>();
builder.Services.AddScoped<ICargoPricingService, CargoPricingService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<ITransferCenterService, TransferCenterService>();
builder.Services.AddScoped<ICargoPriceService, CargoPriceService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();

// Neden: UserContext iki arayüzü birden uyguluyor. Ýkisi de ayný nesneyi (ayný istekte) kullansýn.
builder.Services.AddScoped<UserContext>();
builder.Services.AddScoped<IUserContext>(sp => sp.GetRequiredService<UserContext>());
builder.Services.AddScoped<IAuditUserProvider>(sp => sp.GetRequiredService<UserContext>());
builder.Services.AddScoped<ICargoAccessService, CargoAccessService>();



builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString
    ("DefaultConnection"));

    options.UseLazyLoadingProxies();
});

builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(config =>
{
    config.LoginPath = "/Login/Index";
    config.LogoutPath = "/Login/Logout";
    config.AccessDeniedPath = "/ErrorPages/AccessDenied";
    config.Cookie.Name = "CargoTrackCookie";
});

// Faz 7: Global Exception Handling. Filtre tüm Controller'lara otomatik uygulanýr.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Neden: Sayý giriþlerinde nokta ondalýk ayracý olsun (2.5 kg). Türkçe kültürde "2.5" yanlýþlýkla 25 okunurdu.
var appCulture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(appCulture),
    SupportedCultures = new List<CultureInfo> { appCulture },
    SupportedUICultures = new List<CultureInfo> { appCulture }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}");

// Uygulama baþlarken Seed Data ekleme iþlemi
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Neden: Migration kullandýðýmýz için EnsureCreated yerine Migrate.
    // Bekleyen migration varsa uygulama açýlýrken veritabanýna uygular.
    context.Database.Migrate();

    // Eðer veritabanýnda hiç þehir yoksa ekle
    if (!context.Cities.Any())
    {
        var cities = new List<City>
        {
            new City { Id = Guid.NewGuid(), Name = "Ýstanbul" },
            new City { Id = Guid.NewGuid(), Name = "Ankara" },
            new City { Id = Guid.NewGuid(), Name = "Ýzmir" },
            new City { Id = Guid.NewGuid(), Name = "Bursa" },
            new City { Id = Guid.NewGuid(), Name = "Antalya" },
            new City { Id = Guid.NewGuid(), Name = "Adana" },
            new City { Id = Guid.NewGuid(), Name = "Konya" },
            new City { Id = Guid.NewGuid(), Name = "Þanlýurfa" },
            new City { Id = Guid.NewGuid(), Name = "Gaziantep" },
            new City { Id = Guid.NewGuid(), Name = "Kocaeli" },
            new City { Id = Guid.NewGuid(), Name = "Mersin" },
            new City { Id = Guid.NewGuid(), Name = "Diyarbakýr" },
            new City { Id = Guid.NewGuid(), Name = "Hatay" },
            new City { Id = Guid.NewGuid(), Name = "Kayseri" },
            new City { Id = Guid.NewGuid(), Name = "Samsun" },
            new City { Id = Guid.NewGuid(), Name = "Balýkesir" },
            new City { Id = Guid.NewGuid(), Name = "Kahramanmaraþ" },
            new City { Id = Guid.NewGuid(), Name = "Van" },
            new City { Id = Guid.NewGuid(), Name = "Aydýn" },
            new City { Id = Guid.NewGuid(), Name = "Tekirdað" }
        };

        context.Cities.AddRange(cities);
        context.SaveChanges();
    }

    if (!context.Roles.Any())
    {
        var roles = new List<AppRole>
        {
            new AppRole { Name = "Admin" },
            new AppRole { Name = "Manager" },
            new AppRole { Name = "User" }
        };
        context.Roles.AddRange(roles);
        context.SaveChanges();
    }

    // Baþlangýç fiyat kurallarý (örnek deðerler, Admin ekranýndan deðiþtirilebilir)
    if (!context.CargoPrices.Any())
    {
        context.CargoPrices.AddRange(
            new CargoPrice { DeliveryType = DeliveryType.Standard, Zone = PriceZone.SameCity, BasePrice = 30, PricePerDesi = 3, PricePerKg = 3, InsuranceRate = 0.02m, IsActive = true },
            new CargoPrice { DeliveryType = DeliveryType.Standard, Zone = PriceZone.Domestic, BasePrice = 50, PricePerDesi = 5, PricePerKg = 5, InsuranceRate = 0.02m, IsActive = true },
            new CargoPrice { DeliveryType = DeliveryType.Express, Zone = PriceZone.SameCity, BasePrice = 50, PricePerDesi = 5, PricePerKg = 5, InsuranceRate = 0.03m, IsActive = true },
            new CargoPrice { DeliveryType = DeliveryType.Express, Zone = PriceZone.Domestic, BasePrice = 80, PricePerDesi = 8, PricePerKg = 8, InsuranceRate = 0.03m, IsActive = true }
        );
        context.SaveChanges();
    }

    // Baþlangýç transfer merkezleri (Admin ekraný 8D'de gelecek, þimdilik test için)
    // Neden: Þehir adlarýnda Türkçe karakter olmasýn diye Ankara ve Bursa seçildi.
    if (!context.TransferCenters.Any())
    {
        var ankara = context.Cities.FirstOrDefault(c => c.Name == "Ankara");
        var bursa = context.Cities.FirstOrDefault(c => c.Name == "Bursa");

        if (ankara is not null && bursa is not null)
        {
            context.TransferCenters.AddRange(
                new TransferCenter { Name = "Ankara Aktarma Merkezi", CityId = ankara.Id },
                new TransferCenter { Name = "Bursa Aktarma Merkezi", CityId = bursa.Id }
            );
            context.SaveChanges();
        }
    }
}

app.Run();