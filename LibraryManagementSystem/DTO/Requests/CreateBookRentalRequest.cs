using LibraryManagementSystem.Data.Identity;
using LibraryManagmentSystem.Entities;

namespace LibraryManagementSystem.API.DTO.Requests
{
    public class CreateBookRentalRequest
    {
        public Guid BookId { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime RentEndDate { get; set; }
        public int Quantity { get; set; }
    }
}
