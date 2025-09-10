using LibraryManagementSystem.API.DTO;
using LibraryManagementSystem.API.DTO.Requests;
using LibraryManagementSystem.API.Services.IServices;
using LibraryManagementSystem.Data.Entities;
using LibraryManagmentSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Services
{
    public class BookRentalService : IBookRentalService
    {
        private readonly LibraryContext _context;
        private readonly IConfiguration _configuration;

        public BookRentalService(LibraryContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task RentBookAsync(CreateBookRentalRequest request)
        {
            var customer = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.CustomerEmail);
            if (customer == null)
                throw new Exception("Customer not found");

            var book = _context.Books.Find(request.BookId);
            if (book == null)
                throw new Exception("Book not found");

            var pricePerDay = _configuration.GetValue<double>("RentalSettings:PricePerDay");
            var days = (request.RentEndDate - DateTime.UtcNow).Days;

            var rental = new BookRentals
            {
                BookId = request.BookId,
                CustomerId = customer.Id,
                RentStartDate = DateTime.UtcNow,
                RentEndDate = request.RentEndDate,
                Quantity = request.Quantity,
                Price = days * pricePerDay * request.Quantity
            };

            _context.BookRentals.Add(rental);
            await _context.SaveChangesAsync();
        }

        public async Task<BookRentalDTO> ReturnBookAsync(Guid rentalId)
        {
            var rental = await _context.BookRentals.Include(r => r.Customer)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == rentalId);
            if (rental == null)
                throw new Exception("Rental not found");

            rental.RentEndDate = DateTime.UtcNow;

            var days = (rental.RentEndDate - rental.RentStartDate).Days;
            var pricePerDay = _configuration.GetValue<double>("RentalSettings:PricePerDay");
            rental.Price = days * pricePerDay * rental.Quantity;

            _context.BookRentals.Remove(rental);
            await _context.SaveChangesAsync();

            return new BookRentalDTO
            {
                BookName = rental.Book.Title,
                CustomerEmail = rental.Customer.Email,
                RentStartDate = rental.RentStartDate,
                RentEndDate = rental.RentEndDate,
                Quantity = rental.Quantity,
                Price = rental.Price
            };
        }

        public async Task DeleteRentalAsync(Guid rentalId)
        {
            var rental = await _context.BookRentals.FindAsync(rentalId);
            if (rental == null)
                throw new Exception("Rental not found");

            _context.BookRentals.Remove(rental);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRentalAsync(Guid rentalId, CreateBookRentalRequest request)
        {
            var rental = await _context.BookRentals.FindAsync(rentalId);
            if (rental == null)
                throw new Exception("Rental not found");
            var customer = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.CustomerEmail);
            if (customer == null)
                throw new Exception("Customer not found");
            var book = _context.Books.Find(request.BookId);
            if (book == null)
                throw new Exception("Book not found");

            rental.CustomerId = customer.Id;
            rental.Quantity = request.Quantity;
            rental.RentEndDate = request.RentEndDate;
            rental.BookId = request.BookId;

            var days = (request.RentEndDate - rental.RentStartDate).Days;
            var pricePerDay = _configuration.GetValue<double>("RentalSettings:PricePerDay");

            rental.Price= days * pricePerDay * rental.Quantity;
            _context.BookRentals.Update(rental);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BookRentalDTO>> GetAllBookRentalsAsync()
        {
            return await _context.BookRentals
                .Include(r => r.Book)
                .Include(r => r.Customer)
                .Select(r => new BookRentalDTO
                {
                    BookName = r.Book.Title,
                    CustomerEmail = r.Customer.Email,
                    RentStartDate = r.RentStartDate,
                    RentEndDate = r.RentEndDate,
                    Quantity = r.Quantity,
                    Price = r.Price
                })
                .ToListAsync();
        }

        public async Task<BookRentalDTO> GetBookRentalByIdAsync(Guid rentalId)
        {
            var rental = await _context.BookRentals
                .Include(r => r.Book)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null)
                throw new Exception("Rental not found");

            return new BookRentalDTO
            {
                BookName = rental.Book.Title,
                CustomerEmail = rental.Customer.Email,
                RentStartDate = rental.RentStartDate,
                RentEndDate = rental.RentEndDate,
                Quantity = rental.Quantity,
                Price = rental.Price
            };
        }

        public async Task<CustomerRentalSummaryDTO> GetCustomerRentalSummaryAsync(string customerEmail)
        {
            var customer = await _context.Users.FirstOrDefaultAsync(u => u.Email == customerEmail);
            if (customer == null)
                throw new Exception("Customer not found");

            var rentals = await _context.BookRentals
                .Include(r => r.Book)
                .Where(r => r.CustomerId == customer.Id)
                .ToListAsync();

            return new CustomerRentalSummaryDTO
            {
                CustomerEmail = customerEmail,
                BookTitles = rentals.Select(r => r.Book.Title).ToList(),
                TotalPrice = rentals.Sum(r => r.Price)
            };
        }

    }
}
