using NUnit.Framework;
using Moq;
using FluentAssertions;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Domain.Services;

namespace Blockbuster.Test.Domain.Services
{
    [TestFixture]
    public class UserDomainServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private UserDomainService _userDomainService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userDomainService = new UserDomainService(_userRepositoryMock.Object);
        }

        [Test]
        public async Task CanDeleteUser_WithNoActiveRentals_ShouldReturnTrue()
        {
            // Arrange
            int userId = 1;
            var user = new User("Test User", "test@email.com", "+5599999999999");
            var currentDate = DateTime.UtcNow;

            // Add only past rentals
            user.AddRental(new Rental(userId, 1, currentDate.AddDays(-20), currentDate.AddDays(-13)));
            user.AddRental(new Rental(userId, 2, currentDate.AddDays(-10), currentDate.AddDays(-3)));

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userDomainService.CanDeleteUser(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task CanDeleteUser_WithActiveRentals_ShouldReturnFalse()
        {
            // Arrange
            int userId = 1;
            var user = new User("Test User", "test@email.com", "+5599999999999");
            var currentDate = DateTime.UtcNow;

            // Add an active rental
            user.AddRental(new Rental(userId, 1, currentDate.AddDays(-3), currentDate.AddDays(4)));

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userDomainService.CanDeleteUser(userId);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task CanDeleteUser_WithNonExistentUser_ShouldReturnTrue()
        {
            // Arrange
            int userId = 1;
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userDomainService.CanDeleteUser(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task IsEmailUnique_WithUniqueEmail_ShouldReturnTrue()
        {
            // Arrange
            var email = "new@email.com";
            var existingUsers = new List<User>
            {
                new User("User 1", "user1@email.com", "+5599999999999"),
                new User("User 2", "user2@email.com", "+5599999999999")
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(existingUsers);

            // Act
            var result = await _userDomainService.IsEmailUnique(email);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task IsEmailUnique_WithExistingEmail_ShouldReturnFalse()
        {
            // Arrange
            var email = "user1@email.com";
            var existingUsers = new List<User>
            {
                new User("User 1", "user1@email.com", "+5599999999999"),
                new User("User 2", "user2@email.com", "+5599999999999")
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(existingUsers);

            // Act
            var result = await _userDomainService.IsEmailUnique(email);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task IsEmailUnique_WithExistingEmailDifferentCase_ShouldReturnFalse()
        {
            // Arrange
            var email = "USER1@email.com";
            var existingUsers = new List<User>
            {
                new User("User 1", "user1@email.com", "+5599999999999"),
                new User("User 2", "user2@email.com", "+5599999999999")
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(existingUsers);

            // Act
            var result = await _userDomainService.IsEmailUnique(email);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task GetUsersWithRentalHistory_ShouldReturnOrderedByRentalCount()
        {
            // Arrange
            var currentDate = DateTime.UtcNow;
            var users = new List<User>
            {
                CreateUserWithRentals("User 1", "user1@email.com", 3, currentDate),
                CreateUserWithRentals("User 2", "user2@email.com", 1, currentDate),
                CreateUserWithRentals("User 3", "user3@email.com", 5, currentDate)
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _userDomainService.GetUsersWithRentalHistory();

            // Assert
            result.Should().HaveCount(3);
            result.First().Name.Should().Be("User 3"); // Most rentals (5)
            result.Last().Name.Should().Be("User 2");  // Least rentals (1)
        }

        [Test]
        public async Task HasActiveRentals_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            int userId = 1;
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userDomainService.HasActiveRentals(userId);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task HasActiveRentals_WithActiveRental_ShouldReturnTrue()
        {
            // Arrange
            int userId = 1;
            var user = new User("Test User", "test@email.com", "+5599999999999");
            var currentDate = DateTime.UtcNow;

            user.AddRental(new Rental(userId, 1, currentDate.AddDays(-3), currentDate.AddDays(4)));

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userDomainService.HasActiveRentals(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task GetTopUsersWithMostRentals_ShouldReturnRequestedNumberOfUsers()
        {
            // Arrange
            var currentDate = DateTime.UtcNow;
            var users = new List<User>
            {
                CreateUserWithRentals("User 1", "user1@email.com", 3, currentDate),
                CreateUserWithRentals("User 2", "user2@email.com", 1, currentDate),
                CreateUserWithRentals("User 3", "user3@email.com", 5, currentDate),
                CreateUserWithRentals("User 4", "user4@email.com", 4, currentDate)
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _userDomainService.GetTopUsersWithMostRentals(2);

            // Assert
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("User 3"); // Most rentals (5)
            result.Last().Name.Should().Be("User 4");  // Second most rentals (4)
        }

        private User CreateUserWithRentals(string name, string email, int numberOfRentals, DateTime currentDate)
        {
            var user = new User(name, email, "+5599999999999");
            for (int i = 0; i < numberOfRentals; i++)
            {
                user.AddRental(new Rental(1, i + 1, currentDate.AddDays(-10), currentDate.AddDays(-3)));
            }
            return user;
        }
    }
}