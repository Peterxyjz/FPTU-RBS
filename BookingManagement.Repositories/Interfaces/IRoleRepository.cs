using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetRoleByNameAsync(string roleName);
    }
}
