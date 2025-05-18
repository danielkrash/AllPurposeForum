using System.Collections.Generic;

namespace AllPurposeForum.Web.Models;

public class TopicPageViewModel
{
    public int TopicId { get; set; }
    public string TopicTitle { get; set; }
    public string TopicDescription { get; set; }
    public List<PostViewModel> Posts { get; set; }
}
