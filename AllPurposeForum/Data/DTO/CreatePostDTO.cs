namespace AllPurposeForum.Data.DTO;

public class CreatePostDTO
{
    public required string Title { get; set; }
    public int TopicId { get; set; }
    public required string UserId { get; set; }
    public required string Content { get; set; }
    public required bool Nsfw { get; set; }
}