namespace OasisApi.Core.Models
{
    public class Sentence
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsPositive { get; set; } 
    }
}
