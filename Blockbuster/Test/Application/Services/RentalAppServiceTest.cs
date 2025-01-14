using NUnit.Framework;
using Moq;
using FluentAssertions;
using Blockbuster.Application.DTOs;
using Blockbuster.Application.Services;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Application.Exceptions;
using System.ComponentModel.DataAnnotations;
using Blockbuster.Domain.Services;

namespace Blockbuster.Test.Application.Services
{
    [TestFixture]
    public class RentalAppServiceTests
    {
        private Mock<IRentalRepository> _rentalRepositoryMock;
        private Mock<IRentalDomainService> _rentalDomainServiceMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IMovieRepository> _movieRepositoryMock;
        private RentalAppService _rentalAppService;

        [SetUp]
        public void Setup()
        {
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _rentalDomainServiceMock = new Mock<IRentalDomainService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _movieRepositoryMock = new Mock<IMovieRepository>();

            _rentalAppService = new RentalAppService(
                _rentalRepositoryMock.Object,
                _rentalDomainServiceMock.Object,
                _userRepositoryMock.Object,
                _movieRepositoryMock.Object
            );
        }

        [Test]
        public async Task GetRentalAsync_WithValidIds_ShouldReturnRentalDTO()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;
            var rental = new Rental(userId, movieId, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
            var user = new User("Test User", "test@user.com", "+5599999999999") { Id = userId };
            var movie = new Movie("Test Movie", "Description", 9.99m) { Id = movieId };

            _rentalRepositoryMock.Setup(x => x.GetRentalAsync(userId, movieId))
                .ReturnsAsync(rental);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _rentalAppService.GetRentalAsync(userId, movieId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.MovieId.Should().Be(movieId);
            result.UserName.Should().Be(user.Name);
            result.MovieTitle.Should().Be(movie.Title);
        }

        [Test]
        public async Task GetRentalAsync_WithNonExistentRental_ShouldReturnNull()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;

            _rentalRepositoryMock.Setup(x => x.GetRentalAsync(userId, movieId))
                .ReturnsAsync((Rental)null);

            // Act
            var result = await _rentalAppService.GetRentalAsync(userId, movieId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task CreateRentalAsync_WithValidData_ShouldReturnRentalDTO()
        {
            // Arrange
            var createDto = new CreateRentalDTO
            {
                UserId = 1,
                MovieId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7)
            };

            var user = new User("Test User", "test@user.com", "+5599999999999") { Id = createDto.UserId };
            var movie = new Movie("Test Movie", "Description", 9.99m) { Id = createDto.MovieId };

            _rentalDomainServiceMock.Setup(x => x.CanCreateRental(
                createDto.UserId, createDto.MovieId, createDto.StartDate, createDto.EndDate))
                .ReturnsAsync(true);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(createDto.UserId))
                .ReturnsAsync(user);
            _movieRepositoryMock.Setup(x => x.GetByIdAsync(createDto.MovieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _rentalAppService.CreateRentalAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(createDto.UserId);
            result.MovieId.Should().Be(createDto.MovieId);
            result.UserName.Should().Be(user.Name);
            result.MovieTitle.Should().Be(movie.Title);
            _rentalRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Rental>()), Times.Once);
        }

        [Test]
        public async Task CreateRentalAsync_WhenCannotCreate_ShouldThrowValidationException()
        {
            // Arrange
            var createDto = new CreateRentalDTO
            {
                UserId = 1,
                MovieId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7)
            };

            _rentalDomainServiceMock.Setup(x => x.CanCreateRental(
                createDto.UserId, createDto.MovieId, createDto.StartDate, createDto.EndDate))
                .ReturnsAsync(false);

            // Act & Assert
            await FluentActions.Invoking(() =>
                _rentalAppService.CreateRentalAsync(createDto))
                .Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task UpdateRentalAsync_WithValidData_ShouldReturnUpdatedRentalDTO()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;
            var updateDto = new UpdateRentalDTO
            {
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(8)
            };

            var rental = new Rental(userId, movieId, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
            var user = new User("Test User", "test@user.com", "+5599999999999") { Id = userId };
            var movie = new Movie("Test Movie", "Description", 9.99m) { Id = movieId };

            _rentalRepositoryMock.Setup(x => x.GetRentalAsync(userId, movieId))
                .ReturnsAsync(rental);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _rentalAppService.UpdateRentalAsync(userId, movieId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.StartDate.Should().Be(updateDto.StartDate);
            result.EndDate.Should().Be(updateDto.EndDate);
            _rentalRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Rental>()), Times.Once);
        }

        [Test]
        public async Task DeleteRentalAsync_WithValidIds_ShouldDeleteRental()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;
            var rental = new Rental(userId, movieId, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

            _rentalRepositoryMock.Setup(x => x.GetRentalAsync(userId, movieId))
                .ReturnsAsync(rental);

            // Act
            await _rentalAppService.DeleteRentalAsync(userId, movieId);

            // Assert
            _rentalRepositoryMock.Verify(x => x.DeleteAsync(rental), Times.Once);
        }

