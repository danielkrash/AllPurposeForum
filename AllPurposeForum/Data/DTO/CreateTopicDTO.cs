namespace AllPurposeForum.Data.DTO;

public class CreateTopicDTO
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string UserId { get; set; }
    public bool Nsfw { get; set; }
}