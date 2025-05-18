using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AllPurposeForum.Services;

public class TopicService : ITopicService
{
    private readonly ApplicationDbContext _context;

    public TopicService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TopicDTO>> GetAllTopicsAsync()
    {
        var topic = await _context.Topics.Include(e => e.ApplicationUser).Include(e => e.Posts)
            .ToListAsync();
        return await Task.FromResult(topic.Select(e => new TopicDTO
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            UserId = e.ApplicationUserId,
            UserName = e.ApplicationUser.UserName,
            PostsCount = e.Posts.Count
        }).ToList());
    }

    public async Task<TopicDTO> GetTopicByIdAsync(int id)
    {
        var topic = await _context.Topics
            .Include(e => e.ApplicationUser)
            .Include(e => e.Posts)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (topic == null)
        {
            throw new Exception("Topic not found");
        }

        return await Task.FromResult(new TopicDTO
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            UserId = topic.ApplicationUserId,
            UserName = topic.ApplicationUser.UserName,
            PostsCount = topic.Posts.Count
        });
    }

    public async Task<CreateTopicDTO> CreateTopicAsync(CreateTopicDTO topic)
    {
        var newTopic = new Topic
        {
            ApplicationUserId = topic.UserId,
            Title = topic.Title,
            Description = topic.Description,
            Nsfw = topic.Nsfw
        };
        _context.Add(newTopic);
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to create topic");
        }

        return await Task.FromResult(new CreateTopicDTO
        {
            Title = newTopic.Title,
            Description = newTopic.Description,
            UserId = newTopic.ApplicationUserId,
            Nsfw = newTopic.Nsfw
        });
    }

    public async Task<TopicDTO> UpdateTopicAsync(UpdateTopicDTO topic, int id)
    {
        var existingTopic = await _context.Topics
            .Include(e => e.ApplicationUser)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (existingTopic == null)
        {
            throw new Exception("Topic not found");
        }

        existingTopic.Title = topic.Title;
        existingTopic.Description = topic.Description;
        existingTopic.Nsfw = topic.Nsfw;
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to update topic");
        }

        return await Task.FromResult(new TopicDTO
        {
            Id = existingTopic.Id,
            Title = existingTopic.Title,
            Description = existingTopic.Description,
            UserId = existingTopic.ApplicationUserId,
            UserName = existingTopic.ApplicationUser.UserName
        });
    }

    public async Task<bool> DeleteTopicAsync(int id)
    {
        var topic = await _context.Topics
            .Include(e => e.ApplicationUser)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (topic == null)
        {
            throw new Exception("Topic not found");
        }

        _context.Topics.Remove(topic);
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to delete topic");
        }

        return true;
    }

    public async Task<List<TopicDTO>> GetTopicsByUserIdAsync(string userId)
    {
        var topics = await _context.Topics
            .Include(e => e.ApplicationUser)
            .Include(e => e.Posts)
            .Where(e => e.ApplicationUserId == userId)
            .ToListAsync();
        if (topics == null || topics.Count == 0)
        {
            throw new Exception("No topics found for this user");
        }

        return await Task.FromResult(topics.Select(e => new TopicDTO
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            UserId = e.ApplicationUserId,
            UserName = e.ApplicationUser.UserName,
            PostsCount = e.Posts.Count
        }).ToList());
    }

    public async Task<List<TopicDTO>> GetTopicsByPostIdAsync(int postId)
    {
        var topics = await _context.Topics
            .Include(e => e.ApplicationUser)
            .Include(e => e.Posts)
            .Where(e => e.Posts.Any(p => p.Id == postId))
            .ToListAsync();
        if (topics == null || topics.Count == 0)
        {
            throw new Exception("No topics found for this post");
        }

        return await Task.FromResult(topics.Select(e => new TopicDTO
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            UserId = e.ApplicationUserId,
            UserName = e.ApplicationUser.UserName,
            PostsCount = e.Posts.Count
        }).ToList());
    }
}