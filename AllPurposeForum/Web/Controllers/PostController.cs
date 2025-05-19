// Filepath: c:\Users\danik\source\repos\AllPurposeForum\AllPurposeForum\Controllers\PostController.cs
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
using Microsoft.AspNetCore.Authorization; // Added for [Authorize]

namespace AllPurposeForum.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly IPostCommentService _postCommentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITopicService _topicService; // Added ITopicService

        // Updated constructor to include ITopicService
        public PostController(IPostService postService, IPostCommentService postCommentService, UserManager<ApplicationUser> userManager, ITopicService topicService)
        {
            _postService = postService;
            _postCommentService = postCommentService;
            _userManager = userManager;
            _topicService = topicService; // Initialize ITopicService
        }

        // GET: Topic/{topicId:int}/Post/Create
        [HttpGet("Topic/{topicId:int}/Post/Create", Name = "CreatePost")]
        [Authorize] // Ensure only authenticated users can access
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
        [Authorize] // Ensure only authenticated users can post
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int topicId, CreatePostViewModel model)
        {
            var topic = await _topicService.GetTopicByIdAsync(topicId); // Re-fetch topic to ensure it exists
            if (topic == null)
            {
                ModelState.AddModelError(string.Empty, "Topic not found.");
                model.TopicTitle = "Error: Topic not found"; // Provide a title for the view
                return View(model); // Return with error
            }
            model.TopicTitle = topic.Title; // Ensure TopicTitle is set for the view if returning due to error

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
                    Nsfw = topic.Nsfw // Inherit NSFW status from topic
                };

                try
                {
                    var createdPost = await _postService.CreatePost(createPostDto);
                    // Redirect to the newly created post's detail page
                    return RedirectToRoute("PostDetails", new { postId = createdPost.Id });
                }
                catch (Exception ex)
                {
                    // Log the error (using ILogger or your preferred logging mechanism)
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the post: " + ex.Message);
                }
            }
            return View(model); // If model state is invalid, return to the view with errors
        }

        [HttpGet("Posts/{postId:int}", Name = "PostDetails")]
        public async Task<IActionResult> Index(int postId)
        {
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                Response.StatusCode = 404;
                return View("NotFound"); // Return the custom NotFound view
            }

            var allComments = await _postCommentService.GetPostCommentsByPostIdAsync(postId);
            
            List<PostCommentDTO> commentsToDisplay;
            if (User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                commentsToDisplay = allComments.ToList(); // Managers see all comments
            }
            else
            {
                commentsToDisplay = allComments.Where(c => c.isApproved).ToList(); // Others see only approved
            }

            var commentViewModels = commentsToDisplay.Select(c => new PostCommentViewModel
            {
                Id = c.Id, 
                UserName = c.UserName,
                Content = c.Content,
                CreatedAtFormatted = Utils.TimeAgo(c.CreatedAt),
                IsApproved = c.isApproved,
                UserId = c.UserId // Added UserId
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
                catch (Exception) // Removed unused 'ex'
                {
                    // TODO: Log the exception
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while trying to post your comment. Please try again.");
                }
            }

            // If ModelState is invalid or an exception occurred, re-display the page with current data and errors
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                // This case should ideally not be reached if the user is on a valid post page
                return NotFound("The post you are trying to comment on was not found.");
            }

            var allCommentsForAddComment = await _postCommentService.GetPostCommentsByPostIdAsync(postId);
            List<PostCommentDTO> commentsToDisplayForAddComment;
            if (User.IsInRole("Manager"))
            {
                commentsToDisplayForAddComment = allCommentsForAddComment.ToList(); // Managers see all comments
            }
            else
            {
                commentsToDisplayForAddComment = allCommentsForAddComment.Where(c => c.isApproved).ToList(); // Others see only approved
            }

            var commentViewModelsForAddComment = commentsToDisplayForAddComment.Select(c => new PostCommentViewModel
            {
                Id = c.Id, 
                UserName = c.UserName,
                Content = c.Content,
                CreatedAtFormatted = Utils.TimeAgo(c.CreatedAt),
                IsApproved = c.isApproved,
                UserId = c.UserId // Added UserId
            }).ToList();

            var viewModel = new PostDetailViewModel
            {
                Post = post,
                Comments = commentViewModelsForAddComment, // Use the correctly filtered/unfiltered list
                NewComment = model, // Pass back the model with its (potentially invalid) data and validation errors
                PostCreatedAtFormatted = Utils.TimeAgo(post.CreatedAt)
            };

            return View("Index", viewModel); // Explicitly return the Index view
        }

        [HttpPost]
        [Authorize] // Ensure only authorized users can delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId, int topicId) // Added topicId to redirect back correctly
        {
            var post = await _postService.GetPostById(postId);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Post not found.";
                // Redirect to topic page if topicId is valid, otherwise to home
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
                // Log the error
                TempData["ErrorMessage"] = "An error occurred while deleting the post: " + ex.Message;
            }
            
            // Redirect back to the topic page from which the post was deleted
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
                return RedirectToAction("Index", "Home"); // Or a more appropriate error page
            }

            var topic = await _topicService.GetTopicByIdAsync(post.TopicId);
            if (topic == null)
            {
                // This should ideally not happen if post.TopicId is valid
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
                TopicTitle = topic.Title, // For display purposes in the view
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
                // It's better to redirect to the post details page or an access denied page
                return RedirectToRoute("PostDetails", new { postId = postId });
            }
            
            // Re-fetch topic title if needed for the view in case of validation errors
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
                        // ModelState.AddModelError(string.Empty, "Failed to update the post.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    TempData["ErrorMessage"] = "An error occurred while updating the post: " + ex.Message;
                    // ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                }
            }

            // If ModelState is invalid or an error occurred, return to the edit view
            return View("EditPost", model);
        }
    }
}
