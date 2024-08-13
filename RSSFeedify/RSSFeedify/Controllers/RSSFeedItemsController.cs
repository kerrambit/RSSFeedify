using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSFeedify.Controllers.HelperTypes;
using RSSFeedify.Models;
using RSSFeedify.Repository;
using RSSFeedify.Repository.Types.PaginationQuery;
using RSSFeedify.Services.DataTypeConvertors;

using RSSFeed = RSSFeedify.Models.RSSFeed;
using RSSFeedItem = RSSFeedify.Models.RSSFeedItem;

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

        // GET: api/RSSFeedItems?byRSSFeedGuid=5
        [HttpGet("")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RSSFeedItem>>> GetRSSFeedItems([FromQuery] string byRSSFeedGuid, [FromQuery] ControllerPaginationQuery paginationQuery)
        {
            if (ModelState.IsValid)
            {
                if (byRSSFeedGuid is null || byRSSFeedGuid == string.Empty)
                {
                    var unfilteredItemsResult = await _repository.GetAsync(new PaginationQuery(paginationQuery.Page, paginationQuery.PageSize));
                    return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeedItem>>.Convert(unfilteredItemsResult);
                }

                if (!QueryStringParser.ParseGuid(byRSSFeedGuid, out Guid rssFeedGuid))
                {
                    return ControllersHelper.GetResultForInvalidGuid<IEnumerable<RSSFeedItem>>();
                }

                var result = await _repository.GetFilteredByForeignKeyAsync(rssFeedGuid, new PaginationQuery(paginationQuery.Page, paginationQuery.PageSize));
                return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeedItem>>.Convert(result);
            }

            return BadRequest(ModelState);
        }

        // GET: api/RSSFeedItems/count?byRSSFeedGuid=5
        [HttpGet("count")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> GetRSSFeedsCount([FromQuery] string byRSSFeedGuid)
        {
            if (ModelState.IsValid)
            {
                if (!QueryStringParser.ParseGuid(byRSSFeedGuid, out Guid rssFeedGuid))
                {
                    return ControllersHelper.GetResultForInvalidGuid<int>();
                }

                var result = await _repository.GetTotalCountAsync(rssFeedGuid);
                return RepositoryResultToActionResultConvertor<int>.Convert(result);
            }
            return BadRequest(ModelState);
        }

        // GET: api/RSSFeedItems/5
        [HttpGet("{guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RSSFeedItem>> GetRSSFeedItem(string guid)
        {
            if (ModelState.IsValid)
            {
                if (!QueryStringParser.ParseGuid(guid, out Guid rssFeedGuid))
                {
                    return ControllersHelper.GetResultForInvalidGuid<RSSFeedItem>();
                }
                var result = await _repository.GetAsync(rssFeedGuid);
                return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);
            }
            return BadRequest(ModelState);
        }

        //// PUT: api/RSSFeedItems/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{guid}")]
        //[Authorize]
        //public async Task<ActionResult<RSSFeedItem>> PutRSSFeedItem(string guid, RSSFeedItemDTO rSSFeedItemDto)
        //{
        //    var hash = RSSFeedPollingService.GenerateRSSFeedItemHash(rSSFeedItemDto.Title, rSSFeedItemDto.PublishDate);
        //    var result = await _repository.UpdateAsync(new Guid(guid), RSSFeedItemDTOToRssFeedItem.Convert(rSSFeedItemDto, hash));
        //    return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);
        //}

        //// POST: api/RSSFeedItems
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //[Authorize]
        //public async Task<ActionResult<RSSFeedItem>> PostRSSFeedItem(RSSFeedItemDTO rSSFeedItemDto)
        //{
        //    var hash = RSSFeedPollingService.GenerateRSSFeedItemHash(rSSFeedItemDto.Title, rSSFeedItemDto.PublishDate);
        //    var rSSFeed = RSSFeedItemDTOToRssFeedItem.Convert(rSSFeedItemDto, hash);
        //    var result = await _repository.InsertAsync(rSSFeed);
        //    return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);
        //}

        // DELETE: api/RSSFeedItems/5
        [HttpDelete("{guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RSSFeedItem>> DeleteRSSFeedItem(string guid)
        {
            if (ModelState.IsValid)
            {
                if (!QueryStringParser.ParseGuid(guid, out Guid rssFeedGuid))
                {
                    return ControllersHelper.GetResultForInvalidGuid<RSSFeedItem>();
                }

                var result = await _repository.DeleteAsync(rssFeedGuid);
                return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);

            }
            return BadRequest(ModelState);
        }

        private bool RSSFeedItemExists(Guid id)
        {
            return _repository.Exists(id).Data;
        }
    }
}
