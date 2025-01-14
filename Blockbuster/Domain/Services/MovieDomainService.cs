using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;

namespace Blockbuster.Domain.Services
{
    public class MovieDomainService : IMovieDomainService
    {
        private readonly IMovieRepository _movieRepository;

        public MovieDomainService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<bool> CanRentMovie(int movieId, DateTime startDate, DateTime endDate)
        {
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie == null || movie.DeletedAt != null)
                return false;

            return movie.IsAvailable(startDate, endDate);
        }

        public async Task<IEnumerable<Movie>> GetTrendingMovies(int days)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-days);
            var movies = await _movieRepository.GetAllAsync();

            return movies
                .Where(m => m.DeletedAt == null)
                .Select(m => new
                {
                    Movie = m,
                    RentalCount = m.Rentals.Count(r => r.StartDate >= thirtyDaysAgo)
                })
                .OrderByDescending(x => x.RentalCount)
                .Take(10)
                .Select(x => x.Movie);
        }

        public async Task<bool> IsMovieAvailable(int movieId, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");

            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie == null || movie.DeletedAt != null)
                return false;

            return movie.IsAvailable(startDate, endDate);
        }
    }
}