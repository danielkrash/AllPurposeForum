namespace AllPurposeForum.Data.DTO;

public class TopicDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool Nsfw { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public int PostsCount { get; set; }
}