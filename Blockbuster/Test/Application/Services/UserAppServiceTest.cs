using NUnit.Framework;
using Moq;
using FluentAssertions;
using Blockbuster.Application.DTOs;
using Blockbuster.Application.Services;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Blockbuster.Test.Application.Services
{
    [TestFixture]
    public class UserAppServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IUserDomainService> _userDomainServiceMock;
        private UserAppService _userAppService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userDomainServiceMock = new Mock<IUserDomainService>();
            _userAppService = new UserAppService(_userRepositoryMock.Object, _userDomainServiceMock.Object);
        }

        [Test]
        public async Task GetUserAsync_WithValidId_ShouldReturnUserDTO()
        {
            // Arrange
            var user = new User("Test User", "test@user.com", "+5599999999999") { Id = 1 };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            var result = await _userAppService.GetUserAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Name.Should().Be(user.Name);
            result.Email.Should().Be(user.Email);
            result.Phone.Should().Be(user.Phone);
        }

        [Test]
        public async Task GetUserAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userAppService.GetUserAsync(1);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task CreateUserAsync_WithValidData_ShouldReturnUserDTO()
        {
            // Arrange
            var createDto = new CreateUserDTO
            {
                Name = "New User",
                Email = "new@user.com",
                Phone = "+5599999999999"
            };

            _userDomainServiceMock.Setup(x => x.IsEmailUnique(createDto.Email))
                .ReturnsAsync(true);

            User createdUser = null;
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .ReturnsAsync((User user) => user);

            // Act
            var result = await _userAppService.CreateUserAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createDto.Name);
            result.Email.Should().Be(createDto.Email);
            result.Phone.Should().Be(createDto.Phone);
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task CreateUserAsync_WithExistingEmail_ShouldThrowValidationException()
        {
            // Arrange
            var createDto = new CreateUserDTO
            {
                Name = "New User",
                Email = "existing@user.com",
                Phone = "+5599999999999"
            };

            _userDomainServiceMock.Setup(x => x.IsEmailUnique(createDto.Email))
                .ReturnsAsync(false);

            // Act & Assert
            await FluentActions.Invoking(() =>
                _userAppService.CreateUserAsync(createDto))
                .Should().ThrowAsync<ValidationException>()
                .WithMessage("Email already exists");
        }

        [Test]
        public async Task UpdateUserAsync_WithValidData_ShouldReturnUpdatedUserDTO()
        {
            // Arrange
            var userId = 1;
            var user = new User("Old Name", "old@email.com", "+5599999999999") { Id = userId };
            var updateDto = new UpdateUserDTO
            {
                Name = "Updated Name",
                Email = "new@email.com",
                Phone = "+5599999999988"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _userDomainServiceMock.Setup(x => x.IsEmailUnique(updateDto.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _userAppService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(updateDto.Name);
            result.Email.Should().Be(updateDto.Email);
            result.Phone.Should().Be(updateDto.Phone);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task UpdateUserAsync_WithSameEmail_ShouldNotCheckUniqueness()
        {
            // Arrange
            var userId = 1;
            var user = new User("Old Name", "same@email.com", "+5599999999999") { Id = userId };
            var updateDto = new UpdateUserDTO
            {
                Name = "Updated Name",
                Email = "same@email.com", // Same email, different case
                Phone = "+5599999999988"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userAppService.UpdateUserAsync(userId, updateDto);

            // Assert
            result.Should().NotBeNull();
            _userDomainServiceMock.Verify(x => x.IsEmailUnique(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task DeleteUserAsync_WithValidIdAndNoActiveRentals_ShouldDeleteUser()
        {
            // Arrange
            var userId = 1;
            var user = new User("Test User", "test@user.com", "+5599999999999") { Id = userId };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _userDomainServiceMock.Setup(x => x.CanDeleteUser(userId))
                .ReturnsAsync(true);

            // Act
            await _userAppService.DeleteUserAsync(userId);

            // Assert
            _userRepositoryMock.Verify(x => x.DeleteAsync(user), Times.Once);
        }

        [Test]
        public async Task DeleteUserAsync_WithActiveRentals_ShouldThrowValidationException()
        {
            // Arrange
            var userId = 1;
            var user = new User("Test User", "test@user.com", "+5599999999999") { Id = userId };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _userDomainServiceMock.Setup(x => x.CanDeleteUser(userId))
                .ReturnsAsync(false);

            // Act & Assert
            await FluentActions.Invoking(() =>
                _userAppService.DeleteUserAsync(userId))
                .Should().ThrowAsync<ValidationException>()
                .WithMessage("Cannot delete user with active rentals");
        }

        [Test]
        public async Task SearchUsersAsync_WithCriteria_ShouldReturnFilteredUsers()
        {
            // Arrange
            var currentDate = DateTime.UtcNow;
            var users = new List<User>
    {
        CreateUserWithRentals("John Doe", "john@test.com", 2, currentDate),
        CreateUserWithRentals("Jane Smith", "jane@test.com", 0, currentDate),
        CreateUserWithRentals("Bob Johnson", "bob@test.com", 1, currentDate)
    };

            var searchDto = new UserSearchDTO
            {
                Name = "John",
                Email = "test.com",
                HasActiveRentals = true
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _userAppService.SearchUsersAsync(searchDto);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(u =>
                u.Name.Contains(searchDto.Name, StringComparison.OrdinalIgnoreCase) &&
                u.Email.Contains(searchDto.Email, StringComparison.OrdinalIgnoreCase));
        }

        private User CreateUserWithRentals(string name, string email, int numberOfActiveRentals, DateTime currentDate)
        {
            var user = new User(name, email, "+5599999999999");

            // Add active rentals (if any)
            for (int i = 0; i < numberOfActiveRentals; i++)
            {
                var startDate = currentDate.AddDays(-1);
                var endDate = currentDate.AddDays(5); // Future end date makes it active
                user.AddRental(new Rental(user.Id, i + 1, startDate, endDate));
            }

            return user;
        }

        [Test]
        public async Task GetUsersWithRentalHistoryAsync_ShouldReturnUsersWithHistory()
        {
            // Arrange
            var users = new List<User>();
            var baseDate = DateTime.UtcNow.AddDays(-30);

            for (int i = 1; i <= 3; i++)
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

            // Explicitly order the users by rental count descending before returning
            _userDomainServiceMock.Setup(x => x.GetUsersWithRentalHistory())
                .ReturnsAsync(users.OrderByDescending(u => u.Rentals.Count).ToList());

            // Act
            var result = await _userAppService.GetUsersWithRentalHistoryAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(u => u.RentalCount).Should().BeInDescendingOrder();
            result.First().RentalCount.Should().Be(3);  // User 3 has most rentals
            result.Last().RentalCount.Should().Be(1);   // User 1 has least rentals
            result.All(u => u.LastRental != null).Should().BeTrue();
            result.All(u => u.Id > 0).Should().BeTrue(); // Ensure IDs are set
        }
    }
}