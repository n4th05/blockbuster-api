using Blockbuster.Application.DTOs;
using Blockbuster.Application.Exceptions;
using Blockbuster.Application.Interfaces;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Domain.Services;
using System.ComponentModel.DataAnnotations;

namespace Blockbuster.Application.Services
{
    public class RentalAppService : IRentalAppService
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IRentalDomainService _rentalDomainService;
        private readonly IUserRepository _userRepository;
        private readonly IMovieRepository _movieRepository;

        public RentalAppService(
            IRentalRepository rentalRepository,
            IRentalDomainService rentalDomainService,
            IUserRepository userRepository,
            IMovieRepository movieRepository)
        {
            _rentalRepository = rentalRepository;
            _rentalDomainService = rentalDomainService;
            _userRepository = userRepository;
            _movieRepository = movieRepository;
        }

        public async Task<RentalDTO> GetRentalAsync(int userId, int movieId)
        {
            var rental = await _rentalRepository.GetRentalAsync(userId, movieId);
            return rental == null ? null : await MapToDTOAsync(rental);
        }

        public async Task<IEnumerable<RentalDTO>> GetAllRentalsAsync()
        {
            var rentals = await _rentalRepository.GetAllAsync();
            return await Task.WhenAll(rentals.Select(MapToDTOAsync));
        }

        public async Task<RentalDTO> CreateRentalAsync(CreateRentalDTO createRentalDto)
        {
            var canCreate = await _rentalDomainService.CanCreateRental(
                createRentalDto.UserId,
                createRentalDto.MovieId,
                createRentalDto.StartDate,
                createRentalDto.EndDate);

            if (!canCreate)
                throw new ValidationException("Cannot create rental. Movie might not be available for the specified period.");

            var rental = new Rental(
                createRentalDto.UserId,
                createRentalDto.MovieId,
                createRentalDto.StartDate,
                createRentalDto.EndDate
            );

            await _rentalRepository.AddAsync(rental);
            return await MapToDTOAsync(rental);
        }
        public async Task<RentalDTO> UpdateRentalAsync(int userId, int movieId, UpdateRentalDTO updateRentalDto)
        {
            var rental = await _rentalRepository.GetRentalAsync(userId, movieId);
            if (rental == null)
                return null;

            rental.UpdateDates(updateRentalDto.StartDate, updateRentalDto.EndDate);
            await _rentalRepository.UpdateAsync(rental);
            return await MapToDTOAsync(rental);
        }

        public async Task DeleteRentalAsync(int userId, int movieId)
        {
            var rental = await _rentalRepository.GetRentalAsync(userId, movieId);
            if (rental == null)
                throw new NotFoundException("Rental not found");

            await _rentalRepository.DeleteAsync(rental);
        }

        public async Task<IEnumerable<RentalDTO>> GetActiveRentalsAsync()
        {
            var rentals = await _rentalRepository.GetActiveRentalsAsync();
            return await Task.WhenAll(rentals.Select(MapToDTOAsync));
        }

        public async Task<IEnumerable<RentalDTO>> GetOverdueRentalsAsync()
        {
            var rentals = await _rentalRepository.GetOverdueRentalsAsync();
            return await Task.WhenAll(rentals.Select(MapToDTOAsync));
        }

        public async Task<IEnumerable<RentalDTO>> GetUpcomingRentalsAsync()
        {
            var rentals = await _rentalRepository.GetUpcomingRentalsAsync();
            return await Task.WhenAll(rentals.Select(MapToDTOAsync));
        }

        public async Task<IEnumerable<RentalDTO>> SearchRentalsAsync(RentalSearchDTO searchDto)
        {
            var rentals = await _rentalRepository.GetAllAsync();
            var currentDate = DateTime.UtcNow;

            var query = rentals.AsQueryable();

            if (searchDto.StartDate.HasValue)
                query = query.Where(r => r.StartDate >= searchDto.StartDate.Value);

            if (searchDto.EndDate.HasValue)
                query = query.Where(r => r.EndDate <= searchDto.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(searchDto.UserName))
                query = query.Where(r => r.User.Name.Contains(searchDto.UserName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(searchDto.MovieTitle))
                query = query.Where(r => r.Movie.Title.Contains(searchDto.MovieTitle, StringComparison.OrdinalIgnoreCase));

            if (searchDto.IsActive.HasValue)
                query = query.Where(r => r.IsActive(currentDate) == searchDto.IsActive.Value);

            if (searchDto.IsOverdue.HasValue)
                query = query.Where(r => r.IsOverdue(currentDate) == searchDto.IsOverdue.Value);

            return await Task.WhenAll(query.Select(MapToDTOAsync));
        }

        public async Task<RentalStatisticsDTO> GetRentalStatisticsAsync()
        {
            var stats = await _rentalDomainService.GetRentalStatistics();
            var users = await _userRepository.GetAllAsync();
            var movies = await _movieRepository.GetAllAsync();
            var currentDate = DateTime.UtcNow;
            var thirtyDaysAgo = currentDate.AddDays(-30);

            return new RentalStatisticsDTO
            {
                TotalActiveRentals = stats.TotalActiveRentals,
                OverdueRentals = stats.OverdueRentals,
                RentalsLast30Days = stats.RentalsLast30Days,
                AverageRentalDuration = stats.AverageRentalDuration,
                MostActiveUsers = users
                    .Select(u => new TopUserDTO
                    {
                        Name = u.Name,
                        RentalCount = u.Rentals.Count(r => r.StartDate >= thirtyDaysAgo)
                    })
                    .OrderByDescending(u => u.RentalCount)
                    .Take(5),
                MostRentedMovies = movies
                    .Where(m => m.DeletedAt == null)
                    .Select(m => new TopMovieDTO
                    {
                        Title = m.Title,
                        RentalCount = m.Rentals.Count(r => r.StartDate >= thirtyDaysAgo)
                    })
                    .OrderByDescending(m => m.RentalCount)
                    .Take(5)
            };
        }

        private async Task<RentalDTO> MapToDTOAsync(Rental rental)
        {
            var user = await _userRepository.GetByIdAsync(rental.UserId);
            var movie = await _movieRepository.GetByIdAsync(rental.MovieId);
            var currentDate = DateTime.UtcNow;

            return new RentalDTO
            {
                UserId = rental.UserId,
                MovieId = rental.MovieId,
                UserName = user?.Name,
                MovieTitle = movie?.Title,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                IsActive = rental.IsActive(currentDate),
                IsOverdue = rental.IsOverdue(currentDate)
            };
        }
    }
}
