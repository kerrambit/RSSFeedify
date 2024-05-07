using Microsoft.AspNetCore.Mvc;
using RSSFeedify.Models;
using RSSFeedify.Repository;
using RSSFeedify.Services;

namespace RSSFeedify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RSSFeedItemsController : ControllerBase
    {
        private readonly IRSSFeedItemRepository _repository;

        public RSSFeedItemsController(IRSSFeedItemRepository repository)
        {
            _repository = repository;
        }

        // GET: api/RSSFeedItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RSSFeedItem>>> GetRSSFeedsItems()
        {
            var result = await _repository.GetAsync();
            return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeedItem>>.Convert(result);
        }

        // GET: api/RSSFeedItems/5
        [HttpGet("{guid}")]
        public async Task<ActionResult<IEnumerable<RSSFeedItem>>> GetRSSFeedItemsFiltered(string guid)
        {
            var result = await _repository.GetAsyncFilteredByForeignKey(new Guid(guid));
            return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeedItem>>.Convert(result);
        }

        // PUT: api/RSSFeedItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{guid}")]
        public async Task<ActionResult<RSSFeedItem>> PutRSSFeedItem(string guid, RSSFeedItemDTO rSSFeedItemDto)
        {
            var result = await _repository.UpdateAsync(new Guid(guid), RSSFeedItemDTOToRssFeedItem.Convert(rSSFeedItemDto));
            return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);
        }

        // POST: api/RSSFeedItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RSSFeedItem>> PostRSSFeedItem(RSSFeedItemDTO rSSFeedItemDto)
        {
            var rSSFeed = RSSFeedItemDTOToRssFeedItem.Convert(rSSFeedItemDto);
            var result = _repository.Insert(rSSFeed);
            await _repository.SaveAsync();
            return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);
        }

        // DELETE: api/RSSFeedItems/5
        [HttpDelete("{guid}")]
        public async Task<ActionResult<RSSFeedItem>> DeleteRSSFeedItem(string guid)
        {
            var result = await _repository.DeleteAsync(new Guid(guid));
            await _repository.SaveAsync();
            return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);
        }

        private bool RSSFeedItemExists(Guid id)
        {
            return _repository.Exists(id).Data;
        }
    }
}
