using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NpDirectory.Application.Common;
using NpDirectory.Application.Requests;
using NpDirectory.Application.Responses;
using NpDirectory.Domain.Enum;
using NpDirectory.Domain.Models;
using NpDirectory.Infrastructure;
using Xunit;

namespace NpDirectory.IntegrationTests;

[Collection("NaturalPersonTests")]
public class NaturalPersonsControllerTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _dbName;

    public NaturalPersonsControllerTests()
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
        var city = new City
        {
            Id = 1,
            Name = "Test City"
        };
        await context.Cities.AddAsync(city);

        var persons = new List<NaturalPerson>
        {
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                PersonalNumber = "12345678901",
                BirthDate = new DateTime(1990, 1, 1),
                Sex = Sex.Male,
                CityId = 1,
                City = city
            },
            new()
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                PersonalNumber = "12345678902",
                BirthDate = new DateTime(1992, 2, 2),
                Sex = Sex.Female,
                CityId = 1,
                City = city
            }
        };

        await context.NaturalPersons.AddRangeAsync(persons);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetNaturalPersonInfo_WhenExists_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/naturalpersons/1");
        var content = await response.Content.ReadFromJsonAsync<GetNaturalPersonInfoResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal("John", content.PersonInfo.FirstName);
        Assert.Equal("Doe", content.PersonInfo.LastName);
    }

    [Fact]
    public async Task GetNaturalPersonInfo_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/naturalpersons/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FastSearchNaturalPersons_WithValidQuery_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/naturalpersons/fast-search?searchTerm=John");
        var content = await response.Content.ReadFromJsonAsync<FastSearchNaturalPersonsResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Contains(content.Persons, p => p.FirstName.Contains("John"));
    }

    [Fact]
    public async Task SearchNaturalPersons_WithValidCriteria_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/naturalpersons/search?firstName=John");
        var content = await response.Content.ReadFromJsonAsync<SearchNaturalPersonsResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Contains(content.Persons, p => p.FirstName == "John");
    }

    [Fact]
    public async Task CreateNaturalPerson_WithValidData_ReturnsOk()
    {
        // Arrange
        var request = new CreateNaturalPersonRequest
        {
            FirstName = "Alice",
            LastName = "Johnson",
            PersonalNumber = "12345678903",
            BirthDate = new DateTime(1995, 5, 5),
            Sex = Sex.Female,
            CityId = 1,
            PhoneNumbers = new List<PhoneNumberModel>()
            {
                new()
                {
                    Number = "123456789",
                    Type = PhoneNumberType.Mobile
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/naturalpersons", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify creation
        var getResponse = await _client.GetAsync("/api/naturalpersons/3");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var content = await getResponse.Content.ReadFromJsonAsync<GetNaturalPersonInfoResponse>();
        Assert.Equal("Alice", content.PersonInfo.FirstName);
    }

    [Fact]
    public async Task CreateNaturalPerson_WithDuplicatePersonalNumber_ReturnsConflict()
    {
        // Arrange
        var request = new CreateNaturalPersonRequest
        {
            FirstName = "Test",
            LastName = "User",
            PersonalNumber = "12345678901", // Already exists
            BirthDate = new DateTime(1995, 5, 5),
            Sex = Sex.Male,
            CityId = 1,
            PhoneNumbers = new List<PhoneNumberModel>()
            {
                new()
                {
                    Number = "123456789",
                    Type = PhoneNumberType.Mobile
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/naturalpersons", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNaturalPerson_WhenExists_ReturnsOk()
    {
        // Arrange
        var request = new UpdateNaturalPersonRequest
        {
            FirstName = "JohnUpdated",
            LastName = "DoeUpdated",
            BirthDate = new DateTime(1990, 1, 1),
            Sex = Sex.Male,
            CityId = 1,
            PersonalNumber = "12345678901",
            PhoneNumbers = new List<PhoneNumberModel>()
            {
                new()
                {
                    Number = "123456789",
                    Type = PhoneNumberType.Mobile
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/naturalpersons/1", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify update
        var getResponse = await _client.GetAsync("/api/naturalpersons/1");
        var content = await getResponse.Content.ReadFromJsonAsync<GetNaturalPersonInfoResponse>();
        Assert.Equal("JohnUpdated", content.PersonInfo.FirstName);
        Assert.Equal("DoeUpdated", content.PersonInfo.LastName);
    }

    [Fact]
    public async Task DeleteNaturalPerson_WhenExists_ReturnsOk()
    {
        // Act
        var response = await _client.DeleteAsync("/api/naturalpersons/2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync("/api/naturalpersons/2");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GenerateReport_WithValidRequest_ReturnsOk()
    {
        // Arrange
        // Act
        var response = await _client.GetAsync($"/api/naturalpersons/report");
        var content = await response.Content.ReadFromJsonAsync<GenerateReportResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
    }

    [Fact]
    public async Task UpdateNaturalPersonImage_WithValidImage_ReturnsOk()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        var folder = "TestFiles";
        var fileName = "test.jpg";
        
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        
        if (!File.Exists(Path.Combine(folder, fileName)))
            await File.WriteAllBytesAsync(Path.Combine(folder, fileName), new byte[1024]);
        
        var imageBytes = await File.ReadAllBytesAsync(Path.Combine(folder, fileName));
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "image", "test.jpg");

        // Act
        var response = await _client.PutAsync("/api/naturalpersons/1/image", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        File.Delete(Path.Combine(folder, fileName));
        Directory.Delete(folder);
    }
}