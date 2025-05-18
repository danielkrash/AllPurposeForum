namespace AllPurposeForum.Data.DTO;

public class UpdatePostDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Nsfw { get; set; }
}