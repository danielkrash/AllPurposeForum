// Filepath: c:\Users\danik\source\repos\AllPurposeForum\AllPurposeForum\Web\Models\TopicDetailViewModel.cs
using AllPurposeForum.Data.DTO;
using System.Collections.Generic;

namespace AllPurposeForum.Web.Models
{
    public class TopicDetailViewModel
    {
        public TopicDTO Topic { get; set; }
        public List<PostViewModel> Posts { get; set; }
    }
}