        [Test]
        public async Task DeleteRentalAsync_WithNonExistentRental_ShouldThrowNotFoundException()
        {
            // Arrange
            int userId = 1;
            int movieId = 1;

            _rentalRepositoryMock.Setup(x => x.GetRentalAsync(userId, movieId))
                .ReturnsAsync((Rental)null);

            // Act & Assert
            await FluentActions.Invoking(() =>
                _rentalAppService.DeleteRentalAsync(userId, movieId))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task SearchRentalsAsync_WithCriteria_ShouldReturnFilteredRentals()
        {
            // Arrange
            var currentDate = DateTime.UtcNow;
            var searchDto = new RentalSearchDTO
            {
                StartDate = currentDate.AddDays(-7),
                EndDate = currentDate.AddDays(7),
                UserName = "Test",
                MovieTitle = "Movie",
                IsActive = true,
                IsOverdue = false
            };

            var users = new List<User>
    {
        new User("Test User 1", "test1@user.com", "+5599999999999") { Id = 1 },
        new User("Other User", "test2@user.com", "+5599999999999") { Id = 2 },
        new User("Test User 3", "test3@user.com", "+5599999999999") { Id = 3 }
    };

            var movies = new List<Movie>
    {
        new Movie("Test Movie 1", "Description", 9.99m) { Id = 1 },
        new Movie("Other Movie", "Description", 9.99m) { Id = 2 },
        new Movie("Test Movie 3", "Description", 9.99m) { Id = 3 }
    };

            // Create rentals and set up navigation properties
            var rentals = new List<Rental>
    {
        new Rental(users[0].Id, movies[0].Id, currentDate.AddDays(-3), currentDate.AddDays(4))
        {
            User = users[0],
            Movie = movies[0]
        },
        new Rental(users[1].Id, movies[1].Id, currentDate.AddDays(-10), currentDate.AddDays(-5))
        {
            User = users[1],
            Movie = movies[1]
        },
        new Rental(users[2].Id, movies[2].Id, currentDate.AddDays(1), currentDate.AddDays(8))
        {
            User = users[2],
            Movie = movies[2]
        }
    };

            _rentalRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(rentals);

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _movieRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Set up individual user and movie lookups
            foreach (var user in users)
            {
                _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
                    .ReturnsAsync(user);
            }

            foreach (var movie in movies)
            {
                _movieRepositoryMock.Setup(x => x.GetByIdAsync(movie.Id))
                    .ReturnsAsync(movie);
            }

            // Act
            var result = await _rentalAppService.SearchRentalsAsync(searchDto);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(r => r.StartDate >= searchDto.StartDate);
            result.Should().OnlyContain(r => r.EndDate <= searchDto.EndDate);
            result.Should().OnlyContain(r => r.UserName.Contains("Test", StringComparison.OrdinalIgnoreCase));
            result.Should().OnlyContain(r => r.MovieTitle.Contains("Movie", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task GetRentalStatisticsAsync_ShouldReturnCompleteStatistics()
        {
            // Arrange
            var stats = new RentalStatistics
            {
                TotalActiveRentals = 5,
                OverdueRentals = 2,
                RentalsLast30Days = 10,
                AverageRentalDuration = 7
            };

            var users = CreateTestUsers();
            var movies = CreateTestMovies();

            _rentalDomainServiceMock.Setup(x => x.GetRentalStatistics())
                .ReturnsAsync(stats);
            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _movieRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _rentalAppService.GetRentalStatisticsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalActiveRentals.Should().Be(stats.TotalActiveRentals);
            result.OverdueRentals.Should().Be(stats.OverdueRentals);
            result.RentalsLast30Days.Should().Be(stats.RentalsLast30Days);
            result.AverageRentalDuration.Should().Be(stats.AverageRentalDuration);
            result.MostActiveUsers.Should().NotBeEmpty();
            result.MostRentedMovies.Should().NotBeEmpty();
        }

        private void SetupUserAndMovieRepositories(List<User> users, List<Movie> movies)
        {
            foreach (var user in users)
            {
                _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
                    .ReturnsAsync(user);
            }

            foreach (var movie in movies)
            {
                _movieRepositoryMock.Setup(x => x.GetByIdAsync(movie.Id))
                    .ReturnsAsync(movie);
            }
        }

        private List<User> CreateTestUsers()
        {
            var users = new List<User>();
            var baseDate = DateTime.UtcNow.AddDays(-30);

            for (int i = 1; i <= 5; i++)
            {
                var user = new User($"User {i}", $"user{i}@test.com", "+5599999999999") { Id = i };

                // Add rentals with non-overlapping dates
                for (int j = 0; j < i; j++)
                {
                    var startDate = baseDate.AddDays(j * 2);
                    var endDate = startDate.AddDays(1);
                    user.AddRental(new Rental(user.Id, j + 1, startDate, endDate));
                }

                users.Add(user);
            }

            return users;
        }

        private List<Movie> CreateTestMovies()
        {
            var movies = new List<Movie>();
            var baseDate = DateTime.UtcNow.AddDays(-30);

            for (int i = 1; i <= 5; i++)
            {
                var movie = new Movie($"Movie {i}", "Description", 9.99m) { Id = i };

                // Add rentals with non-overlapping dates
                for (int j = 0; j < i; j++)
                {
                    var startDate = baseDate.AddDays(j * 2);
                    var endDate = startDate.AddDays(1);
                    movie.AddRental(new Rental(j + 1, movie.Id, startDate, endDate));
                }

                movies.Add(movie);
            }

            return movies;
        }
    }
}