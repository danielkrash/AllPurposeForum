using AllPurposeForum.Data.DTO;
using AllPurposeForum.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;

namespace AllPurposeForum.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost("create")]
    /*[Authorize(Policy = "RequireAdministratorRole")]*/
    public async Task<Results<Ok<CreatePostDTO>, BadRequest<string>>> CreatePost([FromBody] CreatePostDTO createPostDto)
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
    // Add authorization if needed, e.g., only post owner or admin can update
    public async Task<Results<Ok<UpdatePostDTO>, NotFound<string>, BadRequest<string>>> UpdatePost(int id,
        [FromBody] UpdatePostDTO updatePostDto)
    {
        if (id != updatePostDto.Id)
        {
            return TypedResults.BadRequest("Post ID in URL must match Post ID in body.");
        }

        try
        {
            var updatedPost = await _postService.UpdatePost(updatePostDto);
            return TypedResults.Ok(updatedPost);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return TypedResults.NotFound(ex.Message);
            }

            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    // Add authorization if needed, e.g., only post owner or admin can delete
    public async Task<Results<Ok, NotFound<string>, BadRequest<string>>> DeletePost(int id)
    {
        try
        {
            var result = await _postService.DeletePost(id);
            if (result) // Assuming DeletePost returns true on success
            {
                return TypedResults.Ok();
            }

            // This path might indicate a failure that wasn't an exception (e.g., service returns false)
            return TypedResults.BadRequest("Failed to delete post.");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return TypedResults.NotFound(ex.Message);
            }

            return TypedResults.BadRequest(ex.Message);
        }
    }
}