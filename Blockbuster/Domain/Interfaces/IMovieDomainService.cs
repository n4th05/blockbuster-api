using Blockbuster.Domain.Entities;

namespace Blockbuster.Domain.Interfaces
{
    public interface IMovieDomainService
    {
        Task<bool> CanRentMovie(int movieId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Movie>> GetTrendingMovies(int days);
        Task<bool> IsMovieAvailable(int movieId, DateTime startDate, DateTime endDate);
    }
}