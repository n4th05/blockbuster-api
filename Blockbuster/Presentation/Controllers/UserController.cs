using System.ComponentModel.DataAnnotations;
using Blockbuster.Application.DTOs;
using Blockbuster.Application.Exceptions;
using Blockbuster.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blockbuster.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserAppService _userAppService;

        public UserController(IUserAppService userAppService)
        {
            _userAppService = userAppService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _userAppService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _userAppService.GetUserAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("with-active-rentals")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsersWithActiveRentals()
        {
            var users = await _userAppService.GetUsersWithActiveRentalsAsync();
            return Ok(users);
        }

        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsersWhoRentedMovie(int movieId)
        {
            var users = await _userAppService.GetUsersWhoRentedMovieAsync(movieId);
            return Ok(users);
        }

        [HttpGet("with-rental-history")]
        public async Task<ActionResult<IEnumerable<UserRentalHistoryDTO>>> GetUsersWithRentalHistory()
        {
            var usersHistory = await _userAppService.GetUsersWithRentalHistoryAsync();
            return Ok(usersHistory);
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser(CreateUserDTO createUserDto)
        {
            try
            {
                var user = await _userAppService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDTO>> UpdateUser(int id, UpdateUserDTO updateUserDto)
        {
            try
            {
                var user = await _userAppService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userAppService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> SearchUsers(
            [FromQuery] string name = null,
            [FromQuery] string email = null,
            [FromQuery] bool? hasActiveRentals = null)
        {
            var searchDto = new UserSearchDTO
            {
                Name = name,
                Email = email,
                HasActiveRentals = hasActiveRentals
            };

            var users = await _userAppService.SearchUsersAsync(searchDto);
            return Ok(users);
        }
    }
}