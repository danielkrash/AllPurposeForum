using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;
using AllPurposeForum.Services;
using AllPurposeForum.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using AllPurposeForum.Helpers; // Add this line

namespace AllPurposeForum.Web.Controllers // Ensured namespace is correct
{
    public class TopicController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;
        private readonly UserManager<ApplicationUser> _userManager; // Added UserManager

        // Updated constructor to include UserManager
        public TopicController(ITopicService topicService, IPostService postService, UserManager<ApplicationUser> userManager)
        {
            _topicService = topicService;
            _postService = postService;
            _userManager = userManager; // Initialize UserManager
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

        // GET: Topic/Create
        [Authorize]
        public IActionResult Create()
        {
            return View(new CreateTopicViewModel()); // Pass a new view model
        }

        // POST: Topic/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTopicViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User); // More robust way to get UserId
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "User not found. Please log in again.");
                    return View(model);
                }

                var createTopicDto = new CreateTopicDTO
                {
                    Title = model.Title,
                    Description = model.Description,
                    UserId = userId,
                    Nsfw = model.isNswf // Map the Nsfw property
                };

                try
                {
                    var createdTopic = await _topicService.CreateTopicAsync(createTopicDto);
                    // Redirect to the newly created topic's detail page
                    return RedirectToRoute("TopicDetails", new { topicId = createdTopic.Id }); 
                }
                catch (Exception ex)
                {
                    // Log the error (using ILogger or your preferred logging mechanism)
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the topic. " + ex.Message);
                }
            }
            return View(model);
        }
    }
}
