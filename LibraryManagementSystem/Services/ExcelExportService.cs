using ClosedXML.Excel;
using LibraryManagementSystem.Data.Entities;

namespace LibraryManagementSystem.API.Services
{
    public class ExcelExportService
    {
        public byte[] ExportBookRentalsToExcel(List<BookRentals> bookRentals)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Book Rentals");

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Book Title";
                worksheet.Cell(1, 3).Value = "Customer Name";
                worksheet.Cell(1, 4).Value = "Rent Start Date";
                worksheet.Cell(1, 5).Value = "Rent End Date";
                worksheet.Cell(1, 6).Value = "Quantity";
                worksheet.Cell(1, 7).Value = "Price";

                for (int i = 0; i < bookRentals.Count; i++)
                {
                    var rental = bookRentals[i];
                    worksheet.Cell(i + 2, 1).Value = rental.Id.ToString();
                    worksheet.Cell(i + 2, 2).Value = rental.Book?.Title ?? "N/A";
                    worksheet.Cell(i + 2, 3).Value = rental.Customer?.FirstName ?? "N/A";
                    worksheet.Cell(i + 2, 4).Value = rental.RentStartDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 5).Value = rental.RentEndDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 6).Value = rental.Quantity;
                    worksheet.Cell(i + 2, 7).Value = rental.Price;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }

        }
    }
}
