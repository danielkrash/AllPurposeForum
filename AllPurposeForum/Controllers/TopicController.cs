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

        [HttpGet("Topics/{topicId:int}")]
        public async Task<IActionResult> Index(int topicId)
        {
            var topic = await _topicService.GetTopicByIdAsync(topicId);
            if (topic == null)
            {
                return NotFound();
            }

            var posts = await _postService.GetPostsByTopicId(topicId);

            var viewModel = new TopicPageViewModel
            {
                TopicId = topic.Id,
                TopicTitle = topic.Title,
                TopicDescription = topic.Description,
                Posts = posts.Select(p => new PostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    UserName = p.UserName,
                    CommentsCount = p.CommentsCount,
                    CreatedAt = p.CreatedAt,
                    CreatedAtFormatted = Utils.TimeAgo(p.CreatedAt)
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
