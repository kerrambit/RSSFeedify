using Microsoft.AspNetCore.Mvc;
using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Services.DataTypeConvertors;
using RSSFeedify.Services;
using RSSFeedify.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using PostgreSQL.Data;
using System.Security.Policy;

namespace RSSFeedify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RSSFeedsController : ControllerBase
    {
        private readonly IRSSFeedRepository _rSSFeedRepository;
        private readonly IRSSFeedItemRepository _rSSFeedItemRepository;
        private readonly ApplicationDbContext _context;

        public RSSFeedsController(ApplicationDbContext context, IRSSFeedRepository rSSFeedRepository, IRSSFeedItemRepository rSSFeedItemRepository)
        {
            _context = context;
            _rSSFeedRepository = rSSFeedRepository;
            _rSSFeedItemRepository = rSSFeedItemRepository;
        }

        // GET: api/RSSFeeds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RSSFeed>>> GetRSSFeeds()
        {
            var result = await _rSSFeedRepository.GetAsync();
            return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeed>>.Convert(result);
        }

        // GET: api/RSSFeeds/5
        [HttpGet("{guid}")]
        public async Task<ActionResult<RSSFeed>> GetRSSFeed(string guid)
        {
            var result = await _rSSFeedRepository.GetAsync(new Guid(guid));
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // PUT: api/RSSFeeds/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{guid}")]
        public async Task<ActionResult<RSSFeed>> PutRSSFeed(string guid, RSSFeedDTO rSSFeedDto)
        {
            var result = await _rSSFeedRepository.UpdateAsync(new Guid(guid), RSSFeedDTOToRSSFeed.Convert(rSSFeedDto));
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // POST: api/RSSFeeds
        [HttpPost]
        public async Task<ActionResult<RSSFeed>> PostRSSFeed(RSSFeedDTO rSSFeedDTO)
        {
            var rSSFeed = RSSFeedDTOToRSSFeed.Convert(rSSFeedDTO);

            var data = RSSFeedPollingService.LoadRSSFeedItemsFromUri(rSSFeed.SourceUrl);
            //var data = RSSFeedPollingService.LoadRSSFeedItemsFromUri(new Uri("https://feedsfwergrwegergerger"));
            if (!data.success)
            {
                rSSFeed.LastPoll = DateTime.UtcNow;
            } else
            {
                rSSFeed.LastPoll = DateTime.UtcNow;
                rSSFeed.LastSuccessfullPoll = rSSFeed.LastPoll;
            }

            var result = _rSSFeedRepository.Insert(rSSFeed);
            await _rSSFeedRepository.SaveAsync();

            Console.WriteLine("\n----------------- POST of RSSFeed -----------------");
            Console.WriteLine($"Last updated time of the feed: {data.lastUpdate}.\nIndividual feed items: ");
            foreach (var pair in data.items)
            {
                Console.WriteLine($"Hash: {pair.hash}");
                var item = pair.dto;
                Console.WriteLine($"Title: {item.Title}");
                Console.WriteLine($"Summary: {item.Summary}");
                Console.WriteLine($"Publish Date: {item.PublishDate}");
                Console.WriteLine($"Content: {item.Content}");
                Console.WriteLine($"Id: {item.Id}");

                Console.WriteLine("\nAuthors:");
                foreach (var author in item.Authors)
                {
                    Console.WriteLine($" - {author}");
                }

                Console.WriteLine("\nContributors:");
                foreach (var contributor in item.Contributors)
                {
                    Console.WriteLine($" - {contributor}");
                }

                Console.WriteLine("\nLinks:");
                foreach (var link in item.Links)
                {
                    Console.WriteLine($" - {link}");
                }

                Console.WriteLine("\nCategories:");
                foreach (var category in item.Categories)
                {
                    Console.WriteLine($"  - {category}");
                }

                Console.WriteLine("-----------------------------------------");
            }

            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                foreach (var item in data.items)
                {
                    var rSSFeedItem = RSSFeedItemDTOToRssFeedItem.Convert(item.dto, item.hash);
                    rSSFeedItem.RSSFeedId = rSSFeed.Guid;
                    _ = _rSSFeedItemRepository.Insert(rSSFeedItem);
                }

                await _rSSFeedItemRepository.SaveAsync();
                dbContextTransaction.Commit();
            }

            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // DELETE: api/RSSFeeds/5
        [HttpDelete("{guid}")]
        public async Task<ActionResult<RSSFeed>> DeleteRSSFeed(string guid)
        {            
            var result = await _rSSFeedRepository.DeleteAsync(new Guid(guid));
            await _rSSFeedRepository.SaveAsync();
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        private bool RSSFeedExists(Guid id)
        {
            return _rSSFeedRepository.Exists(id).Data;
        }
    }
}
