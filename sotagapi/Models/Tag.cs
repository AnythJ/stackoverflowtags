namespace sotagapi.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Count { get; set; }
        public DateTime FetchedAt { get; set; }
    }

}
