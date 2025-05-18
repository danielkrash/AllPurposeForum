using AllPurposeForum.Data;
using System.Data.Common;
using AllPurposeForum.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AllPurposeForum.Service.Test;

public class AllPurposeForumFactory : IDisposable
{
    // Made fields non-readonly to allow re-initialization
    private DbConnection _connection;
    private DbContextOptions<ApplicationDbContext> _contextOptions;

    public AllPurposeForumFactory()
    {
        // Initialize the database when the factory is first created.
        ResetDatabase();
    }

    public ApplicationDbContext Context { get; private set; }

    public void ResetDatabase()
    {
        // Dispose existing context and connection if they exist
        Context?.Dispose();
        if (_connection != null)
        {
            _connection.Close(); // Close connection to release in-memory SQLite database
            _connection.Dispose();
        }

        // Create and open a new connection. This creates a new SQLite in-memory database.
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        // These options will be used by the new context instance.
        _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the schema and seed some data
        Context = new ApplicationDbContext(_contextOptions);
        Context.Database.EnsureCreated();
        SeedData();
    }

    private void SeedData()
    {
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "1",
                UserName = "daniel",
                Email = "jhon@email.com"
            },
            new ApplicationUser
            {
                Id = "2",
                UserName = "ben",
                Email = "doe@email.com"
            }
        };
        var roles = new List<IdentityRole>
        {
            new IdentityRole
            {
                Id = "1",
                Name = "Admin"
            },
            new IdentityRole
            {
                Id = "2",
                Name = "User"
            },
            new IdentityRole
            {
                Id = "3",
                Name = "Moderator"
            }
        };
        var topics = new List<Topic>
        {
            new Topic
            {
                Id = 1,
                Title = "General",
                Description = "General discussion",
                Nsfw = false,
                ApplicationUserId = "1"
                
            },
            new Topic
            {
                Id = 2,
                Title = "Technology",
                Description = "Technology discussion",
                Nsfw = false,
                ApplicationUserId = "2"
            }
        };
        var posts = new List<Post>
        {
            new Post
            {
                Id = 1,
                Title = "First post",
                Content = "This is the first post",
                Nsfw = false,
                TopicId = 1,
                ApplicationUserId = "1"
            },
            new Post
            {
                Id = 2,
                Title = "Second post",
                Content = "This is the second post",
                Nsfw = false,
                TopicId = 1,
                ApplicationUserId = "2"
            }
        };
        var comments = new List<PostComment>
        {
            new PostComment
            {
                Id = 1,
                Content = "This is a comment",
                Acceptence = true,
                PostId = 1,
                UserId = "1"
            },
            new PostComment
            {
                Id = 2,
                Content = "This is another comment",
                Acceptence = true,
                PostId = 1,
                UserId = "2"
            }
        };
        Context.ApplicationUsers.AddRange(users);
        Context.Roles.AddRange(roles);
        Context.Topics.AddRange(topics);
        Context.Posts.AddRange(posts);
        Context.PostComments.AddRange(comments);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context?.Dispose();
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}

