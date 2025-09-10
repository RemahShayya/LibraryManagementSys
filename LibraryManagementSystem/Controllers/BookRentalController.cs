using LibraryManagementSystem.API.DTO.Requests;
using LibraryManagementSystem.API.Services;
using LibraryManagementSystem.API.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookRentalController : ControllerBase
    {
        private readonly IBookRentalService _rentalService;

        public BookRentalController(IBookRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpPost("rent")]
        public async Task<IActionResult> RentBook(CreateBookRentalRequest request)
        {
            await _rentalService.RentBookAsync(request);
            return Ok("Book rented successfully.");
        }

        [HttpPost("return/{rentalId}")]
        public async Task<IActionResult> ReturnBook(Guid rentalId)
        {
            var rentalDto=await _rentalService.ReturnBookAsync(rentalId);
            return Ok(rentalDto);
        }

        [HttpDelete("{rentalId}")]
        public async Task<IActionResult> DeleteRental(Guid rentalId)
        {
            await _rentalService.DeleteRentalAsync(rentalId);
            return Ok("Rental deleted successfully.");
        }

        [HttpPut("{rentalId}")]
        public async Task<IActionResult> UpdateRental(Guid rentalId, [FromBody] CreateBookRentalRequest request)
        {
            await _rentalService.UpdateRentalAsync(rentalId, request);
            return Ok("Rental updated successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRentals()
        {
            var rentals = await _rentalService.GetAllBookRentalsAsync();
            return Ok(rentals);
        }

        [HttpGet("{rentalId}")]
        public async Task<IActionResult> GetRentalById(Guid rentalId)
        {
            var rental = await _rentalService.GetBookRentalByIdAsync(rentalId);
            return Ok(rental);
        }

        [HttpGet("customer/{email}/summary")]
        public async Task<IActionResult> GetCustomerRentalSummary(string email)
        {
            var summary = await _rentalService.GetCustomerRentalSummaryAsync(email);
            return Ok(summary);
        }

    }
}
