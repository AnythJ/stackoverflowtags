using Microsoft.AspNetCore.Mvc;
using sotagapi.DbContexts;
using sotagapi.Models;
using sotagapi.Services;
using System.ComponentModel.DataAnnotations;

namespace sotagapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly TagService _service;
        private readonly StackOverflowClient _client;
        private readonly TagsDbContext _db;

        public TagsController(TagService service, StackOverflowClient client, TagsDbContext db)
        {
            _service = service;
            _client = client;
            _db = db;
        }

        /// <summary>
        /// Pobiera paginowaną listę tagów StackOverflow.
        /// </summary>
        /// <param name="page">Numer strony (domyślnie 1).</param>
        /// <param name="pageSize">Elementów na stronie (maks. 100).</param>
        /// <response code="200">Zwraca paginowaną listę tagów.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<TagDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTags(
           [Range(1, int.MaxValue)] int page = 1, 
           [Range(1, 100)] int pageSize = 20, 
           TagSortBy sortBy = TagSortBy.Share, 
           SortOrder sortOrder = SortOrder.Desc)
        {
            var (tags, totalCount) = await _service.GetTagsAsync(
                page, pageSize, sortBy.ToString(), sortOrder.ToString());

            var items = tags.Select(t => new TagDto
            {
                Name = t.Name,
                Count = t.Count,
                Share = t.Share,
                FetchedAt = t.FetchedAt
            }).ToList();

            return Ok(new PagedResponse<TagDto>
            {
                TotalCount = totalCount,
                Items = items
            });
        }

        /// <summary>
        /// Aktualizuje listę 1000 tagów ze StackOverflow.
        /// </summary>
        /// <response code="200">Zwraca paginowaną listę tagów.</response>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var tags = await _client.FetchTagsAsync(1000);
            await _service.RefreshTagsAsync(tags);
            return Accepted(new { message = $"Refresh initiated for {tags.Count} tags." });
        }
    }
}
