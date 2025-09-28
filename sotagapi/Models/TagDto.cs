namespace sotagapi.Models
{
    public class TagDto
    {
        public string Name { get; set; } = string.Empty;
        public long Count { get; set; }
        public double Share { get; set; }
        public DateTime FetchedAt { get; set; }
    }
}
