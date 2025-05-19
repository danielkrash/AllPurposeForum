using AllPurposeForum.Data.DTO;
using AllPurposeForum.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AllPurposeForum.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
            return TypedResults.NotFound(ex.Message); // Or BadRequest depending on expected error type
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
    // Consider adding [Authorize(Policy = "RequireAdministratorRole")] or other appropriate policy
    public async Task<Results<Ok<CreateTopicDTO>, BadRequest<string>>> CreateTopic(
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
    // Consider adding authorization, e.g., only topic owner or admin can update
    public async Task<Results<Ok<TopicDTO>, NotFound<string>, BadRequest<string>>> UpdateTopic(int id,
        [FromBody] UpdateTopicDTO updateTopicDto)
    {
        // It's good practice to ensure the ID in the route matches the ID in the DTO if present,
        // however, UpdateTopicDTO doesn't have an Id. The service method takes 'id' separately.
        try
        {
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
    // Consider adding authorization, e.g., only topic owner or admin can delete
    public async Task<Results<Ok, NotFound<string>, BadRequest<string>>> DeleteTopic(int id)
    {
        try
        {
            var result = await _topicService.DeleteTopicAsync(id);
            if (result) return TypedResults.Ok();

            return TypedResults.BadRequest("Failed to delete topic."); // Or NotFound if preferred for this case
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