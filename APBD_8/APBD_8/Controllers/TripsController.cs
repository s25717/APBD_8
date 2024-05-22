using System.Linq;
using System.Threading.Tasks;
using APBD_8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TravelApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public TripsController(TravelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10)
        {
            var totalRecords = await _context.Trips.CountAsync();
            var trips = await _context.Trips
                .OrderByDescending(t => t.DateFrom)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.CountryTrips)
                    .ThenInclude(ct => ct.Country)
                .Include(t => t.ClientTrips)
                    .ThenInclude(ct => ct.Client)
                .ToListAsync();

            var response = new
            {
                pageNum,
                pageSize,
                allPages = (int)System.Math.Ceiling((double)totalRecords / pageSize),
                trips = trips.Select(t => new
                {
                    t.Name,
                    t.Description,
                    t.DateFrom,
                    t.DateTo,
                    t.MaxPeople,
                    Countries = t.CountryTrips.Select(ct => new { ct.Country.Name }),
                    Clients = t.ClientTrips.Select(ct => new { ct.Client.FirstName, ct.Client.LastName })
                })
            };

            return Ok(response);
        }
    }
}
