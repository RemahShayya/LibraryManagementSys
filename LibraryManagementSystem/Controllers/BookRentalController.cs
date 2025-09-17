using AutoMapper;
using LibraryManagementSystem.API.DTO;
using LibraryManagementSystem.API.DTO.Requests;
using LibraryManagementSystem.API.Services;
using LibraryManagementSystem.API.Services.IServices;
using LibraryManagementSystem.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookRentalController : ControllerBase
    {
        private readonly IBookRentalService _rentalService;
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public BookRentalController(IBookRentalService rentalService, IBookService bookService, IUserService userService, IMapper mapper, IConfiguration configuration)
        {
            _rentalService = rentalService;
            _bookService = bookService;
            _userService = userService;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpPost("rent")]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IActionResult> RentBook(CreateBookRentalRequest request)
        {
            var book = await _bookService.GetBookById(request.BookId);
            if (book == null)
            {
                return NotFound("Book not found.");
            }
            var customer = await _userService.GetUserById(request.CustomerId);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }
            var pricePerDay = _configuration.GetValue<double>("RentalSettings:PricePerDay");
            var days = (request.RentEndDate - DateTime.UtcNow).Days;
            var rent = new BookRentals
            {
                BookId = request.BookId,
                CustomerId = request.CustomerId,
                RentStartDate = DateTime.UtcNow,
                RentEndDate = request.RentEndDate,
                Quantity = request.Quantity,
                Price = pricePerDay * days * request.Quantity,
            };
            await _rentalService.AddBookRental(rent);
            await _rentalService.Save(rent);
            return Ok("Rental Added Successfully");
        }

        [HttpPost("return/{rentalId}")]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IActionResult> ReturnBook(Guid rentalId)
        {
            var rental = await _rentalService.GetBookRentalByIdWithIncludes(rentalId);
            if (rental == null)
            {
                return NotFound("Rental not found.");
            }

            rental.RentEndDate = DateTime.UtcNow;

            var days = (rental.RentEndDate - rental.RentStartDate).Days;
            var pricePerDay = _configuration.GetValue<double>("RentalSettings:PricePerDay");
            rental.Price = days * pricePerDay * rental.Quantity;

            _rentalService.Delete(rental);
            await _rentalService.Save(rental);

            var rentalDto = _mapper.Map<BookRentalDTO>(rental);
            return Ok(rentalDto);
        }

        [HttpDelete("{rentalId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRental(Guid rentalId)
        {
            var bookRental = _rentalService.GetBookRentalById(rentalId);
            if (bookRental == null)
                return NotFound("Rental not found.");

            _rentalService.Delete(await bookRental);
            await _rentalService.Save(await bookRental);
            return Ok("Rental deleted successfully.");
        }

        [HttpPut("{rentalId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRental(Guid rentalId, [FromBody] CreateBookRentalRequest request)
        {
            var rental = _rentalService.GetBookRentalById(rentalId);
            var book = _bookService.GetBookById(request.BookId);
            var customer = _userService.GetUserById(request.CustomerId);

            if (customer == null || book == null || rental == null)
                return NotFound("Wrong Rental, Book, or Customer ID.");

            rental.Result.CustomerId = request.CustomerId;
            rental.Result.BookId = request.BookId;
            rental.Result.RentEndDate = request.RentEndDate;
            rental.Result.Quantity = request.Quantity;
            rental.Result.Price = (request.Quantity * (rental.Result.RentEndDate - rental.Result.RentStartDate).Days) * _configuration.GetValue<double>("RentalSettings:PricePerDay");
            await _rentalService.Save(await rental);
            return Ok("Rental updated successfully.");
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<ActionResult<BookRentalDTO>> GetAllRentals()
        {
            var rentals = await _rentalService.GetAllBookRentalsWithIncludes();
            var rentalDto = _mapper.Map<IEnumerable<BookRentalDTO>>(rentals);
            return Ok(rentalDto);
        }

        [HttpGet("{rentalId}")]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<ActionResult<IEnumerable<BookRentalDTO>>> GetRentalById(Guid rentalId)
        {
            var rental = await _rentalService.GetBookRentalByIdWithIncludes(rentalId);
            if (rental == null) return NotFound("Rental Not Found");
            var rentalDto=_mapper.Map<BookRentalDTO>(rental);
            return Ok(rentalDto);
        }

        [HttpGet("customer/{email}/summary")]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<ActionResult<IEnumerable<BookRentalDTO>>> GetCustomerRentalSummary(string customerEmail)
        {
            var allUsers = await _userService.GetAllUsers(); // returns List<User>
            var customer = allUsers.FirstOrDefault(u => u.Email == customerEmail);
            if (customer == null)
                throw new Exception("Customer not found");

            // Get all rentals with related Book and Customer
            var rentals = await _rentalService.GetAllBookRentalsWithIncludes();


            // Filter rentals for this customer
            var customerRentals = rentals.Where(r => r.CustomerId.ToString() == customer.Id).ToList();

            var customerRentalsDTO = _mapper.Map<IEnumerable<BookRentalDTO>>(customerRentals);
            return Ok(customerRentalsDTO);

        }

        [HttpGet("export-rentals")]
        public IActionResult ExportRentals([FromServices] ExcelExportService excelExportService)
        {
            var rentals = _rentalService.GetAllBookRentalsWithIncludes().Result;
            var fileContent = excelExportService.ExportBookRentalsToExcel(rentals.ToList());
            var fileName = $"BookRentals_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

    }
}
