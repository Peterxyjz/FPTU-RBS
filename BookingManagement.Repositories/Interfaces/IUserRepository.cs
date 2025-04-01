using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> IsEmailExistsAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId);
    }
}
