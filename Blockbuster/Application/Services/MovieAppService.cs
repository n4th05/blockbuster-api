using Blockbuster.Application.DTOs;
using Blockbuster.Application.Exceptions;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;

namespace Blockbuster.Application.Services
{
    public class MovieAppService : IMovieAppService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IMovieDomainService _movieDomainService;

        public MovieAppService(
            IMovieRepository movieRepository,
            IMovieDomainService movieDomainService)
        {
            _movieRepository = movieRepository;
            _movieDomainService = movieDomainService;
        }

        public async Task<MovieDTO> GetMovieAsync(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null || movie.DeletedAt != null)
                return null;

            return MapToDTO(movie);
        }

        public async Task<MovieDTO> CreateMovieAsync(CreateMovieDTO createMovieDto)
        {
            var movie = new Movie(
                createMovieDto.Title,
                createMovieDto.Description,
                createMovieDto.Value
            );

            await _movieRepository.AddAsync(movie);
            return MapToDTO(movie);
        }

        private MovieDTO MapToDTO(Movie movie)
        {
            return new MovieDTO
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Value = movie.Value,
                IsAvailable = movie.IsAvailable(DateTime.UtcNow, DateTime.UtcNow),
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt
            };
        }

        public async Task<IEnumerable<MovieDTO>> GetAllMoviesAsync()
        {
            var movies = await _movieRepository.GetAllAsync();
            return movies.Where(m => m.DeletedAt == null)
                        .Select(MapToDTO);
        }

        public async Task<MovieDTO> UpdateMovieAsync(int id, UpdateMovieDTO updateMovieDto)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null || movie.DeletedAt != null)
                return null;

            movie.Update(
                updateMovieDto.Title,
                updateMovieDto.Description,
                updateMovieDto.Value
            );

            await _movieRepository.UpdateAsync(movie);
            return MapToDTO(movie);
        }

        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null || movie.DeletedAt != null)
                throw new NotFoundException("Movie not found");

            movie.Delete();
            await _movieRepository.UpdateAsync(movie);
        }

        public async Task<IEnumerable<MovieDTO>> GetTrendingMoviesAsync()
        {
            var trendingMovies = await _movieDomainService.GetTrendingMovies(30); // Last 30 days
            return trendingMovies.Select(MapToDTO);
        }

        public async Task<IEnumerable<MovieDTO>> GetAvailableMoviesAsync()
        {
            var currentDate = DateTime.UtcNow;
            var availableMovies = await _movieRepository.GetAvailableAsync(currentDate, currentDate);
            return availableMovies.Select(MapToDTO);
        }

        public async Task<IEnumerable<MovieDTO>> SearchMoviesAsync(MovieSearchDTO searchDto)
        {
            var query = await _movieRepository.GetAllAsync();

            var filteredMovies = query.Where(m => m.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(searchDto.Title))
                filteredMovies = filteredMovies.Where(m =>
                    m.Title.Contains(searchDto.Title, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(searchDto.Description))
                filteredMovies = filteredMovies.Where(m =>
                    m.Description.Contains(searchDto.Description, StringComparison.OrdinalIgnoreCase));

            if (searchDto.MinValue.HasValue)
                filteredMovies = filteredMovies.Where(m => m.Value >= searchDto.MinValue.Value);

            if (searchDto.MaxValue.HasValue)
                filteredMovies = filteredMovies.Where(m => m.Value <= searchDto.MaxValue.Value);

            if (searchDto.IsAvailable.HasValue)
            {
                var currentDate = DateTime.UtcNow;
                filteredMovies = filteredMovies.Where(m =>
                    m.IsAvailable(currentDate, currentDate) == searchDto.IsAvailable.Value);
            }

            return filteredMovies.Select(MapToDTO);
        }

        public async Task<MovieStatisticsDTO> GetMovieStatisticsAsync()
        {
            var allMovies = await _movieRepository.GetAllAsync();
            var activeMovies = allMovies.Where(m => m.DeletedAt == null).ToList();
            var currentDate = DateTime.UtcNow;

            var stats = new MovieStatisticsDTO
            {
                TotalMovies = activeMovies.Count,
                AvailableMovies = activeMovies.Count(m => m.IsAvailable(currentDate, currentDate)),
                RentedMovies = activeMovies.Count(m => !m.IsAvailable(currentDate, currentDate)),
                AverageRentalDuration = CalculateAverageRentalDuration(activeMovies),
                TopRentedMovies = GetTopRentedMovies(activeMovies, 5)
            };

            return stats;
        }

        private decimal CalculateAverageRentalDuration(IEnumerable<Movie> movies)
        {
            var allRentals = movies.SelectMany(m => m.Rentals);
            if (!allRentals.Any())
                return 0;

            return (decimal)allRentals.Average(r => (r.EndDate - r.StartDate).TotalDays);
        }

        private IEnumerable<MovieRentalStatDTO> GetTopRentedMovies(IEnumerable<Movie> movies, int count)
        {
            return movies
                .Select(m => new MovieRentalStatDTO
                {
                    MovieId = m.Id,
                    Title = m.Title,
                    RentalCount = m.Rentals.Count
                })
                .OrderByDescending(m => m.RentalCount)
                .Take(count);
        }
    }
}