using Microsoft.EntityFrameworkCore;
using sotagapi.DbContexts;
using sotagapi.Models;
using sotagapi.Services;

namespace sotagapi.Tests.Unit
{
    public class TagServiceTests
    {
        private TagsDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TagsDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new TagsDbContext(options);

            // Clear existing data
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Tags.AddRange(new[]
            {
            new Tag { Id = 1, Name = "csharp", Count = 100, FetchedAt = DateTime.UtcNow.AddHours(-1) },
            new Tag { Id = 2, Name = "java", Count = 50, FetchedAt = DateTime.UtcNow.AddHours(-2) },
            new Tag { Id = 3, Name = "python", Count = 40, FetchedAt = DateTime.UtcNow.AddHours(-3) },
            new Tag { Id = 4, Name = "go", Count = 10, FetchedAt = DateTime.UtcNow.AddHours(-4) }
        });
            context.SaveChanges();
            return context;
        }

        // Tests for Share Calculation Logic

        [Fact]
        public async Task GetTagsAsync_CalculatesShareCorrectly()
        {
            using var context = CreateInMemoryDbContext(nameof(GetTagsAsync_CalculatesShareCorrectly));
            var service = new TagService(context);

            // Act
            var (tags, _) = await service.GetTagsAsync(1, 10, "name", "asc");

            // Assert
            var csharpTag = tags.First(t => t.Name == "csharp");
            var javaTag = tags.First(t => t.Name == "java");
            var goTag = tags.First(t => t.Name == "go");

            // Expected shares: 50.0 (100/200), 25.0 (50/200), 5.0 (10/200)
            Assert.Equal(50.0, csharpTag.Share, 2);
            Assert.Equal(25.0, javaTag.Share, 2);
            Assert.Equal(5.0, goTag.Share, 2);
        }

        [Fact]
        public async Task GetTagsAsync_ReturnsZeroShare_WhenTotalTagCountIsZero()
        {
            using var context = new TagsDbContext(new DbContextOptionsBuilder<TagsDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(GetTagsAsync_ReturnsZeroShare_WhenTotalTagCountIsZero))
                .Options);

            // Add a tag with Count=0, ensuring total count sum is 0
            context.Tags.Add(new Tag { Id = 1, Name = "empty", Count = 0, FetchedAt = DateTime.UtcNow });
            context.SaveChanges();

            var service = new TagService(context);

            // Act
            var (tags, _) = await service.GetTagsAsync(1, 10, "name", "asc");

            // Assert
            Assert.Single(tags);
            Assert.Equal(0.0, tags.First().Share);
        }

        // Tests for Pagination Logic 

        [Theory]
        [InlineData(1, 2, "csharp", "java", 2)] // Page 1, Size 2: should get csharp(100), java(50)
        [InlineData(2, 2, "python", "go", 2)]
        [InlineData(3, 2, null, null, 0)]
        public async Task GetTagsAsync_AppliesPaginationCorrectly(
            int page, int pageSize, string expectedFirstName, string expectedSecondName, int expectedCount)
        {
            using var context = CreateInMemoryDbContext(nameof(GetTagsAsync_AppliesPaginationCorrectly) + page);
            var service = new TagService(context);

            // Sort by Count Descending to ensure predictable order
            var (tags, totalCount) = await service.GetTagsAsync(page, pageSize, "count", "desc");

            // Assert
            Assert.Equal(4, totalCount);
            Assert.Equal(expectedCount, tags.Count);

            if (expectedCount > 0)
            {
                Assert.Equal(expectedFirstName, tags.First().Name);
            }
            if (expectedCount > 1)
            {
                Assert.Equal(expectedSecondName, tags.Skip(1).First().Name);
            }
        }

        // Tests for Sorting Logic 

        [Fact]
        public async Task GetTagsAsync_SortsByCountDescendingCorrectly()
        {
            using var context = CreateInMemoryDbContext(nameof(GetTagsAsync_SortsByCountDescendingCorrectly));
            var service = new TagService(context);

            var (tags, _) = await service.GetTagsAsync(1, 10, "count", "desc");

            // Expected order by Count: 100, 50, 40, 10
            Assert.Equal("csharp", tags[0].Name);
            Assert.Equal("java", tags[1].Name);
            Assert.Equal("python", tags[2].Name);
            Assert.Equal("go", tags[3].Name);
        }

        [Fact]
        public async Task GetTagsAsync_SortsByNameAscendingCorrectly()
        {
            using var context = CreateInMemoryDbContext(nameof(GetTagsAsync_SortsByNameAscendingCorrectly));
            var service = new TagService(context);

            var (tags, _) = await service.GetTagsAsync(1, 10, "name", "asc");

            // Expected order alphabetically: csharp, go, java, python
            Assert.Equal("csharp", tags[0].Name);
            Assert.Equal("go", tags[1].Name);
            Assert.Equal("java", tags[2].Name);
            Assert.Equal("python", tags[3].Name);
        }

        [Fact]
        public async Task GetTagsAsync_SortsByShareAscendingCorrectly()
        {
            using var context = CreateInMemoryDbContext(nameof(GetTagsAsync_SortsByShareAscendingCorrectly));
            var service = new TagService(context);

            // Share order (ascending, based on Count): go(5.0), python(20.0), java(25.0), csharp(50.0)
            var (tags, _) = await service.GetTagsAsync(1, 10, "share", "asc");

            Assert.Equal("go", tags[0].Name);
            Assert.Equal("python", tags[1].Name);
            Assert.Equal("java", tags[2].Name);
            Assert.Equal("csharp", tags[3].Name);
        }

        [Fact]
        public async Task GetTagsAsync_UsesDefaultSortWhenUnknownColumnIsProvided()
        {
            using var context = CreateInMemoryDbContext(nameof(GetTagsAsync_UsesDefaultSortWhenUnknownColumnIsProvided));
            var service = new TagService(context);

            var (tags, _) = await service.GetTagsAsync(1, 10, "invalid_column", "asc");

            Assert.Equal(4, tags.Count);
        }


        // Test for RefreshTagsAsync Logic 

        [Fact]
        public async Task RefreshTagsAsync_OverwritesExistingTagsInDatabase()
        {
            using var context = CreateInMemoryDbContext(nameof(RefreshTagsAsync_OverwritesExistingTagsInDatabase));
            var service = new TagService(context);

            // Arrange
            var newTags = new[]
                {
                    new Tag { Name = "new_tag_a", Count = 10, FetchedAt = DateTime.UtcNow },
                    new Tag { Name = "new_tag_b", Count = 20, FetchedAt = DateTime.UtcNow }
                };

            // Act
            await service.RefreshTagsAsync(newTags);

            // Assert
            var tagsInDb = await context.Tags.ToListAsync();

            // Only the new tags should exist
            Assert.Equal(2, tagsInDb.Count);
            Assert.Contains(tagsInDb, t => t.Name == "new_tag_a");
            Assert.Contains(tagsInDb, t => t.Name == "new_tag_b");
            Assert.DoesNotContain(tagsInDb, t => t.Name == "csharp");
        }
    }
}
