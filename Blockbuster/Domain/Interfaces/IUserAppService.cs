using Blockbuster.Application.DTOs;

namespace Blockbuster.Application.Interfaces
{
    public interface IUserAppService
    {
        Task<UserDTO> GetUserAsync(int id);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto);
        Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO updateUserDto);
        Task DeleteUserAsync(int id);
        Task<IEnumerable<UserDTO>> GetUsersWithActiveRentalsAsync();
        Task<IEnumerable<UserDTO>> GetUsersWhoRentedMovieAsync(int movieId);
        Task<IEnumerable<UserDTO>> SearchUsersAsync(UserSearchDTO searchDto);
        Task<IEnumerable<UserRentalHistoryDTO>> GetUsersWithRentalHistoryAsync();
    }
}