using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using RSSFeedify.Controllers.HelperTypes;
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
        private readonly ILogger<RSSFeedsController> _logger;

        public RSSFeedsController(IRSSFeedRepository rSSFeedRepository, IRSSFeedItemRepository rSSFeedItemRepository, ILogger<RSSFeedsController> logger)
        {
            _rSSFeedRepository = rSSFeedRepository;
            _rSSFeedItemRepository = rSSFeedItemRepository;
            _logger = logger;
        }

        // GET: api/RSSFeeds?page=1&pageSize=10
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RSSFeed>>> GetRSSFeeds([FromQuery] ControllerPaginationQuery paginationQuery)
        {
            if (ModelState.IsValid)
            {
                var result = await _rSSFeedRepository.GetSortedByNameAsync(new PaginationQuery(paginationQuery.Page, paginationQuery.PageSize));
                return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeed>>.Convert(result);
            }
            return BadRequest(ModelState);
        }

        // GET: api/RSSFeeds/5
        [HttpGet("{guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RSSFeed>> GetRSSFeed([FromRoute] string guid)
        {
            if (ModelState.IsValid)
            {
                if (!QueryStringParser.ParseGuid(guid, out Guid rssFeedGuid))
                {
                    return ControllersHelper.GetResultForInvalidGuid<RSSFeed>();
                }

                var result = await _rSSFeedRepository.GetAsync(rssFeedGuid);
                return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
            }
            return BadRequest(ModelState);
        }

        // GET: api/RSSFeeds/count
        [HttpGet("count")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<int>> GetRSSFeedsCount()
        {
            if (ModelState.IsValid)
            {
                var result = await _rSSFeedRepository.GetTotalCountAsync();
                return RepositoryResultToActionResultConvertor<int>.Convert(result);
            }
            return BadRequest(ModelState);
        }

        // PUT: api/RSSFeeds/5
        [HttpPut("{guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RSSFeed>> PutRSSFeed([FromRoute] string guid, [FromBody] RSSFeedDTO rSSFeedDto)
        {
            if (ModelState.IsValid)
            {
                if (!QueryStringParser.ParseGuid(guid, out Guid rssFeedGuid))
                {
                    return ControllersHelper.GetResultForInvalidGuid<RSSFeed>();
                }

                var result = await _rSSFeedRepository.UpdateAsync(rssFeedGuid, rSSFeedDto);
                return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
            }
            return BadRequest(ModelState);
        }

        // POST: api/RSSFeeds
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RSSFeed>> PostRSSFeed([FromBody] RSSFeedDTO rSSFeedDTO)
        {
            if (ModelState.IsValid)
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

                _logger.LogInformation("RSSFeedItems for new feed will be loaded from '{Url}'.", rSSFeed.SourceUrl);    
                var pollResult = RSSFeedPollingService.LoadRSSFeedItemsFromUri(rSSFeed.SourceUrl);
                if (pollResult.IsError)
                {
                    rSSFeed.LastPoll = DateTime.UtcNow;
                    _logger.LogError("RSSFeedItems for new feed could not be loaded from '{Url}'. Detailed message: '{Message}'.", rSSFeed.SourceUrl, pollResult.GetError);
                }
                else
                {
                    rSSFeed.LastPoll = DateTime.UtcNow;
                    rSSFeed.LastSuccessfullPoll = rSSFeed.LastPoll;
                }

                var result = await _rSSFeedRepository.InsertAsync(rSSFeed);

                IList<RSSFeedItem> items = new List<RSSFeedItem>();
                foreach (var item in pollResult.GetValue.items)
                {
                    var rSSFeedItem = RSSFeedItemDTOToRssFeedItem.Convert(item.Item, item.Hash);
                    rSSFeedItem.RSSFeedId = rSSFeed.Guid;
                    items.Add(rSSFeedItem);
                }
                _ = await _rSSFeedItemRepository.InsertMultipleAsync(items);

                return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
            }
            return BadRequest(ModelState);
        }

        // DELETE: api/RSSFeeds/5
        [HttpDelete("{guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RSSFeed>> DeleteRSSFeed([FromRoute] string guid)
        {
            if (ModelState.IsValid)
            {
                if (!QueryStringParser.ParseGuid(guid, out Guid rssFeedGuid))
                {
                    return ControllersHelper.GetResultForInvalidGuid<RSSFeed>();
                }

                var result = await _rSSFeedRepository.DeleteAsync(rssFeedGuid);
                return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
            }
            return BadRequest(ModelState);
        }

        private bool RSSFeedExists(Guid id)
        {
            return _rSSFeedRepository.Exists(id).Data;
        }
    }
}
