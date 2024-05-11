﻿using Microsoft.AspNetCore.Mvc;
using RSSFeedify.Models;
using RSSFeedify.Repository;
using RSSFeedify.Services;
using RSSFeedify.Services.DataTypeConvertors;

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
        public async Task<ActionResult<IEnumerable<RSSFeedItem>>> GetRSSFeedItems([FromQuery] string? byRSSFeedGuid)
        {
            if (byRSSFeedGuid is null || byRSSFeedGuid == string.Empty)
            {
                var unfilteredItemsResult = await _repository.GetAsync();
                return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeedItem>>.Convert(unfilteredItemsResult);
            }

            if (!QueryStringParser.ParseGuid(byRSSFeedGuid, out Guid rssFeedGuid))
            {
                return ControllersHelper.GetResultForInvalidGuid<IEnumerable<RSSFeedItem>>();
            }

            var result = await _repository.GetFilteredByForeignKeyAsync(rssFeedGuid);
            return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeedItem>>.Convert(result);
        }

        // GET: api/RSSFeedItems/5
        [HttpGet("{guid}")]
        public async Task<ActionResult<RSSFeedItem>> GetRSSFeedItem(string guid)
        {
            if (!QueryStringParser.ParseGuid(guid, out Guid rssFeedGuid))
            {
                return ControllersHelper.GetResultForInvalidGuid<RSSFeedItem>();
            }
            var result = await _repository.GetAsync(rssFeedGuid);
            return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);
        }

        // PUT: api/RSSFeedItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{guid}")]
        public async Task<ActionResult<RSSFeedItem>> PutRSSFeedItem(string guid, RSSFeedItemDTO rSSFeedItemDto)
        {
            var hash = RSSFeedPollingService.GenerateRSSFeedItemHash(rSSFeedItemDto.Title, rSSFeedItemDto.PublishDate);
            var result = await _repository.UpdateAsync(new Guid(guid), RSSFeedItemDTOToRssFeedItem.Convert(rSSFeedItemDto, hash));
            return RepositoryResultToActionResultConvertor<RSSFeedItem>.Convert(result);
        }

        // POST: api/RSSFeedItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RSSFeedItem>> PostRSSFeedItem(RSSFeedItemDTO rSSFeedItemDto)
        {
            var hash = RSSFeedPollingService.GenerateRSSFeedItemHash(rSSFeedItemDto.Title, rSSFeedItemDto.PublishDate);
            var rSSFeed = RSSFeedItemDTOToRssFeedItem.Convert(rSSFeedItemDto, hash);
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
