namespace AllPurposeForum.Data.DTO;

public class PostDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public bool Nsfw { get; set; }
    public string UserId { get; set; }
    public int TopicId { get; set; }
}