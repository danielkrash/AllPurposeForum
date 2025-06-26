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
public class TopicController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    [HttpGet("list")]
    public async Task<Results<Ok<List<TopicDTO>>, NotFound<string>>> GetAllTopics()
    {
        try
        {
            var topics = await _topicService.GetAllTopicsAsync();
            return TypedResults.Ok(topics);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<Results<Ok<TopicDTO>, NotFound<string>>> GetTopicById(int id)
    {
        try
        {
            var topic = await _topicService.GetTopicByIdAsync(id);
            return TypedResults.Ok(topic);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpPost("create")]
    public async Task<Results<Ok<TopicDTO>, BadRequest<string>>> CreateTopic(
        [FromBody] CreateTopicDTO createTopicDto)
    {
        try
        {
            var createdTopic = await _topicService.CreateTopicAsync(createTopicDto);
            return TypedResults.Ok(createdTopic);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<Results<Ok<TopicDTO>, NotFound<string>, BadRequest<string>, ForbidHttpResult>> UpdateTopic(int id,
        [FromBody] UpdateTopicDTO updateTopicDto)
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
                var existingTopic = await _topicService.GetTopicByIdAsync(id);
                if (existingTopic?.UserId != currentUserId)
                {
                    return TypedResults.Forbid();
                }
            }

            var updatedTopic = await _topicService.UpdateTopicAsync(updateTopicDto, id);
            return TypedResults.Ok(updatedTopic);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return TypedResults.NotFound(ex.Message);

            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<Results<Ok, NotFound<string>, BadRequest<string>, ForbidHttpResult>> DeleteTopic(int id)
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
                var existingTopic = await _topicService.GetTopicByIdAsync(id);
                if (existingTopic?.UserId != currentUserId)
                {
                    return TypedResults.Forbid();
                }
            }

            var result = await _topicService.DeleteTopicAsync(id);
            if (result) return TypedResults.Ok();

            return TypedResults.BadRequest("Failed to delete topic.");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return TypedResults.NotFound(ex.Message);

            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<Results<Ok<List<TopicDTO>>, NotFound<string>>> GetTopicsByUserId(string userId)
    {
        try
        {
            var topics = await _topicService.GetTopicsByUserIdAsync(userId);
            return TypedResults.Ok(topics);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }

    [HttpGet("post/{postId}")]
    public async Task<Results<Ok<List<TopicDTO>>, NotFound<string>>> GetTopicsByPostId(int postId)
    {
        try
        {
            var topics = await _topicService.GetTopicsByPostIdAsync(postId);
            return TypedResults.Ok(topics);
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }
}