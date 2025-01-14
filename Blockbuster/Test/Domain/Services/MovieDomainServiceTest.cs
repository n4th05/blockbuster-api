using NUnit.Framework;
using Moq;
using FluentAssertions;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Domain.Services;

namespace Blockbuster.Test.Domain.Services
{
    [TestFixture]
    public class MovieDomainServiceTests
    {
        private Mock<IMovieRepository> _movieRepositoryMock;
        private MovieDomainService _movieDomainService;

        [SetUp]
        public void Setup()
        {
            _movieRepositoryMock = new Mock<IMovieRepository>();
            _movieDomainService = new MovieDomainService(_movieRepositoryMock.Object);
        }

        [Test]
        public async Task CanRentMovie_WithNonExistentMovie_ShouldReturnFalse()
        {
            // Arrange
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync((Movie)null);

            // Act
            var result = await _movieDomainService.CanRentMovie(movieId, startDate, endDate);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task CanRentMovie_WithDeletedMovie_ShouldReturnFalse()
        {
            // Arrange
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            var movie = new Movie("Test", "Description", 9.99m);
            movie.Delete(); // Mark as deleted

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _movieDomainService.CanRentMovie(movieId, startDate, endDate);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task CanRentMovie_WithAvailableMovie_ShouldReturnTrue()
        {
            // Arrange
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            var movie = new Movie("Test", "Description", 9.99m);

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _movieDomainService.CanRentMovie(movieId, startDate, endDate);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task GetTrendingMovies_ShouldReturnTop10Movies()
        {
            // Arrange
            var movies = new List<Movie>();
            var baseDate = DateTime.UtcNow.AddDays(-30); // Start from 30 days ago

            for (int i = 0; i < 15; i++)
            {
                var movie = new Movie($"Movie {i}", "Description", 9.99m);
                // Add different number of rentals to each movie
                for (int j = 0; j < i; j++)
                {
                    // Space out rentals by 2 days each to avoid overlap
                    var startDate = baseDate.AddDays(j * 2);
                    var endDate = startDate.AddDays(1);
                    movie.AddRental(new Rental(1, i, startDate, endDate));
                }
                movies.Add(movie);
            }

            _movieRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _movieDomainService.GetTrendingMovies(30);

            // Assert
            result.Should().HaveCount(10);
            // Should be ordered by number of rentals (descending)
            result.Select(m => m.Rentals.Count())
                .Should()
                .BeInDescendingOrder();
        }

        [Test]
        public async Task IsMovieAvailable_StartDateAfterEndDate_ThrowsArgumentException()
        {
            // Arrange
            var movieId = 1;
            var startDate = DateTime.UtcNow.AddDays(7);
            var endDate = DateTime.UtcNow;

            // Act
            Func<Task> act = async () => await _movieDomainService.IsMovieAvailable(movieId, startDate, endDate);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Start date must be before end date");
        }

        [Test]
        public async Task IsMovieAvailable_WithNonExistentMovie_ShouldReturnFalse()
        {
            // Arrange
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync((Movie)null);

            // Act
            var result = await _movieDomainService.IsMovieAvailable(movieId, startDate, endDate);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task IsMovieAvailable_WithAvailableMovie_ShouldReturnTrue()
        {
            // Arrange
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            var movie = new Movie("Test", "Description", 9.99m);

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _movieDomainService.IsMovieAvailable(movieId, startDate, endDate);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task IsMovieAvailable_WithUnavailableMovie_ShouldReturnFalse()
        {
            // Arrange
            int movieId = 1;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);

            var movie = new Movie("Test", "Description", 9.99m);
            movie.AddRental(new Rental(1, movieId, startDate, endDate));

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _movieDomainService.IsMovieAvailable(movieId, startDate, endDate);

            // Assert
            result.Should().BeFalse();
        }
    }
}