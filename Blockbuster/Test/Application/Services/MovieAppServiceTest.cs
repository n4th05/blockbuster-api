using NUnit.Framework;
using Moq;
using FluentAssertions;
using Blockbuster.Application.DTOs;
using Blockbuster.Application.Services;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Application.Exceptions;

namespace Blockbuster.Test.Application.Services
{
    [TestFixture]
    public class MovieAppServiceTests
    {
        private Mock<IMovieRepository> _movieRepositoryMock;
        private Mock<IMovieDomainService> _movieDomainServiceMock;
        private MovieAppService _movieAppService;

        [SetUp]
        public void Setup()
        {
            _movieRepositoryMock = new Mock<IMovieRepository>();
            _movieDomainServiceMock = new Mock<IMovieDomainService>();
            _movieAppService = new MovieAppService(
                _movieRepositoryMock.Object,
                _movieDomainServiceMock.Object
            );
        }

        [Test]
        public async Task GetMovieAsync_WithValidId_ShouldReturnMovieDTO()
        {
            // Arrange
            int movieId = 1;
            var movie = new Movie("Test Movie", "Description", 9.99m) { Id = movieId };

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _movieAppService.GetMovieAsync(movieId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(movieId);
            result.Title.Should().Be("Test Movie");
            result.Description.Should().Be("Description");
            result.Value.Should().Be(9.99m);
        }

        [Test]
        public async Task GetMovieAsync_WithDeletedMovie_ShouldReturnNull()
        {
            // Arrange
            int movieId = 1;
            var movie = new Movie("Test Movie", "Description", 9.99m) { Id = movieId };
            movie.Delete();

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _movieAppService.GetMovieAsync(movieId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task CreateMovieAsync_WithValidData_ShouldReturnCreatedMovieDTO()
        {
            // Arrange
            var createDto = new CreateMovieDTO
            {
                Title = "New Movie",
                Description = "New Description",
                Value = 14.99m
            };

            Movie createdMovie = null;
            _movieRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Movie>()))
                .Callback<Movie>(movie => createdMovie = movie)
                .ReturnsAsync((Movie movie) => movie);

            // Act
            var result = await _movieAppService.CreateMovieAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(createDto.Title);
            result.Description.Should().Be(createDto.Description);
            result.Value.Should().Be(createDto.Value);

            _movieRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Once);
        }

        [Test]
        public async Task GetAllMoviesAsync_ShouldReturnOnlyNonDeletedMovies()
        {
            // Arrange
            var movies = new List<Movie>
            {
                new Movie("Movie 1", "Description 1", 9.99m),
                new Movie("Movie 2", "Description 2", 14.99m),
                new Movie("Movie 3", "Description 3", 19.99m)
            };
            movies[2].Delete(); // Mark the last movie as deleted

            _movieRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _movieAppService.GetAllMoviesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().NotContain(m => m.Title == "Movie 3");
        }

        [Test]
        public async Task UpdateMovieAsync_WithValidData_ShouldReturnUpdatedMovieDTO()
        {
            // Arrange
            int movieId = 1;
            var movie = new Movie("Old Title", "Old Description", 9.99m) { Id = movieId };
            var updateDto = new UpdateMovieDTO
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Value = 19.99m
            };

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            var result = await _movieAppService.UpdateMovieAsync(movieId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(updateDto.Title);
            result.Description.Should().Be(updateDto.Description);
            result.Value.Should().Be(updateDto.Value);

            _movieRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Movie>()), Times.Once);
        }

        [Test]
        public async Task DeleteMovieAsync_WithValidId_ShouldMarkMovieAsDeleted()
        {
            // Arrange
            int movieId = 1;
            var movie = new Movie("Test Movie", "Description", 9.99m) { Id = movieId };

            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync(movie);

            // Act
            await _movieAppService.DeleteMovieAsync(movieId);

            // Assert
            movie.DeletedAt.Should().NotBeNull();
            _movieRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Movie>(m => m.DeletedAt != null)), Times.Once);
        }

        [Test]
        public async Task DeleteMovieAsync_WithNonExistentMovie_ShouldThrowNotFoundException()
        {
            // Arrange
            int movieId = 1;
            _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId))
                .ReturnsAsync((Movie)null);

            // Act & Assert
            await FluentActions.Invoking(() =>
                _movieAppService.DeleteMovieAsync(movieId))
                .Should().ThrowAsync<NotFoundException>()
                .WithMessage("Movie not found");
        }

        [Test]
        public async Task GetTrendingMoviesAsync_ShouldReturnMappedTrendingMovies()
        {
            // Arrange
            var trendingMovies = new List<Movie>
            {
                new Movie("Trending 1", "Description 1", 9.99m),
                new Movie("Trending 2", "Description 2", 14.99m)
            };

            _movieDomainServiceMock.Setup(x => x.GetTrendingMovies(30))
                .ReturnsAsync(trendingMovies);

            // Act
            var result = await _movieAppService.GetTrendingMoviesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Select(m => m.Title).Should().Contain(new[] { "Trending 1", "Trending 2" });
        }

        [Test]
        public async Task SearchMoviesAsync_WithCriteria_ShouldReturnFilteredMovies()
        {
            // Arrange
            var movies = new List<Movie>
            {
                new Movie("Action Movie", "Action packed", 9.99m),
                new Movie("Drama Movie", "Dramatic story", 14.99m),
                new Movie("Action Drama", "Mixed genre", 19.99m)
            };

            var searchDto = new MovieSearchDTO
            {
                Title = "Action",
                MinValue = 10m,
                MaxValue = 20m,
                IsAvailable = true
            };

            _movieRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _movieAppService.SearchMoviesAsync(searchDto);

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Action Drama");
        }

        [Test]
        public async Task GetMovieStatisticsAsync_ShouldReturnCorrectStatistics()
        {
            // Arrange
            var movies = new List<Movie>
            {
                CreateMovieWithRentals("Movie 1", 3),
                CreateMovieWithRentals("Movie 2", 1),
                CreateMovieWithRentals("Movie 3", 5)
            };

            _movieRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(movies);

            // Act
            var result = await _movieAppService.GetMovieStatisticsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalMovies.Should().Be(3);
            result.TopRentedMovies.Should().HaveCount(3);
            result.TopRentedMovies.First().RentalCount.Should().Be(5);
            result.AverageRentalDuration.Should().BeGreaterThan(0);
        }

        private Movie CreateMovieWithRentals(string title, int numberOfRentals)
        {
            var movie = new Movie(title, "Description", 9.99m);
            var baseDate = DateTime.UtcNow.AddDays(-30); // Start 30 days ago

            for (int i = 0; i < numberOfRentals; i++)
            {
                // Space out rentals by 2 days each to avoid overlap
                var startDate = baseDate.AddDays(i * 2);
                var endDate = startDate.AddDays(1);
                movie.AddRental(new Rental(1, movie.Id, startDate, endDate));
            }

            return movie;
        }
    }
}