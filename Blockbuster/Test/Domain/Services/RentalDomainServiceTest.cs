using NUnit.Framework;
using Moq;
using FluentAssertions;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Domain.Services;

namespace Blockbuster.Test.Domain.Services
{
    [TestFixture]
    public class RentalDomainServiceTests
    {
        private Mock<IRentalRepository> _rentalRepositoryMock;
        private Mock<IMovieRepository> _movieRepositoryMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private RentalDomainService _rentalDomainService;

        [SetUp]
        public void Setup()
        {
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _movieRepositoryMock = new Mock<IMovieRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _rentalDomainService = new RentalDomainService(
                _rentalRepositoryMock.Object,
                _movieRepositoryMock.Object,
                _userRepositoryMock.Object
            );
        }

        [Test]
        public async Task CanCreateRental_WithNonExistentMovie_ShouldReturnFalse()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync((Movie)null);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(new User("Test User", "test@user.com", "+5599999999999"));

            // Act
            var result = await _rentalDomainService.CanCreateRental(userId, movieId, startDate, endDate);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task CanCreateRental_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(new Movie("Test Movie", "Description", 9.99m));
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _rentalDomainService.CanCreateRental(userId, movieId, startDate, endDate);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task CanCreateRental_WithDeletedMovie_ShouldReturnFalse()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            var movie = new Movie("Test Movie", "Description", 9.99m);
            movie.Delete();

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(new User("Test User", "test@user.com", "+5599999999999"));

            // Act
            var result = await _rentalDomainService.CanCreateRental(userId, movieId, startDate, endDate);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task CanCreateRental_WithAvailableMovie_ShouldReturnTrue()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            var movie = new Movie("Test Movie", "Description", 9.99m);

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(new User("Test User", "test@user.com", "+5599999999999"));

            // Act
            var result = await _rentalDomainService.CanCreateRental(userId, movieId, startDate, endDate);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task GetUserRentals_ShouldReturnUserRentals()
        {
            // Arrange
            int userId = 1;
            var rentals = new List<Rental>
            {
                new Rental(userId, 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
                new Rental(userId, 2, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
                new Rental(2, 3, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)) // Different user
            };

            _rentalRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(rentals);

            // Act
            var result = await _rentalDomainService.GetUserRentals(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(r => r.UserId.Should().Be(userId));
        }

        [Test]
        public async Task GetMovieRentals_ShouldReturnMovieRentals()
        {
            // Arrange
            int movieId = 1;
            var rentals = new List<Rental>
            {
                new Rental(1, movieId, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
                new Rental(2, movieId, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
                new Rental(3, 2, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)) // Different movie
            };

            _rentalRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(rentals);

            // Act
            var result = await _rentalDomainService.GetMovieRentals(movieId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(r => r.MovieId.Should().Be(movieId));
        }

        [Test]
        public async Task GetRentalStatistics_WithNoRentals_ShouldReturnEmptyStatistics()
        {
            // Arrange
            _rentalRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Rental>());

            // Act
            var result = await _rentalDomainService.GetRentalStatistics();

            // Assert
            result.TotalActiveRentals.Should().Be(0);
            result.OverdueRentals.Should().Be(0);
            result.RentalsLast30Days.Should().Be(0);
            result.AverageRentalDuration.Should().Be(0);
        }

        [Test]
        public async Task GetRentalStatistics_ShouldCalculateCorrectStatistics()
        {
            // Arrange
            var currentDate = DateTime.UtcNow;
            var rentals = new List<Rental>
            {
                // Active rental
                new Rental(1, 1, currentDate.AddDays(-2), currentDate.AddDays(5)),
                // Overdue rental
                new Rental(2, 2, currentDate.AddDays(-10), currentDate.AddDays(-3)),
                // Future rental
                new Rental(3, 3, currentDate.AddDays(1), currentDate.AddDays(8)),
                // Old completed rental
                new Rental(4, 4, currentDate.AddDays(-40), currentDate.AddDays(-35))
            };

            _rentalRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(rentals);

            // Act
            var result = await _rentalDomainService.GetRentalStatistics();

            // Assert
            result.TotalActiveRentals.Should().Be(1); // Only the first rental is active
            result.OverdueRentals.Should().Be(2); // The second rental is overdue
            result.RentalsLast30Days.Should().Be(3); // First two rentals started within last 30 days
            result.AverageRentalDuration.Should().Be(6.5m); // All rentals are 7 days long
        }
    }
}