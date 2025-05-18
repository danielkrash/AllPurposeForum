namespace AllPurposeForum.Data.DTO;

public class UpdateTopicDTO
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool Nsfw { get; set; }
}