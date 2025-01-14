using Blockbuster.Application.DTOs;
using Blockbuster.Application.Exceptions;
using Blockbuster.Application.Interfaces;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Blockbuster.Application.Services
{
    public class UserAppService : IUserAppService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserDomainService _userDomainService;

        public UserAppService(IUserRepository userRepository, IUserDomainService userDomainService)
        {
            _userRepository = userRepository;
            _userDomainService = userDomainService;
        }

        public async Task<UserDTO> GetUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToDTO(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDTO);
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto)
        {
            if (!await _userDomainService.IsEmailUnique(createUserDto.Email))
                throw new ValidationException("Email already exists");

            var user = new User(
                createUserDto.Name,
                createUserDto.Email,
                createUserDto.Phone
            );

            await _userRepository.AddAsync(user);
            return MapToDTO(user);
        }

        public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            var isEmailChanged = !user.Email.Equals(updateUserDto.Email, StringComparison.OrdinalIgnoreCase);
            if (isEmailChanged && !await _userDomainService.IsEmailUnique(updateUserDto.Email))
                throw new ValidationException("Email already exists");

            user.Update(
                updateUserDto.Name,
                updateUserDto.Email,
                updateUserDto.Phone
            );

            await _userRepository.UpdateAsync(user);
            return MapToDTO(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("User not found");

            if (!await _userDomainService.CanDeleteUser(id))
                throw new ValidationException("Cannot delete user with active rentals");

            await _userRepository.DeleteAsync(user);
        }

        public async Task<IEnumerable<UserDTO>> GetUsersWithActiveRentalsAsync()
        {
            var users = await _userRepository.GetUsersWithActiveRentalsAsync();
            return users.Select(MapToDTO);
        }

        public async Task<IEnumerable<UserDTO>> GetUsersWhoRentedMovieAsync(int movieId)
        {
            var users = await _userRepository.GetUsersWhoRentedMovieAsync(movieId);
            return users.Select(MapToDTO);
        }

        public async Task<IEnumerable<UserDTO>> SearchUsersAsync(UserSearchDTO searchDto)
        {
            var users = await _userRepository.GetAllAsync();
            var query = users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchDto.Name))
                query = query.Where(u => u.Name.Contains(searchDto.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(searchDto.Email))
                query = query.Where(u => u.Email.Contains(searchDto.Email, StringComparison.OrdinalIgnoreCase));

            if (searchDto.HasActiveRentals.HasValue)
            {
                var currentDate = DateTime.UtcNow;
                query = query.Where(u => u.HasActiveRentals(currentDate) == searchDto.HasActiveRentals.Value);
            }

            return query.Select(MapToDTO);
        }

        public async Task<IEnumerable<UserRentalHistoryDTO>> GetUsersWithRentalHistoryAsync()
        {
            var users = await _userDomainService.GetUsersWithRentalHistory();
            return users.Select(MapToRentalHistoryDTO);
        }

        private UserDTO MapToDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        private UserRentalHistoryDTO MapToRentalHistoryDTO(User user)
        {
            var lastRental = user.Rentals
                .OrderByDescending(r => r.StartDate)
                .FirstOrDefault();

            return new UserRentalHistoryDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RentalCount = user.Rentals.Count,
                LastRental = lastRental == null ? null : new LastRentalDTO
                {
                    MovieId = lastRental.MovieId,
                    StartDate = lastRental.StartDate,
                    EndDate = lastRental.EndDate
                }
            };
        }
    }
}