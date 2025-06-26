
using AllPurposeForum.Services;
using AllPurposeForum.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using AllPurposeForum.Helpers;
using AllPurposeForum.Data.DTO;
using Microsoft.AspNetCore.Identity;
using AllPurposeForum.Data.Models;
using System.Security.Claims; // Added for User.FindFirstValue
using Microsoft.AspNetCore.Authorization;

namespace AllPurposeForum.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly IPostCommentService _postCommentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITopicService _topicService;
        
        public PostController(IPostService postService, IPostCommentService postCommentService, UserManager<ApplicationUser> userManager, ITopicService topicService)
        {
            _postService = postService;
            _postCommentService = postCommentService;
            _userManager = userManager;
            _topicService = topicService;
        }

        // GET: Topic/{topicId:int}/Post/Create
        [HttpGet("Topic/{topicId:int}/Post/Create", Name = "CreatePost")]
        [Authorize]
        public async Task<IActionResult> Create(int topicId)
        {
            var topic = await _topicService.GetTopicByIdAsync(topicId);
            if (topic == null)
            {
                return NotFound("Topic not found.");
            }

            var model = new CreatePostViewModel
            {
                TopicId = topicId,
                TopicTitle = topic.Title // Pass topic title to the view
            };
            return View(model);
        }

        // POST: Topic/{topicId:int}/Post/Create
        [HttpPost("Topic/{topicId:int}/Post/Create")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int topicId, CreatePostViewModel model)
        {
            var topic = await _topicService.GetTopicByIdAsync(topicId);
            if (topic == null)
            {
                ModelState.AddModelError(string.Empty, "Topic not found.");
                model.TopicTitle = "Error: Topic not found";
                return View(model);
            }
            model.TopicTitle = topic.Title;

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "User not found. Please log in again.");
                    return View(model);
                }

                var createPostDto = new CreatePostDTO
                {
                    TopicId = topicId,
                    UserId = userId,
                    Title = model.Title,
                    Content = model.Content,
                    Nsfw = topic.Nsfw
                };

                try
                {
                    var createdPost = await _postService.CreatePost(createPostDto);
                    return RedirectToRoute("PostDetails", new { postId = createdPost.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the post: " + ex.Message);
                }
            }
            return View(model);
        }

        [HttpGet("Posts/{postId:int}", Name = "PostDetails")]
        public async Task<IActionResult> Index(int postId)
        {
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            var allComments = await _postCommentService.GetPostCommentsByPostIdAsync(postId);
            
            List<PostCommentDTO> commentsToDisplay;
            if (User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                commentsToDisplay = allComments.ToList();
            }
            else
            {
                commentsToDisplay = allComments.Where(c => c.isApproved).ToList();
            }

            var commentViewModels = commentsToDisplay.Select(c => new PostCommentViewModel
            {
                Id = c.Id, 
                UserName = c.UserName,
                Content = c.Content,
                CreatedAtFormatted = Utils.TimeAgo(c.CreatedAt),
                IsApproved = c.isApproved,
                UserId = c.UserId
            }).ToList();

            var viewModel = new PostDetailViewModel
            {
                Post = post,
                Comments = commentViewModels,
                NewComment = new CreateCommentViewModel { PostId = postId },
                PostCreatedAtFormatted = Utils.TimeAgo(post.CreatedAt)
            };

            return View(viewModel);
        }

        [HttpPost("Posts/{postId:int}/Comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, [Bind(Prefix = "NewComment")] CreateCommentViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); 
            }

            if (ModelState.IsValid)
            {
                var createCommentDto = new CreatePostCommentDTO
                {
                    PostId = postId,
                    UserId = user.Id, 
                    Content = model.Content
                };

                try
                {
                    await _postCommentService.CreatePostCommentAsync(createCommentDto);
                    return RedirectToRoute("PostDetails", new { postId = postId });
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while trying to post your comment. Please try again.");
                }
            }
            
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                return NotFound("The post you are trying to comment on was not found.");
            }

            var allCommentsForAddComment = await _postCommentService.GetPostCommentsByPostIdAsync(postId);
            List<PostCommentDTO> commentsToDisplayForAddComment;
            if (User.IsInRole("Manager"))
            {
                commentsToDisplayForAddComment = allCommentsForAddComment.ToList();
            }
            else
            {
                commentsToDisplayForAddComment = allCommentsForAddComment.Where(c => c.isApproved).ToList();
            }

            var commentViewModelsForAddComment = commentsToDisplayForAddComment.Select(c => new PostCommentViewModel
            {
                Id = c.Id, 
                UserName = c.UserName,
                Content = c.Content,
                CreatedAtFormatted = Utils.TimeAgo(c.CreatedAt),
                IsApproved = c.isApproved,
                UserId = c.UserId
            }).ToList();

            var viewModel = new PostDetailViewModel
            {
                Post = post,
                Comments = commentViewModelsForAddComment,
                NewComment = model,
                PostCreatedAtFormatted = Utils.TimeAgo(post.CreatedAt)
            };

            return View("Index", viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId, int topicId)
        {
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Post not found.";
                return topicId > 0 ? RedirectToAction("Index", "Topic", new { topicId = topicId }) : RedirectToAction("Index", "Home");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isOwner = currentUser != null && post.UserId == currentUser.Id;
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = User.IsInRole("Manager");

            if (!isOwner && !isAdmin && !isManager)
            {
                TempData["ErrorMessage"] = "You are not authorized to delete this post.";
                return RedirectToRoute("PostDetails", new { postId = postId });
            }

            try
            {
                var success = await _postService.DeletePost(postId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Post deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete the post.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the post: " + ex.Message;
            }
            
            return RedirectToAction("Index", "Topic", new { topicId = topicId });
        }

        [HttpGet("Posts/{postId:int}/Edit", Name = "EditPostGet")]
        [Authorize]
        public async Task<IActionResult> EditPost(int postId)
        {
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Post not found.";
                return RedirectToAction("Index", "Home");
            }

            var topic = await _topicService.GetTopicByIdAsync(post.TopicId);
            if (topic == null)
            {
                TempData["ErrorMessage"] = "Associated topic not found.";
                return RedirectToAction("Index", "Home");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isOwner = currentUser != null && post.UserId == currentUser.Id;
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = User.IsInRole("Manager");

            if (!isOwner && !isAdmin && !isManager)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit this post.";
                return RedirectToRoute("PostDetails", new { postId = postId });
            }

            var model = new EditPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                TopicId = post.TopicId,
                TopicTitle = topic.Title,
                OriginalPostTitlePreview = post.Title.Length > 50 ? post.Title.Substring(0, 50) + "..." : post.Title
            };

            return View("EditPost", model);
        }

        [HttpPost("Posts/{postId:int}/Edit", Name = "EditPostPost")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int postId, EditPostViewModel model)
        {
            if (postId != model.Id)
            {
                TempData["ErrorMessage"] = "Post ID mismatch.";
                return RedirectToAction("Index", "Home");
            }

            var postToUpdate = await _postService.GetPostById(postId);
            if (postToUpdate == null)
            {
                TempData["ErrorMessage"] = "Post not found.";
                return RedirectToAction("Index", "Home");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool isOwner = currentUser != null && postToUpdate.UserId == currentUser.Id;
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = User.IsInRole("Manager");

            if (!isOwner && !isAdmin && !isManager)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit this post.";
                return RedirectToRoute("PostDetails", new { postId = postId });
            }
            
            var topic = await _topicService.GetTopicByIdAsync(postToUpdate.TopicId);
            model.TopicTitle = topic?.Title ?? "N/A";
            model.OriginalPostTitlePreview = postToUpdate.Title.Length > 50 ? postToUpdate.Title.Substring(0, 50) + "..." : postToUpdate.Title;


            if (ModelState.IsValid)
            {
                var updateDto = new UpdatePostDTO
                {
                    Id = postId,
                    Title = model.Title,
                    Content = model.Content
                    // NSFW status is not editable at the post level in this flow, it's inherited from topic
                };

                try
                {
                    var success = await _postService.UpdatePost(updateDto);
                    if (success != null)
                    {
                        TempData["SuccessMessage"] = "Post updated successfully.";
                        return RedirectToRoute("PostDetails", new { postId = postId });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update the post.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the post: " + ex.Message;
                }
            }
            
            return View("EditPost", model);
        }
    }
}
