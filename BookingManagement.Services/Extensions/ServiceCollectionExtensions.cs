﻿using BookingManagement.Services.Interfaces;
using BookingManagement.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingManagement.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<ITimeSlotService, TimeSlotService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOperationalHoursService, OperationalHoursService>();
            services.AddScoped<ISpecialScheduleService, SpecialScheduleService>();
            services.AddScoped<IBlockedTimeSlotService, BlockedTimeSlotService>();
            
            return services;
        }
    }
}
