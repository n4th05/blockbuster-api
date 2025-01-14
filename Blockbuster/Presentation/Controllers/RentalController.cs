using System.ComponentModel.DataAnnotations;
using Blockbuster.Application.DTOs;
using Blockbuster.Application.Exceptions;
using Blockbuster.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blockbuster.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalController : ControllerBase
    {
        private readonly IRentalAppService _rentalAppService;

        public RentalController(IRentalAppService rentalAppService)
        {
            _rentalAppService = rentalAppService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RentalDTO>>> GetRentals()
        {
            var rentals = await _rentalAppService.GetAllRentalsAsync();
            return Ok(rentals);
        }

        [HttpGet("{userId}/{movieId}")]
        public async Task<ActionResult<RentalDTO>> GetRental(int userId, int movieId)
        {
            var rental = await _rentalAppService.GetRentalAsync(userId, movieId);
            if (rental == null)
                return NotFound();

            return Ok(rental);
        }

        [HttpPost]
        public async Task<ActionResult<RentalDTO>> CreateRental(CreateRentalDTO createRentalDto)
        {
            try
            {
                var rental = await _rentalAppService.CreateRentalAsync(createRentalDto);
                return CreatedAtAction(
                    nameof(GetRental),
                    new { userId = rental.UserId, movieId = rental.MovieId },
                    rental);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{userId}/{movieId}")]
        public async Task<ActionResult<RentalDTO>> UpdateRental(
            int userId,
            int movieId,
            UpdateRentalDTO updateRentalDto)
        {
            try
            {
                var rental = await _rentalAppService.UpdateRentalAsync(userId, movieId, updateRentalDto);
                if (rental == null)
                    return NotFound();

                return Ok(rental);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{userId}/{movieId}")]
        public async Task<IActionResult> DeleteRental(int userId, int movieId)
        {
            try
            {
                await _rentalAppService.DeleteRentalAsync(userId, movieId);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<RentalDTO>>> GetActiveRentals()
        {
            var rentals = await _rentalAppService.GetActiveRentalsAsync();
            return Ok(rentals);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<RentalDTO>>> GetOverdueRentals()
        {
            var rentals = await _rentalAppService.GetOverdueRentalsAsync();
            return Ok(rentals);
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<RentalDTO>>> GetUpcomingRentals()
        {
            var rentals = await _rentalAppService.GetUpcomingRentalsAsync();
            return Ok(rentals);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RentalDTO>>> SearchRentals(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string userName = null,
            [FromQuery] string movieTitle = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isOverdue = null)
        {
            var searchDto = new RentalSearchDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                UserName = userName,
                MovieTitle = movieTitle,
                IsActive = isActive,
                IsOverdue = isOverdue
            };

            var rentals = await _rentalAppService.SearchRentalsAsync(searchDto);
            return Ok(rentals);
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<RentalStatisticsDTO>> GetRentalStatistics()
        {
            var statistics = await _rentalAppService.GetRentalStatisticsAsync();
            return Ok(statistics);
        }
    }
}