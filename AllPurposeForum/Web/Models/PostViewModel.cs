namespace AllPurposeForum.Web.Models
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? AuthorName { get; set; }
        public string? CreatedAtFormatted { get; set; }
        public int CommentsCount { get; set; }
        public int TopicId { get; set; }
        public string? ContentPreview { get; set; } // For a short preview on the topic page
        public string? UserId { get; set; } // Added UserId for ownership checks
    }
}
