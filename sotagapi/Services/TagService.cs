using Microsoft.EntityFrameworkCore;
using sotagapi.DbContexts;
using sotagapi.Models;

namespace sotagapi.Services
{
    public interface ITagService
    {
        Task<(List<TagDto> tags, int totalCount)> GetTagsAsync(int page, int pageSize, string sortBy, string sortOrder);
    }

    public class TagService : ITagService
    {
        private readonly TagsDbContext _dbContext;

        public TagService(TagsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(List<TagDto> tags, int totalCount)> GetTagsAsync(int page, int pageSize, string sortBy, string sortOrder)
        {
            var totalCount = await _dbContext.Tags.CountAsync();
            var totalTagCount = await _dbContext.Tags.SumAsync(t => t.Count);

            var query = _dbContext.Tags.Select(t => new TagDto
            {
                Name = t.Name,
                Count = t.Count,
                Share = totalTagCount == 0 ? 0 : ((double)t.Count / totalTagCount) * 100
            });

            query = sortBy.ToLower() switch
            {
                "name" => sortOrder == "asc" ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name),
                "count" => sortOrder == "asc" ? query.OrderBy(t => t.Count) : query.OrderByDescending(t => t.Count),
                "share" => sortOrder == "asc" ? query.OrderBy(t => t.Share) : query.OrderByDescending(t => t.Share),
                _ => query
            };

            var tags = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (tags, totalCount);
        }

        public async Task RefreshTagsAsync(IEnumerable<Tag> newTags)
        {
            _dbContext.Tags.RemoveRange(_dbContext.Tags);
            _dbContext.Tags.AddRange(newTags);
            await _dbContext.SaveChangesAsync();
        }
    }

}
