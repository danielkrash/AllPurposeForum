using AllPurposeForum.Services;
using AllPurposeForum.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using AllPurposeForum.Helpers; // Add this line

namespace AllPurposeForum.Controllers
{
    public class TopicController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;

        public TopicController(ITopicService topicService, IPostService postService)
        {
            _topicService = topicService;
            _postService = postService;
        }

        [HttpGet("Topics/{topicId:int}", Name = "TopicDetails")]
        public async Task<IActionResult> Index(int topicId)
        {
            var topic = await _topicService.GetTopicByIdAsync(topicId);
            if (topic == null)
            {
                return NotFound();
            }

            var postsFromService = await _postService.GetPostsByTopicId(topicId);

            var postViewModels = postsFromService.Select(p => new PostViewModel
            {
                Id = p.Id,
                Title = p.Title,
                AuthorName = p.UserName,
                CreatedAtFormatted = Utils.TimeAgo(p.CreatedAt),
                CommentsCount = p.CommentsCount,
                TopicId = p.TopicId,
                ContentPreview = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content // Simple preview
            }).ToList();

            var viewModel = new TopicDetailViewModel
            {
                Topic = topic,
                Posts = postViewModels
            };

            return View(viewModel);
        }
    }
}
