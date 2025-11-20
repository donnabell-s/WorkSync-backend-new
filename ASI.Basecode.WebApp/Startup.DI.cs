using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ASI.Basecode.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            this._services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Register DbContext
            this._services.AddScoped<WorkSyncDbContext>();

            // Common
            this._services.AddScoped<TokenProvider>();
            this._services.TryAddSingleton<TokenProviderOptionsFactory>();
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register role claims transformer to normalize role claims
            this._services.TryAddSingleton<Microsoft.AspNetCore.Authentication.IClaimsTransformation, RoleClaimsTransformer>();

            // Services
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<IBookingService, BookingService>();
            this._services.AddScoped<IRoomService, RoomService>();
            this._services.AddScoped<ISessionService, SessionService>();
            this._services.AddScoped<IUserPreferenceService, UserPreferenceService>();
            this._services.AddScoped<IBookingLogService, BookingLogService>();
            this._services.AddScoped<IDashboardService, DashboardService>();
            this._services.AddScoped<IRoomLogService, RoomLogService>();
            
            // Metrics Service for optimized dashboard (uses MemoryCache)
            this._services.AddScoped<IMetricsService, MetricsService>();

            // Repositories
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<IBookingRepository, BookingRepository>();
            this._services.AddScoped<IBookingLogRepository, BookingLogRepository>();
            this._services.AddScoped<IRoomRepository, RoomRepository>();
            this._services.AddScoped<IRoomLogRepository, RoomLogRepository>();
            this._services.AddScoped<ISessionRepository, SessionRepository>();
            this._services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            this._services.AddScoped<IDashboardRepository, DashboardRepository>();
            
            // Metrics Repository for summary tables
            this._services.AddScoped<IMetricsRepository, MetricsRepository>();

            // Manager Class
            this._services.AddScoped<SignInManager>();

            // Background Services
            // MetricsComputationHostedService runs every 5 minutes to compute dashboard metrics
            this._services.AddHostedService<ASI.Basecode.WebApp.HostedServices.MetricsComputationHostedService>();

            this._services.AddHttpClient();

            this._services.AddControllers();
        }
    }
}
