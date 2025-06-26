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
public class PostController : ControllerBase
{
    private readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost("create")]
    public async Task<Results<Ok<PostDTO>, BadRequest<string>>> CreatePost([FromBody] CreatePostDTO createPostDto)
    {
        try
        {
            var createdPost = await _postService.CreatePost(createPostDto);
            return TypedResults.Ok(createdPost);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<Results<Ok<PostDTO>, NotFound<string>>> GetPostById(int id)
    {
        try
        {
            var post = await _postService.GetPostById(id);
            return TypedResults.Ok(post);
        }
        catch (Exception ex)
        {
            // Assuming "Post not found" is a common exception message for not found scenarios
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("list")]
    public async Task<Results<Ok<List<PostDTO>>, NotFound<string>>> GetAllPosts()
    {
        try
        {
            var posts = await _postService.GetAllPosts();
            return TypedResults.Ok(posts);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("topic/{topicId}")]
    public async Task<Results<Ok<List<PostDTO>>, NotFound<string>>> GetPostsByTopicId(int topicId)
    {
        try
        {
            var posts = await _postService.GetPostsByTopicId(topicId);
            return TypedResults.Ok(posts);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<Results<Ok<List<PostDTO>>, NotFound<string>>> GetPostsByUserId(string userId)
    {
        try
        {
            var posts = await _postService.GetPostsByUserId(userId);
            return TypedResults.Ok(posts);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("user/{userId}/topic/{topicId}")]
    public async Task<Results<Ok<List<PostDTO>>, NotFound<string>>> GetPostsByUserIdAndTopicId(string userId,
        int topicId)
    {
        try
        {
            var posts = await _postService.GetPostsByUserIdAndTopicId(userId, topicId);
            return TypedResults.Ok(posts);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<Results<Ok<UpdatePostDTO>, NotFound<string>, BadRequest<string>, ForbidHttpResult>> UpdatePost(int id,
        [FromBody] UpdatePostDTO updatePostDto)
    {
        if (id != updatePostDto.Id) return TypedResults.BadRequest("Post ID in URL must match Post ID in body.");

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
                var existingPost = await _postService.GetPostById(id);
                if (existingPost?.UserId != currentUserId)
                {
                    return TypedResults.Forbid();
                }
            }

            var updatedPost = await _postService.UpdatePost(updatePostDto);
            return TypedResults.Ok(updatedPost);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return TypedResults.NotFound(ex.Message);

            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<Results<Ok, NotFound<string>, BadRequest<string>, ForbidHttpResult>> DeletePost(int id)
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
                var existingPost = await _postService.GetPostById(id);
                if (existingPost?.UserId != currentUserId)
                {
                    return TypedResults.Forbid();
                }
            }

            var result = await _postService.DeletePost(id);
            if (result)
                return TypedResults.Ok();

            return TypedResults.BadRequest("Failed to delete post.");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return TypedResults.NotFound(ex.Message);

            return TypedResults.BadRequest(ex.Message);
        }
    }
}