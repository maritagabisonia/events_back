using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.Management.Service.Services;
using usermangment.Data.Models;

namespace usermangment.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<AplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;


        public AdminController(UserManager<AplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;

            _roleManager=roleManager;

            _context= context;


        }

        [HttpGet("h")]
        public async Task<IActionResult> h()
        {
          
                return Ok($"successfully.");
           
        }
        [HttpGet("artists")]

        public async Task<IActionResult> GetArtists()
        {
            var usersInArtistRole = new List<AplicationUser>();

            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Artist"))
                {
                    usersInArtistRole.Add(user);
                }
            }

            return Ok(usersInArtistRole);
        }
        [AllowAnonymous]
        [HttpGet("event")]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(events);
        }

        // POST: api/events
        [HttpPost("event")]
        public async Task<IActionResult> AddEvent([FromBody] Events newEvent)
        {
            if (newEvent == null)
            {
                return BadRequest();
            }

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvents), new { id = newEvent.Id }, newEvent);
        }

        // DELETE: api/events/{id}
        [AllowAnonymous]

        [HttpDelete("event/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete == null)
            {
                return NotFound();
            }

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        /// <summary>
        [AllowAnonymous]
        [HttpGet("filter-by-date")]
        public async Task<IActionResult> GetEventsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var events = await _context.Events
                .Where(e => e.DateAndTime >= startDate && e.DateAndTime <= endDate)
                .ToListAsync();

            return Ok(events);
        }
        ///
        [HttpGet("GetEvents")]
        public async Task<IActionResult> GetEvents(int pageNumber = 1, int pageSize = 10)
        {
            var events = await _context.Events
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalRecords = await _context.Events.CountAsync();

            var result = new
            {
                Items = events,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };

            return Ok(result);
        }

    }
}
