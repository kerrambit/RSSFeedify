using Microsoft.AspNetCore.Mvc;
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
        private readonly IRSSFeedRepository _repository;

        public RSSFeedsController(IRSSFeedRepository repository)
        {
            _repository = repository;
        }

        // GET: api/RSSFeeds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RSSFeed>>> GetRSSFeeds()
        {
            var result = await _repository.GetRSSFeeds();
            return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeed>>.Convert(result);
        }

        // GET: api/RSSFeeds/5
        [HttpGet("{guid}")]
        public async Task<ActionResult<RSSFeed>> GetRSSFeed(string guid)
        {
            var result = await _repository.GetRSSFeedByGUID(new Guid(guid));
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // PUT: api/RSSFeeds/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{guid}")]
        public async Task<ActionResult<RSSFeed>> PutRSSFeed(string guid, RSSFeedDTO rSSFeedDto)
        {
            var result = await _repository.UpdateRSSFeed(new Guid(guid), RSSFeedDTOToRSSFeed.Convert(rSSFeedDto));
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // POST: api/RSSFeeds
        [HttpPost]
        public async Task<ActionResult<RSSFeed>> PostRSSFeed(RSSFeedDTO rSSFeedDTO)
        {
            var rSSFeed = RSSFeedDTOToRSSFeed.Convert(rSSFeedDTO);
            var result = _repository.InsertRSSFeed(rSSFeed);
            await _repository.SaveAsync();
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // DELETE: api/RSSFeeds/5
        [HttpDelete("{guid}")]
        public async Task<ActionResult<RSSFeed>> DeleteRSSFeed(string guid)
        {            
            var result = await _repository.DeleteRSSFeedAsync(new Guid(guid));
            await _repository.SaveAsync();
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        private bool RSSFeedExists(Guid id)
        {
            return _repository.RSSFeedExists(id).Data;
        }
    }
}
