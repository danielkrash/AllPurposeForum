using AllPurposeForum.Data.DTO;
using AllPurposeForum.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AllPurposeForum.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostCommentController : ControllerBase
{
    private readonly IPostCommentService _postCommentService;

    public PostCommentController(IPostCommentService postCommentService)
    {
        _postCommentService = postCommentService;
    }

    [HttpGet("list")]
    public async Task<Results<Ok<List<PostCommentDTO>>, NotFound<string>>> GetAllPostComments()
    {
        try
        {
            var comments = await _postCommentService.GetAllPostCommentsAsync();
            return TypedResults.Ok(comments);
        }
        catch (Exception ex)
        {
            // Assuming "Post not found" or similar if list is empty and service throws.
            // Adjust if service returns empty list instead of throwing for "not found".
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
    // Consider adding [Authorize] or a specific policy
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
    // Consider adding [Authorize] or a specific policy (e.g., only comment owner or admin)
    public async Task<Results<Ok<PostCommentDTO>, NotFound<string>, BadRequest<string>>> UpdatePostComment(int id,
        [FromBody] UpdatePostCommentDTO updateCommentDto)
    {
        // Optional: Add check if id in route matches an Id property in updateCommentDto if it exists.
        // if (id != updateCommentDto.Id) return TypedResults.BadRequest("ID mismatch");

        try
        {
            var updatedComment = await _postCommentService.UpdatePostCommentAsync(updateCommentDto, id);
            return TypedResults.Ok(updatedComment);
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
    // Consider adding [Authorize] or a specific policy (e.g., only comment owner or admin)
    public async Task<Results<Ok, NotFound<string>, BadRequest<string>>> DeletePostComment(int id)
    {
        try
        {
            var result = await _postCommentService.DeletePostCommentAsync(id);
            if (result)
            {
                return TypedResults.Ok();
            }

            // This case might be hit if DeletePostCommentAsync returns false for a non-exception failure
            return TypedResults.BadRequest("Failed to delete post comment.");
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

    [HttpGet("user/{userId}")]
    public async Task<Results<Ok<List<PostCommentDTO>>, NotFound<string>>> GetPostCommentsByUserId(string userId)
    {
        try
        {
            var comments = await _postCommentService.GetPostCommentsByUserIdAsync(userId);
            // The service throws if "Post not found" (which might mean no comments for user)
            // If it could return an empty list, an additional check for comments.Count == 0 might be desired
            // to return NotFound explicitly, or just Ok(emptyList).
            return TypedResults.Ok(comments);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("post/{postId}")]
    public async Task<Results<Ok<List<PostCommentDTO>>, NotFound<string>>> GetPostCommentsByPostId(int postId)
    {
        try
        {
            var comments = await _postCommentService.GetPostCommentsByPostIdAsync(postId);
            // Similar to GetPostCommentsByUserIdAsync, service throws if "Post not found"
            return TypedResults.Ok(comments);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }
}