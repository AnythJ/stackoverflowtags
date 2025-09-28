using Xunit;
using System.Net;
using System.Net.Http.Json;
using sotagapi.Models;

namespace sotagapi.Tests.Integration
{
    public class TagsIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TagsIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetTags_ReturnsOk_WithCorrectPaginationAndTotalCount()
        {
            // Arrange
            const int pageSize = 1;
            const int page = 2;
            var url = $"/api/tags?page={page}&pageSize={pageSize}&sortBy=name&sortOrder=asc";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<TagDto>>();

            Assert.NotNull(pagedResponse);
            Assert.Equal(3, pagedResponse.TotalCount); 
            Assert.Single(pagedResponse.Items);
            Assert.Equal("java", pagedResponse.Items.First().Name);
        }

        [Fact]
        public async Task GetTags_ReturnsBadRequest_ForInvalidPageSize()
        {
            // Arrange
            var url = "/api/tags?page=1&pageSize=101";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("The field pageSize must be between 1 and 100", content);
        }
    }
}
