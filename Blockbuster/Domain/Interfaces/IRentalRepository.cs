using Blockbuster.Domain.Entities;

namespace Blockbuster.Domain.Interfaces
{
    public interface IRentalRepository
    {
        Task<Rental> GetRentalAsync(int userId, int movieId);
        Task<IEnumerable<Rental>> GetAllAsync();
        Task<IEnumerable<Rental>> GetActiveRentalsAsync();
        Task<IEnumerable<Rental>> GetOverdueRentalsAsync();
        Task<IEnumerable<Rental>> GetUpcomingRentalsAsync();
        Task<Rental> AddAsync(Rental rental);
        Task UpdateAsync(Rental rental);
        Task DeleteAsync(Rental rental);
    }
}