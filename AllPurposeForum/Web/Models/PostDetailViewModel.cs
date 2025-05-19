// Filepath: c:\Users\danik\source\repos\AllPurposeForum\AllPurposeForum\Web\Models\PostDetailViewModel.cs
using AllPurposeForum.Data.DTO;
using System.Collections.Generic;

namespace AllPurposeForum.Web.Models
{
    public class PostDetailViewModel
    {
        public PostDTO Post { get; set; } = new PostDTO();
        public List<PostCommentViewModel> Comments { get; set; } = new List<PostCommentViewModel>();
        public CreateCommentViewModel NewComment { get; set; } = new CreateCommentViewModel();
        public string PostCreatedAtFormatted { get; set; } = string.Empty;
    }
}
