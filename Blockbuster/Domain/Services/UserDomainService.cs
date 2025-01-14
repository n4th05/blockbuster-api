using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;

namespace Blockbuster.Domain.Services
{
    public class UserDomainService : IUserDomainService
    {
        private readonly IUserRepository _userRepository;

        public UserDomainService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> CanDeleteUser(int userId)
        {
            var hasActiveRentals = await HasActiveRentals(userId);
            return !hasActiveRentals;
        }

        public async Task<bool> IsEmailUnique(string email)
        {
            var users = await _userRepository.GetAllAsync();
            return !users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<User>> GetUsersWithRentalHistory()
        {
            var users = await _userRepository.GetAllAsync();
            return users.OrderByDescending(u => u.Rentals.Count);
        }

        public async Task<bool> HasActiveRentals(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            var currentDate = DateTime.UtcNow;
            return user.HasActiveRentals(currentDate);
        }

        public async Task<IEnumerable<User>> GetTopUsersWithMostRentals(int count)
        {
            var users = await _userRepository.GetAllAsync();
            return users
                .OrderByDescending(u => u.Rentals.Count)
                .Take(count);
        }
    }
}