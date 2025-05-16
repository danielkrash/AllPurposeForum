using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System.Threading.Tasks;
using AllPurposeForum.Data;
using AllPurposeForum.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AllPurposeForum.Identity.Test;

public class UserTest : IClassFixture<AllPurposeForumFactory>
{
    AllPurposeForumFactory _factory;

    public UserTest(AllPurposeForumFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateUser_WhenSuccessful_ReturnsSuccessIdentityResult()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserName = "daniel",
            Email = "krashol72@gmail.com"
        };
        var password = "Password123!";

        // UserManager has a complex constructor.
        // For simplicity, we'll mock UserManager directly here.
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManagerMock = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);

        userManagerMock
            .CreateAsync(user, password)
            .Returns(Task.FromResult(IdentityResult.Success));

        // Act
        var result = await userManagerMock.CreateAsync(user, password);

        // Assert
        Assert.True(result.Succeeded);
        await userManagerMock.Received(1).CreateAsync(user, password);
    }
    [Fact]
    public async Task CreateUser_WhenSuccessful_ReturnsSuccessIdentityResult_WithoutMocking()
    {
        // Arrange
        var user = new ApplicationUser()
        {
            UserName = "daniel",
            Email = "krashol72@gmail.com"
        };
        var password = "Password123!";
        var test = await _factory.Context.Users.AddAsync(user);
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userStore = new UserStore<ApplicationUser>(_factory.Context);
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var userValidator = new UserValidator<ApplicationUser>();
        var passwordValidator = new PasswordValidator<ApplicationUser>();
        var options = new IdentityOptions();
        var optionsAccessor = Substitute.For<IOptions<IdentityOptions>>();
        optionsAccessor.Value.Returns(options);
        var userManager = new UserManager<ApplicationUser>(
            userStore,
            optionsAccessor,
            passwordHasher,
            [userValidator],
            [passwordValidator],
            null,
            null,
            null,
            null);
        var userManagerMock = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);

        userManagerMock
            .CreateAsync(user, password)
            .Returns(Task.FromResult(IdentityResult.Success));

        // Act
        var result = await userManagerMock.CreateAsync(user, password);
        var result2 = await userManager.CreateAsync(user, password);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result2.Succeeded);
        await userManagerMock.Received(1).CreateAsync(user, password);
    }

    [Fact]
    public async Task CreateUser_WhenFails_ReturnsFailedIdentityResult()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserName = "testuser",
            Email = "test@example.com"
        };
        var password = "Password123!";
        var expectedError = new IdentityError { Code = "TestError", Description = "Test error description" };

        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManagerMock = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);

        userManagerMock
            .CreateAsync(user, password)
            .Returns(Task.FromResult(IdentityResult.Failed(expectedError)));

        // Act
        var result = await userManagerMock.CreateAsync(user, password);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(expectedError, result.Errors);
        await userManagerMock.Received(1).CreateAsync(user, password);
    }
}