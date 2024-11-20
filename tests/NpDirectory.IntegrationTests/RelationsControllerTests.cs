using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NpDirectory.Application.Requests;
using NpDirectory.Domain.Enum;
using NpDirectory.Domain.Models;
using NpDirectory.Infrastructure;
using Xunit;

namespace NpDirectory.IntegrationTests;

[Collection("RelationTests")]
public class RelationsControllerTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _dbName;

    public RelationsControllerTests()
    {
        _dbName = $"TestingDb_{Guid.NewGuid()}";
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("IntegrationTesting");

            builder.ConfigureServices((context, services) =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        });

        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await SeedTestData(context);
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private static async Task SeedTestData(AppDbContext context)
    {
        var persons = new List<NaturalPerson>
        {
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                PersonalNumber = "12345678901"
            },
            new()
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Doe",
                PersonalNumber = "12345678902"
            },
            new()
            {
                Id = 3,
                FirstName = "Jim",
                LastName = "Doe",
                PersonalNumber = "12345678903"
            }
        };

        var relations = new List<Relation>
        {
            new()
            {
                Id = 1,
                NaturalPersonId = 1,
                RelatedPersonId = 2,
                Type = RelationType.Acquaintance
            }
        };

        await context.NaturalPersons.AddRangeAsync(persons);
        await context.Relations.AddRangeAsync(relations);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateRelation_WhenValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new CreateRelationRequest
        {
            FromId = 1,
            ToId = 3,
            RelationType = RelationType.Acquaintance
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/relations", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    

    [Fact]
    public async Task CreateRelation_WhenPersonNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateRelationRequest
        {
            FromId = 999,
            ToId = 1,
            RelationType = RelationType.Acquaintance
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/relations", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRelation_ById_WhenExists_ReturnsOk()
    {
        // Act
        var response = await _client.DeleteAsync("/api/relations/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRelation_ById_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/relations/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRelation_ByIds_WhenExists_ReturnsOk()
    {
        // Act
        var response = await _client.DeleteAsync("/api/relations/1/2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRelation_ByIds_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/relations/1/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task CreateRelation_WithInvalidIds_ReturnsBadRequest(int fromId, int toId)
    {
        // Arrange
        var request = new CreateRelationRequest
        {
            FromId = fromId,
            ToId = toId,
            RelationType = RelationType.Acquaintance
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/relations", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRelation_WhenSameIds_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateRelationRequest
        {
            FromId = 1,
            ToId = 1,
            RelationType = RelationType.Acquaintance
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/relations", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidateDatabase_AfterOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var createRequest = new CreateRelationRequest
        {
            FromId = 1,
            ToId = 3,
            RelationType = RelationType.Acquaintance
        };
        await _client.PostAsJsonAsync("/api/relations", createRequest);

        // Act & Assert
        // Verify relation was created
        var newRelation = await context.Relations
            .FirstOrDefaultAsync(r => r.NaturalPersonId == 1 && r.RelatedPersonId == 3);
        Assert.NotNull(newRelation);

        // Delete relation
        await _client.DeleteAsync($"/api/relations/{newRelation.Id}");

        // Verify relation was deleted
        var deletedRelation = await context.Relations
            .FirstOrDefaultAsync(r => r.Id == newRelation.Id);
        Assert.Null(deletedRelation);
    }
}