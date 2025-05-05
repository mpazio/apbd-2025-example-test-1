using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using APBD_example_test1_2025.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_example_test1_2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IDbService _dbService;
        public CustomersController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}/rentals")]
        public async Task<IActionResult> GetCustomerRentals(int id)
        {
            try
            {
                var res = await _dbService.GetRentalsForCustomerByIdAsync(id);
                return Ok(res);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{id}/rentals")]
        public async Task<IActionResult> AddNewRental(int id, CreateRentalRequestDto createRentalRequest)
        {
            if (!createRentalRequest.Movies.Any())
            {
                return BadRequest("At least one item is required.");
            }

            try
            {
                await _dbService.AddNewRentalAsync(id, createRentalRequest);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            
            return CreatedAtAction(nameof(GetCustomerRentals), new { id }, createRentalRequest);
        }    
    }
}
