using LibraryManagementSystem.API.DTO;
using LibraryManagementSystem.API.DTO.Requests;

namespace LibraryManagementSystem.API.Services.IServices
{
    public interface IBookRentalService
    {
        Task RentBookAsync(CreateBookRentalRequest request);
        Task<BookRentalDTO> ReturnBookAsync(Guid rentalId);
        Task DeleteRentalAsync(Guid rentalId);
        Task UpdateRentalAsync(Guid rentalId, CreateBookRentalRequest request);
        Task<List<BookRentalDTO>> GetAllBookRentalsAsync();
        Task<BookRentalDTO> GetBookRentalByIdAsync(Guid rentalId);
        Task<CustomerRentalSummaryDTO> GetCustomerRentalSummaryAsync(string customerEmail);
    }
}
