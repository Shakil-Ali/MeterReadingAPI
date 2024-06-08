using CsvHelper.Configuration;
using CsvHelper;
using MeterReadingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace MeterReadingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeterReadingUploadsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MeterReadingUploadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var meterReadings = csv.GetRecords<MeterReadingCsvModel>().ToList();
                var results = new List<MeterReadingResult>();

                foreach (var reading in meterReadings)
                {
                    var account = await _context.Accounts.FindAsync(reading.AccountId);
                    if (account == null)
                    {
                        results.Add(new MeterReadingResult { Reading = reading, Success = false, Error = "Invalid Account ID" });
                        continue;
                    }

                    var existingReading = await _context.MeterReadings
                        .Where(m => m.AccountId == reading.AccountId)
                        .OrderByDescending(m => m.ReadingDate)
                        .FirstOrDefaultAsync();

                    if (existingReading != null && reading.ReadingDate <= existingReading.ReadingDate)
                    {
                        results.Add(new MeterReadingResult { Reading = reading, Success = false, Error = "Reading date is older than existing reading" });
                        continue;
                    }

                    if (_context.MeterReadings.Any(m => m.AccountId == reading.AccountId && m.ReadingDate == reading.ReadingDate))
                    {
                        results.Add(new MeterReadingResult { Reading = reading, Success = false, Error = "Duplicate reading" });
                        continue;
                    }

                    _context.MeterReadings.Add(new MeterReading
                    {
                        AccountId = reading.AccountId,
                        ReadingDate = reading.ReadingDate,
                        ReadingValue = reading.ReadingValue
                    });
                    await _context.SaveChangesAsync();

                    results.Add(new MeterReadingResult { Reading = reading, Success = true });
                }

                return Ok(new
                {
                    SuccessCount = results.Count(r => r.Success),
                    FailureCount = results.Count(r => !r.Success),
                    Results = results
                });
            }
        }
    }

    public class MeterReadingCsvModel
    {
        public int AccountId { get; set; }
        public DateTime ReadingDate { get; set; }
        public string ReadingValue { get; set; }
    }

    public class MeterReadingResult
    {
        public MeterReadingCsvModel Reading { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

}
