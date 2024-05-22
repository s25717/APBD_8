using System;
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
                allPages = (int)Math.Ceiling((double)totalRecords / pageSize),
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

        [HttpPost("{idTrip}/clients")]
        public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientAssignmentDto clientDto)
        {
            var trip = await _context.Trips
                .Include(t => t.ClientTrips)
                .FirstOrDefaultAsync(t => t.IdTrip == idTrip);

            if (trip == null || trip.DateFrom <= DateTime.Now)
            {
                return BadRequest(new { message = "Trip does not exist or has already started" });
            }

            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Pesel == clientDto.Pesel);

            if (existingClient != null)
            {
                var clientOnTrip = await _context.ClientTrips
                    .FirstOrDefaultAsync(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);

                if (clientOnTrip != null)
                {
                    return BadRequest(new { message = "Client is already registered for this trip" });
                }
            }
            else
            {
                existingClient = new Client
                {
                    FirstName = clientDto.FirstName,
                    LastName = clientDto.LastName,
                    Email = clientDto.Email,
                    Telephone = clientDto.Telephone,
                    Pesel = clientDto.Pesel
                };

                _context.Clients.Add(existingClient);
                await _context.SaveChangesAsync();
            }

            var newClientTrip = new Client_Trip
            {
                IdClient = existingClient.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = clientDto.PaymentDate
            };

            _context.ClientTrips.Add(newClientTrip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Client successfully assigned to the trip" });
        }
    }
}
