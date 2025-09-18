using LibraryManagementSystem.API.Services;
using LibraryManagementSystem.API.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IBookRentalService _rentalService;
        private readonly IReturnedRentalService _returnedRentalService;
        public ReportsController(IBookRentalService rentalService, IReturnedRentalService returnedRentalService)
        {
            _rentalService = rentalService;
            _returnedRentalService = returnedRentalService;
        }

        [HttpGet("export-rentals")]
        public async Task<IActionResult> ExportRentals([FromServices] ExcelExportService excelExportService)
        {
            var rentals = await _rentalService.GetAllBookRentalsWithIncludes();
            var returnedRentals = await _returnedRentalService.GetAllBookReturnedRentalsWithIncludes();
            var fileContent = excelExportService.ExportBookRentalsToExcel(rentals.ToList(), returnedRentals.ToList());
            var fileName = $"BookRentals_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
