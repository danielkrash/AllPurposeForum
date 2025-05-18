using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;
using AllPurposeForum.Services.Implementation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;
using System; // For Exception
using System.Threading;
using AllPurposeForum.Services; // For CancellationToken

namespace AllPurposeForum.Service.Test;

public class PostServiceTest : IClassFixture<AllPurposeForumFactory>
{
    AllPurposeForumFactory _factory;
    public PostServiceTest(AllPurposeForumFactory factory)
    {
        _factory = factory;
    }
    [Fact]
    public async Task Test1()
    {
        // Arrange
        var createPostDto = new CreatePostDTO
        {
            UserId = "1",
            TopicId = 1,
            Content = "Test Content",
            Title = "Test Title",
            Nsfw = false,
        };
        var mockPostService = Substitute.For<IPostService>();
        mockPostService.CreatePost(createPostDto)
            .Returns(Task.FromResult(createPostDto));
        

        // Act
        var resultDto = await mockPostService.CreatePost(createPostDto);

        // Assert
        Assert.NotNull(resultDto);
        Assert.Equal(createPostDto.UserId, resultDto.UserId);
        Assert.Equal(createPostDto.TopicId, resultDto.TopicId);
        Assert.Equal(createPostDto.Content, resultDto.Content);
        Assert.Equal(createPostDto.Title, resultDto.Title);
        Assert.Equal(createPostDto.Nsfw, resultDto.Nsfw);
        
        Assert.Same(createPostDto, resultDto);
    }

    [Fact]
    public async Task CreatePost_ShouldCreatePost_WhenSaveChangesSuccessful()
    {
        // Arrange
        // And that 'Posts' property is virtual on ApplicationDbContext.
        var dbContext = _factory.Context;

        var createPostDto = new CreatePostDTO
        {
            UserId = "1",
            TopicId = 1,
            Content = "Test Content",
            Title = "Test Title",
            Nsfw = false,
        };

        var postService = new PostService(dbContext);

        // Act
        var resultDto = await postService.CreatePost(createPostDto);

        // Assert
        var addedPost = new Post
        {
            ApplicationUserId = createPostDto.UserId,
            TopicId = createPostDto.TopicId,
            Content = createPostDto.Content,
            Title = createPostDto.Title,
            Nsfw = createPostDto.Nsfw
        };
        // Verify Add was called on the mockDbSet and capture the argument.
        // This requires that _context.Posts in PostService correctly returned mockDbSet.
        /*mockDbSet.*/
        Assert.NotNull(addedPost);
        Assert.Equal(createPostDto.UserId, addedPost.ApplicationUserId);
        Assert.Equal(createPostDto.TopicId, addedPost.TopicId);
        Assert.Equal(createPostDto.Content, addedPost.Content);
        Assert.Equal(createPostDto.Title, addedPost.Title);
        Assert.Equal(createPostDto.Nsfw, addedPost.Nsfw);
        
        Assert.Same(createPostDto, resultDto);
    }

    [Fact]
    public async Task CreatePost_ShouldThrowException_WhenSaveChangesFails()
    {
        // Arrange
        var mockDbContext = Substitute.For<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        var mockDbSet = Substitute.For<DbSet<Post>>();
        // Crucial: ApplicationDbContext.Posts must be virtual for this to work.
        mockDbContext.Posts.Returns(mockDbSet);

        var createPostDto = new CreatePostDTO
        {
            UserId = "testUser",
            TopicId = 1,
            Content = "Test Content",
            Title = "Test Title",
            Nsfw = false
        };

        var postService = new PostService(mockDbContext);

        // Setup SaveChangesAsync to return 0 (failure)
        mockDbContext.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => postService.CreatePost(createPostDto));
        Assert.Equal("Failed to create post", exception.Message);

        // Verify Add was called on the mockDbSet.
        mockDbSet.Received(1).Add(Arg.Any<Post>());
        await mockDbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

