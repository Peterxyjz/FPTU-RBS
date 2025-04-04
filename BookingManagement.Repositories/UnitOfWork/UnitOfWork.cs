using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Implementations;
using BookingManagement.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FptuRoomBookingContext _context;
        private IUserRepository _userRepository;
        private IRoomRepository _roomRepository;
        private IBookingRepository _bookingRepository;
        private ITimeSlotRepository _timeSlotRepository;
        private INotificationRepository _notificationRepository;
        private IRoleRepository _roleRepository;
        private IOperationalHoursRepository _operationalHoursRepository;
        private ISpecialScheduleRepository _specialScheduleRepository;
        private IBlockedTimeSlotRepository _blockedTimeSlotRepository;
        
        public UnitOfWork(FptuRoomBookingContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _userRepository ??= new UserRepository(_context);
        public IRoomRepository Rooms => _roomRepository ??= new RoomRepository(_context);
        public IBookingRepository Bookings => _bookingRepository ??= new BookingRepository(_context);
        public ITimeSlotRepository TimeSlots => _timeSlotRepository ??= new TimeSlotRepository(_context);
        public INotificationRepository Notifications => _notificationRepository ??= new NotificationRepository(_context);
        public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);
        public IOperationalHoursRepository OperationalHours => _operationalHoursRepository ??= new OperationalHoursRepository(_context);
        public ISpecialScheduleRepository SpecialSchedules => _specialScheduleRepository ??= new SpecialScheduleRepository(_context);
        public IBlockedTimeSlotRepository BlockedTimeSlots => _blockedTimeSlotRepository ??= new BlockedTimeSlotRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
