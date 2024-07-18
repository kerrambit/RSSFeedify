using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSFeedify.Models;
using RSSFeedify.Repository;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.PaginationQuery;
using RSSFeedify.Services;
using RSSFeedify.Services.DataTypeConvertors;

namespace RSSFeedify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RSSFeedsController : ControllerBase
    {
        private readonly IRSSFeedRepository _rSSFeedRepository;
        private readonly IRSSFeedItemRepository _rSSFeedItemRepository;

        public RSSFeedsController(IRSSFeedRepository rSSFeedRepository, IRSSFeedItemRepository rSSFeedItemRepository)
        {
            _rSSFeedRepository = rSSFeedRepository;
            _rSSFeedItemRepository = rSSFeedItemRepository;
        }

        // GET: api/RSSFeeds
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<RSSFeed>>> GetRSSFeeds(int page, int pageSize)
        {

            var result = await _rSSFeedRepository.GetSortedByNameAsync(new PaginationQuery(page, pageSize));
            return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeed>>.Convert(result);
        }

        // GET: api/RSSFeeds/5
        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<RSSFeed>> GetRSSFeed(string guid)
        {
            var result = await _rSSFeedRepository.GetAsync(new Guid(guid));
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // GET: api/RSSFeeds/count
        [HttpGet("count")]
        [Authorize]
        public async Task<ActionResult<int>> GetRSSFeedsCount()
        {
            var result = await _rSSFeedRepository.GetTotalCountAsync();
            return RepositoryResultToActionResultConvertor<int>.Convert(result);
        }

        // PUT: api/RSSFeeds/5
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<ActionResult<RSSFeed>> PutRSSFeed(string guid, RSSFeedDTO rSSFeedDto)
        {
            var result = await _rSSFeedRepository.UpdateAsync(new Guid(guid), rSSFeedDto);
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // POST: api/RSSFeeds
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RSSFeed>> PostRSSFeed(RSSFeedDTO rSSFeedDTO)
        {
            var rSSFeed = RSSFeedDTOToRSSFeed.Convert(rSSFeedDTO);

            var originalRssFeeds = await _rSSFeedRepository.GetAsync();
            if (originalRssFeeds is Success<IEnumerable<RSSFeed>>)
            {
                if (originalRssFeeds.Data.Where(feed => feed.SourceUrl == rSSFeedDTO.SourceUrl).Count() != 0)
                {
                    return ControllersHelper.GetResultForDuplicatedSourcerUrl<RSSFeed>(rSSFeedDTO.SourceUrl);
                }
            }

            var data = RSSFeedPollingService.LoadRSSFeedItemsFromUri(rSSFeed.SourceUrl);
            if (!data.success)
            {
                rSSFeed.LastPoll = DateTime.UtcNow;
            }
            else
            {
                rSSFeed.LastPoll = DateTime.UtcNow;
                rSSFeed.LastSuccessfullPoll = rSSFeed.LastPoll;
            }

            var result = await _rSSFeedRepository.InsertAsync(rSSFeed);

            IList<RSSFeedItem> items = new List<RSSFeedItem>();
            foreach (var item in data.items)
            {
                var rSSFeedItem = RSSFeedItemDTOToRssFeedItem.Convert(item.dto, item.hash);
                rSSFeedItem.RSSFeedId = rSSFeed.Guid;
                items.Add(rSSFeedItem);
            }
            _ = await _rSSFeedItemRepository.InsertMultipleAsync(items);

            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // DELETE: api/RSSFeeds/5
        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult<RSSFeed>> DeleteRSSFeed(string guid)
        {
            var result = await _rSSFeedRepository.DeleteAsync(new Guid(guid));
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        private bool RSSFeedExists(Guid id)
        {
            return _rSSFeedRepository.Exists(id).Data;
        }
    }
}
