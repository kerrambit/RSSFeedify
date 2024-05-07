using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Services;

namespace RSSFeedify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RSSFeedsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRSSFeedRepository _repository;

        public RSSFeedsController(ApplicationDbContext context, IRSSFeedRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        // GET: api/RSSFeeds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RSSFeed>>> GetRSSFeeds()
        {
            return await _context.RSSFeeds.ToListAsync();
        }

        // GET: api/RSSFeeds/5
        [HttpGet("{guid}")]
        public async Task<ActionResult<RSSFeed>> GetRSSFeed(string guid)
        {
            var rSSFeed = await _context.RSSFeeds.FindAsync(new Guid(guid));

            if (rSSFeed == null)
            {
                return NotFound();
            }

            return rSSFeed;
        }

        // PUT: api/RSSFeeds/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{guid}")]
        public async Task<IActionResult> PutRSSFeed(string guid, RSSFeed rSSFeed)
        {
            if (guid != rSSFeed.Guid.ToString())
            {
                return BadRequest();
            }

            _context.Entry(rSSFeed).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RSSFeedExists(new Guid(guid)))
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

        // POST: api/RSSFeeds
        [HttpPost]
        public async Task<ActionResult<RSSFeed>> PostRSSFeed(RSSFeedDTO rSSFeedDTO)
        {
            var rSSFeed = new RSSFeed
            {
                Name = rSSFeedDTO.Name,
                Description = rSSFeedDTO.Description,
                SourceUrl = rSSFeedDTO.SourceUrl,
                PollingInterval = rSSFeedDTO.PollingInterval
            };

            var result = _repository.InsertRSSFeed(rSSFeed);
            await _repository.SaveAsync();
            return RepositoryResultToActionResultConvertor.Convert(result);
        }

        // DELETE: api/RSSFeeds/5
        [HttpDelete("{guid}")]
        public async Task<ActionResult<RSSFeed>> DeleteRSSFeed(string guid)
        {            
            var result = await _repository.DeleteRSSFeedAsync(new Guid(guid));
            await _repository.SaveAsync();
            return RepositoryResultToActionResultConvertor.Convert(result);
        }

        private bool RSSFeedExists(Guid id)
        {
            return _context.RSSFeeds.Any(e => e.Guid == id);
        }
    }
}
