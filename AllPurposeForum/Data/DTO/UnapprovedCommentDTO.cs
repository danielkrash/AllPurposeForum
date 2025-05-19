namespace AllPurposeForum.Data.DTO
{
    public class UnapprovedCommentDTO
    {
        public int CommentId { get; set; }
        public string Text { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string AuthorUserName { get; set; }
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public int TopicId { get; set; }
        public string TopicTitle { get; set; }
        public string OriginalPostContentPreview { get; set; }
    }
}
