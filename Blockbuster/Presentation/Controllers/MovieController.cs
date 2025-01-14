using Blockbuster.Application.DTOs;
using Blockbuster.Application.Exceptions;
using Blockbuster.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blockbuster.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieAppService _movieAppService;

        public MovieController(IMovieAppService movieAppService)
        {
            _movieAppService = movieAppService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieDTO>>> GetMovies()
        {
            var movies = await _movieAppService.GetAllMoviesAsync();
            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDTO>> GetMovie(int id)
        {
            var movie = await _movieAppService.GetMovieAsync(id);
            if (movie == null)
                return NotFound();
            return Ok(movie);
        }

        [HttpGet("trending")]
        public async Task<ActionResult<IEnumerable<MovieDTO>>> GetTrendingMovies()
        {
            var trendingMovies = await _movieAppService.GetTrendingMoviesAsync();
            return Ok(trendingMovies);
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<MovieDTO>>> GetAvailableMovies()
        {
            var availableMovies = await _movieAppService.GetAvailableMoviesAsync();
            return Ok(availableMovies);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MovieDTO>>> SearchMovies(
            [FromQuery] string title = null,
            [FromQuery] string description = null,
            [FromQuery] decimal? minValue = null,
            [FromQuery] decimal? maxValue = null,
            [FromQuery] bool? isAvailable = null)
        {
            var searchDto = new MovieSearchDTO
            {
                Title = title,
                Description = description,
                MinValue = minValue,
                MaxValue = maxValue,
                IsAvailable = isAvailable
            };

            var movies = await _movieAppService.SearchMoviesAsync(searchDto);
            return Ok(movies);
        }

        [HttpPost]
        public async Task<ActionResult<MovieDTO>> CreateMovie(CreateMovieDTO createMovieDto)
        {
            var movie = await _movieAppService.CreateMovieAsync(createMovieDto);
            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MovieDTO>> UpdateMovie(int id, UpdateMovieDTO updateMovieDto)
        {
            var movie = await _movieAppService.UpdateMovieAsync(id, updateMovieDto);
            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                await _movieAppService.DeleteMovieAsync(id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<MovieStatisticsDTO>> GetMovieStatistics()
        {
            var statistics = await _movieAppService.GetMovieStatisticsAsync();
            return Ok(statistics);
        }
    }
}