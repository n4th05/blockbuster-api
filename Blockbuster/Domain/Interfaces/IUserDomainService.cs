using Blockbuster.Domain.Entities;

namespace Blockbuster.Domain.Interfaces
{
    public interface IUserDomainService
    {
        Task<bool> CanDeleteUser(int userId);
        Task<bool> IsEmailUnique(string email);
        Task<IEnumerable<User>> GetUsersWithRentalHistory();
    }
}
