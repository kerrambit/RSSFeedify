using Microsoft.AspNetCore.Mvc;
using PostgreSQL.Data;
using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Services;
using System.Xml;
using System.ServiceModel.Syndication;

namespace RSSFeedify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RSSFeedsController : ControllerBase
    {
        private readonly IRepository<RSSFeed> _repository;

        public RSSFeedsController(IRepository<RSSFeed> repository)
        {
            _repository = repository;
        }

        // GET: api/RSSFeeds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RSSFeed>>> GetRSSFeeds()
        {
            var result = await _repository.GetAsync();
            return RepositoryResultToActionResultConvertor<IEnumerable<RSSFeed>>.Convert(result);
        }

        // GET: api/RSSFeeds/5
        [HttpGet("{guid}")]
        public async Task<ActionResult<RSSFeed>> GetRSSFeed(string guid)
        {
            var result = await _repository.GetAsync(new Guid(guid));
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // PUT: api/RSSFeeds/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{guid}")]
        public async Task<ActionResult<RSSFeed>> PutRSSFeed(string guid, RSSFeedDTO rSSFeedDto)
        {
            var result = await _repository.UpdateAsync(new Guid(guid), RSSFeedDTOToRSSFeed.Convert(rSSFeedDto));
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // POST: api/RSSFeeds
        [HttpPost]
        public async Task<ActionResult<RSSFeed>> PostRSSFeed(RSSFeedDTO rSSFeedDTO)
        {
            var rSSFeed = RSSFeedDTOToRSSFeed.Convert(rSSFeedDTO);
            var result = _repository.Insert(rSSFeed);
            await _repository.SaveAsync();

            /// TMP ----------------------------------------------------------------------------------------------------
            string url = "https://feeds.bbci.co.uk/news/rss.xml?edition=uk";
            XmlReader reader = XmlReader.Create(url);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();

            DateTimeOffset feetLastUpdatedTime = feed.LastUpdatedTime;
            Console.WriteLine($"\n\nLast updated time of the feed: {feetLastUpdatedTime}.\n Individual feed items: ");

            foreach (SyndicationItem item in feed.Items)
            {
                String subject = item.Title.Text;
                String summary = item.Summary.Text;
                System.Collections.ObjectModel.Collection<SyndicationLink> links = item.Links;
                System.Collections.ObjectModel.Collection<SyndicationCategory> categories = item.Categories;
                DateTimeOffset date = item.PublishDate;

                // Authors
                string authors = string.Join(", ", item.Authors.Select(a => a.Name));

                // Contributors
                string contributors = string.Join(", ", item.Contributors.Select(c => c.Name));

                // Content
                string? content = item.Content?.ToString();

                // Id
                string id = item.Id;

                // BaseUri
                Uri baseUri = item.BaseUri;

                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Summary: {summary}");
                Console.WriteLine($"Authors: {authors}");
                Console.WriteLine($"Contributors: {contributors}");
                Console.WriteLine($"Content: {content}");
                Console.WriteLine($"Id: {id}");
                Console.WriteLine($"BaseUri: {baseUri}");

                Console.WriteLine("\nLinks:");
                foreach (var link in links)
                {
                    Console.WriteLine($"  - {link.Uri}");
                }

                Console.WriteLine("\nCategories:");
                foreach (var category in categories)
                {
                    Console.WriteLine($"  - {category.Name}");
                }

                Console.WriteLine($"Publish Date: {date}");
                Console.WriteLine("-----------------------------------------");
            }

            /// TMP ----------------------------------------------------------------------------------------------------

            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        // DELETE: api/RSSFeeds/5
        [HttpDelete("{guid}")]
        public async Task<ActionResult<RSSFeed>> DeleteRSSFeed(string guid)
        {            
            var result = await _repository.DeleteAsync(new Guid(guid));
            await _repository.SaveAsync();
            return RepositoryResultToActionResultConvertor<RSSFeed>.Convert(result);
        }

        private bool RSSFeedExists(Guid id)
        {
            return _repository.Exists(id).Data;
        }
    }
}
