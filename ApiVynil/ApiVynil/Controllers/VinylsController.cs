using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVynil.Models;

namespace ApiVynil.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VinylsController : ControllerBase
    {
        private readonly VynilstoreContext _context;

        public VinylsController(VynilstoreContext context)
        {
            _context = context;
        }

        // GET: api/Vinyls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vinyl>>> GetVinyls()
        {
            return await _context.Vinyls.ToListAsync();
        }

        // GET: api/Vinyls/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vinyl>> GetVinyl(int id)
        {
            var vinyl = await _context.Vinyls.FindAsync(id);

            if (vinyl == null)
            {
                return NotFound();
            }

            return vinyl;
        }

        // PUT: api/Vinyls/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVinyl(int id, Vinyl vinyl)
        {
            if (id != vinyl.Id)
            {
                return BadRequest();
            }

            _context.Entry(vinyl).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VinylExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Vinyls
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Vinyl>> PostVinyl(Vinyl vinyl)
        {
            _context.Vinyls.Add(vinyl);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVinyl", new { id = vinyl.Id }, vinyl);
        }

        // DELETE: api/Vinyls/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVinyl(int id)
        {
            var vinyl = await _context.Vinyls.FindAsync(id);
            if (vinyl == null)
            {
                return NotFound();
            }

            _context.Vinyls.Remove(vinyl);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VinylExists(int id)
        {
            return _context.Vinyls.Any(e => e.Id == id);
        }
    }
}
