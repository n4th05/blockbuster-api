using Blockbuster.Domain.Entities;

namespace Blockbuster.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetUsersWithActiveRentalsAsync();
        Task<IEnumerable<User>> GetUsersWhoRentedMovieAsync(int movieId);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}