using AllPurposeForum.Data.DTO;

namespace AllPurposeForum.Services;

public interface ITopicService
{
    public Task<List<TopicDTO>> GetAllTopicsAsync();
    public Task<TopicDTO> GetTopicByIdAsync(int id);
    public Task<TopicDTO> CreateTopicAsync(CreateTopicDTO topic); // Changed return type to TopicDTO
    public Task<TopicDTO> UpdateTopicAsync(UpdateTopicDTO topic, int id);
    public Task<bool> DeleteTopicAsync(int id);
    public Task<List<TopicDTO>> GetTopicsByUserIdAsync(string userId);
    public Task<List<TopicDTO>> GetTopicsByPostIdAsync(int postId);
}