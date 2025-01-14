using Blockbuster.Application.DTOs;

namespace Blockbuster.Application.Interfaces
{
    public interface IRentalAppService
    {
        Task<RentalDTO> GetRentalAsync(int userId, int movieId);
        Task<IEnumerable<RentalDTO>> GetAllRentalsAsync();
        Task<RentalDTO> CreateRentalAsync(CreateRentalDTO createRentalDto);
        Task<RentalDTO> UpdateRentalAsync(int userId, int movieId, UpdateRentalDTO updateRentalDto);
        Task DeleteRentalAsync(int userId, int movieId);
        Task<IEnumerable<RentalDTO>> GetActiveRentalsAsync();
        Task<IEnumerable<RentalDTO>> GetOverdueRentalsAsync();
        Task<IEnumerable<RentalDTO>> GetUpcomingRentalsAsync();
        Task<IEnumerable<RentalDTO>> SearchRentalsAsync(RentalSearchDTO searchDto);
        Task<RentalStatisticsDTO> GetRentalStatisticsAsync();
    }
}