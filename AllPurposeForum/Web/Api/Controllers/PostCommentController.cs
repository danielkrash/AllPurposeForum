using AllPurposeForum.Data.DTO;
using AllPurposeForum.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AllPurposeForum.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostCommentController : ControllerBase
{
    private readonly IPostCommentService _postCommentService;

    public PostCommentController(IPostCommentService postCommentService)
    {
        _postCommentService = postCommentService;
    }

    [HttpGet("list")]
    public async Task<Results<Ok<List<PostCommentDTO>>, NotFound<string>>> GetAllPostComments([FromQuery] int? commentStatus)
    {
        try
        {
            var comments = await _postCommentService.GetAllPostCommentsAsync();
            if (commentStatus.HasValue)
            {
                if (commentStatus.Value == 1)
                {
                    comments = comments.Where(c => c.isApproved).ToList();
                }
                else if (commentStatus.Value == 0)
                {
                    comments = comments.Where(c => !c.isApproved).ToList();
                }
                // If commentStatus is not 0 or 1, return all comments (no filtering)
            }
            return TypedResults.Ok(comments);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<Results<Ok<PostCommentDTO>, NotFound<string>>> GetPostCommentById(int id)
    {
        try
        {
            var comment = await _postCommentService.GetPostCommentByIdAsync(id);
            return TypedResults.Ok(comment);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpPost("create")]
    public async Task<Results<Ok<CreatePostCommentDTO>, BadRequest<string>>> CreatePostComment(
        [FromBody] CreatePostCommentDTO createCommentDto)
    {
        try
        {
            var createdComment = await _postCommentService.CreatePostCommentAsync(createCommentDto);
            return TypedResults.Ok(createdComment);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<Results<Ok<PostCommentDTO>, NotFound<string>, BadRequest<string>, ForbidHttpResult>> UpdatePostComment(int id,
        [FromBody] UpdatePostCommentDTO updateCommentDto)
    {

        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return TypedResults.Forbid();
            }

            var isAdminOrModerator = User.IsInRole("Admin") || User.IsInRole("Moderator");
            
            if (!isAdminOrModerator)
            {
                var existingComment = await _postCommentService.GetPostCommentByIdAsync(id);
                if (existingComment.UserId != currentUserId)
                {
                    return TypedResults.Forbid();
                }
            }

            var updatedComment = await _postCommentService.UpdatePostCommentAsync(updateCommentDto, id);
            return TypedResults.Ok(updatedComment);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return TypedResults.NotFound(ex.Message);

            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<Results<Ok, NotFound<string>, BadRequest<string>, ForbidHttpResult>> DeletePostComment(int id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return TypedResults.Forbid();
            }

            var isAdminOrModerator = User.IsInRole("Admin") || User.IsInRole("Moderator");
            
            if (!isAdminOrModerator)
            {
                var existingComment = await _postCommentService.GetPostCommentByIdAsync(id);
                if (existingComment.UserId != currentUserId)
                {
                    return TypedResults.Forbid();
                }
            }

            var result = await _postCommentService.DeletePostCommentAsync(id);
            if (result) return TypedResults.Ok();
            
            return TypedResults.BadRequest("Failed to delete post comment.");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return TypedResults.NotFound(ex.Message);

            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<Results<Ok<List<PostCommentDTO>>, NotFound<string>>> GetPostCommentsByUserId(string userId, [FromQuery] int? commentStatus)
    {
        try
        {
            var comments = await _postCommentService.GetPostCommentsByUserIdAsync(userId);
            if (commentStatus.HasValue)
            {
                if (commentStatus.Value == 1)
                {
                    comments = comments.Where(c => c.isApproved).ToList();
                }
                else if (commentStatus.Value == 0)
                {
                    comments = comments.Where(c => !c.isApproved).ToList();
                }
                // If commentStatus is not 0 or 1, return all comments (no filtering)
            }
            return TypedResults.Ok(comments);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("post/{postId}")]
    public async Task<Results<Ok<List<PostCommentDTO>>, NotFound<string>>> GetPostCommentsByPostId(int postId, [FromQuery] int? commentStatus)
    {
        try
        {
            var comments = await _postCommentService.GetPostCommentsByPostIdAsync(postId);
            if (commentStatus.HasValue)
            {
                if (commentStatus.Value == 1)
                {
                    comments = comments.Where(c => c.isApproved).ToList();
                }
                else if (commentStatus.Value == 0)
                {
                    comments = comments.Where(c => !c.isApproved).ToList();
                }
                // If commentStatus is not 0 or 1, return all comments (no filtering)
            }
            return TypedResults.Ok(comments);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }
}