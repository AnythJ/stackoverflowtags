using sotagapi.Models;
using System.Net;

namespace sotagapi.Services
{
    public class StackOverflowClient
    {
        private readonly HttpClient _http;

        public StackOverflowClient(HttpClient http)
        {
            _http = http;
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("sotagapi/1.0");
        }

        public async Task<List<Tag>> FetchTagsAsync(int minCount = 1000)
        {
            var tags = new List<Tag>();
            int page = 1;

            while (tags.Count < minCount)
            {
                if (page > 1)
                {
                    await Task.Delay(1000);
                }

                var url = $"https://api.stackexchange.com/2.3/tags?order=desc&sort=popular&site=stackoverflow&page={page}&pagesize=100";

                var httpResponse = await _http.GetAsync(url);

                if (httpResponse.StatusCode == (HttpStatusCode)429)
                {
                    break;
                }

                httpResponse.EnsureSuccessStatusCode(); 

                var response = await httpResponse.Content.ReadFromJsonAsync<StackOverflowTagsResponse>();

                if (response == null || response.Items == null || response.Items.Count == 0) break;

                tags.AddRange(response.Items.Select(i => new Tag
                {
                    Name = i.Name,
                    Count = i.Count,
                    FetchedAt = DateTime.UtcNow
                }));

                page++;
            }

            return tags;
        }

        private class StackOverflowTagsResponse
        {
            public List<Item> Items { get; set; } = new();
            public class Item
            {
                public string Name { get; set; } = string.Empty;
                public long Count { get; set; }
            }
        }
    }


}
