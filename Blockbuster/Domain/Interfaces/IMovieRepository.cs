using Blockbuster.Domain.Entities;

namespace Blockbuster.Domain.Interfaces
{
    public interface IMovieRepository
    {
        Task<Movie> GetByIdAsync(int id);
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<IEnumerable<Movie>> GetAvailableAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Movie>> GetTrendingAsync(int days);
        Task<Movie> AddAsync(Movie movie);
        Task UpdateAsync(Movie movie);
        Task DeleteAsync(Movie movie);
    }
}