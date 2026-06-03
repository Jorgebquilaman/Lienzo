using System.Text;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;

using Lienzo.Infrastructure.Data;
using Lienzo.Infrastructure.Identity;
using Lienzo.Infrastructure.Repositories;
using Lienzo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Lienzo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<LienzoDbContext>(options =>
            options.UseNpgsql(dataSource,
                b => b.MigrationsAssembly(typeof(LienzoDbContext).Assembly.FullName)));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<LienzoDbContext>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddRoleManager<RoleManager<IdentityRole<Guid>>>();

        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IPeriodoRepository, PeriodoRepository>();
        services.AddScoped<ICarreraRepository, CarreraRepository>();
        services.AddScoped<IActividadRepository, ActividadRepository>();
        services.AddScoped<ITipoPeriodoRepository, TipoPeriodoRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISyncBuildingService, SyncBuildingService>();
        services.AddScoped<ISyncClassroomService, SyncClassroomService>();
        services.AddScoped<ISyncTipoPeriodoService, SyncTipoPeriodoService>();
        services.AddScoped<ISyncPeriodoService, SyncPeriodoService>();
        services.AddScoped<ISyncActividadService, SyncActividadService>();
        services.AddScoped<ISyncDocenteService, SyncDocenteService>();
        services.AddScoped<ISyncCarreraService, SyncCarreraService>();
        services.AddScoped<ISyncEstudianteService, SyncEstudianteService>();
        services.AddScoped<ISgaAsistenciaService, SgaAsistenciaService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();

        services.AddScoped<IRepository<ReservationReminder>, GenericRepository<ReservationReminder>>();

        services.AddSingleton<IHostedService, ReservationReminderService>();

        services.AddHttpContextAccessor();

        return services;
    }
}
