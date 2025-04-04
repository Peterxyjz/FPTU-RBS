using BookingManagement.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoomRepository Rooms { get; }
        IBookingRepository Bookings { get; }
        ITimeSlotRepository TimeSlots { get; }
        INotificationRepository Notifications { get; }
        IRoleRepository Roles { get; }
        IOperationalHoursRepository OperationalHours { get; }
        ISpecialScheduleRepository SpecialSchedules { get; }
        IBlockedTimeSlotRepository BlockedTimeSlots { get; }
        
        Task<int> CompleteAsync();
    }
}
