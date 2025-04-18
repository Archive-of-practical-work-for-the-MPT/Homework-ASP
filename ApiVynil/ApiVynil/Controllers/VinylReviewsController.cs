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
    public class VinylReviewsController : ControllerBase
    {
        private readonly VynilstoreContext _context;

        public VinylReviewsController(VynilstoreContext context)
        {
            _context = context;
        }

        // GET: api/VinylReviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VinylReview>>> GetVinylReviews()
        {
            return await _context.VinylReviews.ToListAsync();
        }

        // GET: api/VinylReviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VinylReview>> GetVinylReview(int id)
        {
            var vinylReview = await _context.VinylReviews.FindAsync(id);

            if (vinylReview == null)
            {
                return NotFound();
            }

            return vinylReview;
        }

        // PUT: api/VinylReviews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVinylReview(int id, VinylReview vinylReview)
        {
            if (id != vinylReview.Id)
            {
                return BadRequest();
            }

            _context.Entry(vinylReview).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VinylReviewExists(id))
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

        // POST: api/VinylReviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<VinylReview>> PostVinylReview(VinylReview vinylReview)
        {
            _context.VinylReviews.Add(vinylReview);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVinylReview", new { id = vinylReview.Id }, vinylReview);
        }

        // DELETE: api/VinylReviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVinylReview(int id)
        {
            var vinylReview = await _context.VinylReviews.FindAsync(id);
            if (vinylReview == null)
            {
                return NotFound();
            }

            _context.VinylReviews.Remove(vinylReview);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VinylReviewExists(int id)
        {
            return _context.VinylReviews.Any(e => e.Id == id);
        }
    }
}
