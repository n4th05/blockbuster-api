using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;

namespace Blockbuster.Domain.Services
{
    public interface IRentalDomainService
    {
        Task<bool> CanCreateRental(int userId, int movieId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Rental>> GetUserRentals(int userId);
        Task<IEnumerable<Rental>> GetMovieRentals(int movieId);
        Task<RentalStatistics> GetRentalStatistics();
    }

    public class RentalStatistics
    {
        public int TotalActiveRentals { get; set; }
        public int OverdueRentals { get; set; }
        public int RentalsLast30Days { get; set; }
        public decimal AverageRentalDuration { get; set; }
    }

    public class RentalDomainService : IRentalDomainService
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IUserRepository _userRepository;

        public RentalDomainService(
            IRentalRepository rentalRepository,
            IMovieRepository movieRepository,
            IUserRepository userRepository)
        {
            _rentalRepository = rentalRepository;
            _movieRepository = movieRepository;
            _userRepository = userRepository;
        }

        public async Task<bool> CanCreateRental(int userId, int movieId, DateTime startDate, DateTime endDate)
        {
            var movie = await _movieRepository.GetByIdAsync(movieId);
            var user = await _userRepository.GetByIdAsync(userId);

            if (movie == null || movie.DeletedAt != null || user == null)
                return false;

            return movie.IsAvailable(startDate, endDate);
        }

        public async Task<IEnumerable<Rental>> GetUserRentals(int userId)
        {
            var rentals = await _rentalRepository.GetAllAsync();
            return rentals.Where(r => r.UserId == userId);
        }

        public async Task<IEnumerable<Rental>> GetMovieRentals(int movieId)
        {
            var rentals = await _rentalRepository.GetAllAsync();
            return rentals.Where(r => r.MovieId == movieId);
        }

        public async Task<RentalStatistics> GetRentalStatistics()
        {
            var currentDate = DateTime.UtcNow;
            var thirtyDaysAgo = currentDate.AddDays(-30);
            var allRentals = await _rentalRepository.GetAllAsync();

            return new RentalStatistics
            {
                TotalActiveRentals = allRentals.Count(r => r.IsActive(currentDate)),
                OverdueRentals = allRentals.Count(r => r.IsOverdue(currentDate)),
                RentalsLast30Days = allRentals.Count(r => r.StartDate >= thirtyDaysAgo),
                AverageRentalDuration = allRentals.Any()
                    ? (decimal)allRentals.Average(r => r.GetRentalDurationInDays())
                    : 0
            };
        }
    }
}
