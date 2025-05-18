using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq; 
using System.Threading.Tasks; 

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
        var topics = await _context.Topics
            .Include(t => t.ApplicationUser) // Ensure ApplicationUser is loaded
            .Include(t => t.Posts)
            .Select(t => new TopicDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Nsfw = t.Nsfw,
                UserId = t.ApplicationUserId,
                UserName = t.ApplicationUser != null && t.ApplicationUser.UserName != null ? t.ApplicationUser.UserName : "Unknown User",
                PostsCount = t.Posts.Count
            })
            .ToListAsync();
        return topics;
    }

    public async Task<TopicDTO> GetTopicByIdAsync(int id)
    {
        var topic = await _context.Topics
            .Include(t => t.ApplicationUser) // Ensure ApplicationUser is loaded
            .Include(t => t.Posts)
            .Where(t => t.Id == id)
            .Select(t => new TopicDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Nsfw = t.Nsfw,
                UserId = t.ApplicationUserId,
                UserName = t.ApplicationUser != null && t.ApplicationUser.UserName != null ? t.ApplicationUser.UserName : "Unknown User",
                PostsCount = t.Posts.Count
            })
            .FirstOrDefaultAsync();

        if (topic == null)
        {
            throw new Exception("Topic not found");
        }
        return topic;
    }

    public async Task<CreateTopicDTO> CreateTopicAsync(CreateTopicDTO topicDto)
    {
        var user = await _context.Users.FindAsync(topicDto.UserId);
        if (user == null)
        {
            throw new Exception("User not found for topic creation");
        }

        var topic = new Topic
        {
            ApplicationUserId = topicDto.UserId,
            Title = topicDto.Title,
            Description = topicDto.Description,
            Nsfw = topicDto.Nsfw,
            ApplicationUser = user
        };

        _context.Topics.Add(topic);
        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to create topic");
        }
        return topicDto; 
    }

    public async Task<TopicDTO> UpdateTopicAsync(UpdateTopicDTO topicDto, int id)
    {
        var existingTopic = await _context.Topics
            .Include(t => t.ApplicationUser) 
            .Include(t => t.Posts) // Also include posts if PostsCount is needed
            .FirstOrDefaultAsync(t => t.Id == id);

        if (existingTopic == null)
        {
            throw new Exception("Topic not found");
        }

        existingTopic.Title = topicDto.Title;
        existingTopic.Description = topicDto.Description;
        existingTopic.Nsfw = topicDto.Nsfw;

        var result = await _context.SaveChangesAsync();
        if (result == 0)
        {
            throw new Exception("Failed to update topic");
        }

        return new TopicDTO 
        {
            Id = existingTopic.Id,
            Title = existingTopic.Title,
            Description = existingTopic.Description,
            Nsfw = existingTopic.Nsfw,
            UserId = existingTopic.ApplicationUserId,
            UserName = existingTopic.ApplicationUser != null && existingTopic.ApplicationUser.UserName != null ? existingTopic.ApplicationUser.UserName : "Unknown User",
            PostsCount = existingTopic.Posts?.Count ?? 0 
        };
    }

    public async Task<bool> DeleteTopicAsync(int id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
        {
            throw new Exception("Topic not found");
        }

        _context.Topics.Remove(topic);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<TopicDTO>> GetTopicsByUserIdAsync(string userId)
    {
        var topics = await _context.Topics
            .Include(t => t.ApplicationUser) 
            .Include(t => t.Posts)
            .Where(t => t.ApplicationUserId == userId)
            .Select(t => new TopicDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Nsfw = t.Nsfw,
                UserId = t.ApplicationUserId,
                UserName = t.ApplicationUser != null && t.ApplicationUser.UserName != null ? t.ApplicationUser.UserName : "Unknown User",
                PostsCount = t.Posts.Count
            })
            .ToListAsync();

        if (topics == null || !topics.Any())
        {
            return new List<TopicDTO>();
        }
        return topics;
    }

    public async Task<List<TopicDTO>> GetTopicsByPostIdAsync(int postId)
    {
        var post = await _context.Posts
            .Include(p => p.Topic) // Include the Topic navigation property
                .ThenInclude(t => t.ApplicationUser) // Then include the ApplicationUser of the Topic
            .Include(p => p.Topic) // Include the Topic navigation property again
                .ThenInclude(t => t.Posts) // Then include the Posts of the Topic
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null || post.Topic == null)
        {
            return new List<TopicDTO>();
        }

        var topic = post.Topic;
        return new List<TopicDTO>
        {
            new TopicDTO
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
                Nsfw = topic.Nsfw,
                UserId = topic.ApplicationUserId,
                UserName = topic.ApplicationUser != null && topic.ApplicationUser.UserName != null ? topic.ApplicationUser.UserName : "Unknown User",
                PostsCount = topic.Posts?.Count ?? 0
            }
        };
    }
}