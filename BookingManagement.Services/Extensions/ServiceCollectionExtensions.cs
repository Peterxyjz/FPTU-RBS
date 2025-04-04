using BookingManagement.Services.Interfaces;
using BookingManagement.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

namespace BookingManagement.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Add existing services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ITimeSlotService, TimeSlotService>();
            services.AddScoped<INotificationService, NotificationService>();
            
            // Add Time Management services
            services.AddScoped<IOperationalHoursService, OperationalHoursService>();
            services.AddScoped<ISpecialScheduleService, SpecialScheduleService>();
            services.AddScoped<IBlockedTimeSlotService, BlockedTimeSlotService>();
            
            // Add SignalR service
            services.AddScoped<ISignalRService, SignalRService>();
            
            return services;
        }
    }
}
