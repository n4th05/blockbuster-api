using Blockbuster.Application.DTOs;

namespace Blockbuster.Domain.Interfaces
{
    public interface IMovieAppService
    {
        Task<MovieDTO> GetMovieAsync(int id);
        Task<IEnumerable<MovieDTO>> GetAllMoviesAsync();
        Task<MovieDTO> CreateMovieAsync(CreateMovieDTO createMovieDto);
        Task<MovieDTO> UpdateMovieAsync(int id, UpdateMovieDTO updateMovieDto);
        Task DeleteMovieAsync(int id);
        Task<IEnumerable<MovieDTO>> GetTrendingMoviesAsync();
        Task<IEnumerable<MovieDTO>> GetAvailableMoviesAsync();
        Task<IEnumerable<MovieDTO>> SearchMoviesAsync(MovieSearchDTO searchDto);
        Task<MovieStatisticsDTO> GetMovieStatisticsAsync();
    }
}