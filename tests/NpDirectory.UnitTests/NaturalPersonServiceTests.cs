using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using NpDirectory.Application;
using NpDirectory.Application.Common;
using NpDirectory.Application.Exceptions;
using NpDirectory.Application.ReadModels;
using NpDirectory.Application.Requests;
using NpDirectory.Application.Services;
using NpDirectory.Domain.Enum;
using NpDirectory.Domain.Models;

namespace NpDirectory.UnitTests;

public class NaturalPersonsServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IStringLocalizer> _localizerMock;
    private readonly INaturalPersonsService _service;

    public NaturalPersonsServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();
        _localizerMock = new Mock<IStringLocalizer>();
        _service = new NaturalPersonsService(_uowMock.Object, _fileServiceMock.Object, _localizerMock.Object);

        // Setup common localizer behavior
        _localizerMock.Setup(x => x["Error.NaturalPerson.NotFound"])
            .Returns(new LocalizedString("Error.NaturalPerson.NotFound", "Not found"));
        _localizerMock.Setup(x => x["Error.NaturalPerson.AlreadyExists"])
            .Returns(new LocalizedString("Error.NaturalPerson.AlreadyExists", "Already exists"));
        _localizerMock.Setup(x => x["Error.PersonalNumber.AlreadyExists"])
            .Returns(new LocalizedString("Error.PersonalNumber.AlreadyExists", "Personal number exists"));
    }

    [Fact]
    public async Task GetNaturalPersonInfoAsync_WhenPersonExists_ReturnsPersonInfo()
    {
        // Arrange
        var personInfo = new GetNaturalPersonInfoReadModel()
        {
            PersonInfo = new PersonInfoModel { Id = 1 },
            RelatedPersons = new List<RelatedPersonModel>()
        };
        
        _uowMock.Setup(x => x.NaturalPersonsRepository.GetNaturalPersonInfoAsync(1))
            .ReturnsAsync(personInfo);

        // Act
        var result = await _service.GetNaturalPersonInfoAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PersonInfo.Id);
    }

    [Fact]
    public async Task GetNaturalPersonInfoAsync_WhenPersonNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _uowMock.Setup(x => x.NaturalPersonsRepository.GetNaturalPersonInfoAsync(1))
            .ReturnsAsync((GetNaturalPersonInfoReadModel)null);

        // Act
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetNaturalPersonInfoAsync(1));
    }

    [Fact]
    public async Task SearchNaturalPersonsAsync_ReturnsMatchingPersons()
    {
        // Arrange
        var request = new SearchNaturalPersonsRequest
        {
            FirstName = "John",
            Page = 0,
            PageSize = 10
        };

        var persons = new List<NaturalPerson>
        {
            new() { Id = 1, FirstName = "John", PhoneNumbers = new List<PhoneNumber>() }
        };

        _uowMock.Setup(x => x.NaturalPersonsRepository.GetManyByFilterAsync(
                It.IsAny<Expression<Func<NaturalPerson, bool>>>(), 
                request.Page, 
                request.PageSize,
                It.IsAny<bool>()))
            .ReturnsAsync(persons);

        // Act
        var result = await _service.SearchNaturalPersonsAsync(request);

        // Assert
        Assert.Single(result.Persons);
        Assert.Equal("John", result.Persons[0].FirstName);
    }

    [Fact]
    public async Task FastSearchNaturalPersonAsync_ReturnsMatchingPersons()
    {
        // Arrange
        var request = new FastSearchNaturalPersonRequest
        {
            FirstName = "John",
            Page = 0,
            PageSize = 10
        };

        var persons = new List<NaturalPerson>
        {
            new() { Id = 1, FirstName = "John", PhoneNumbers = new List<PhoneNumber>() }
        };

        _uowMock.Setup(x => x.NaturalPersonsRepository.GetManyByFilterAsync(
                It.IsAny<Expression<Func<NaturalPerson, bool>>>(),
                request.Page,
                request.PageSize,
                It.IsAny<bool>()))
            .ReturnsAsync(persons);

        // Act
        var result = await _service.FastSearchNaturalPersonAsync(request);

        // Assert
        Assert.Single(result.Persons);
        Assert.Equal("John", result.Persons[0].FirstName);
    }

    [Fact]
    public async Task CreateNaturalPersonAsync_WhenPersonNotExists_CreatesNewPerson()
    {
        // Arrange
        var request = new CreateNaturalPersonRequest
        {
            FirstName = "John",
            LastName = "Doe",
            PersonalNumber = "123456"
        };

        _uowMock.Setup(x => x.NaturalPersonsRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<NaturalPerson, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync((NaturalPerson)null);
        _uowMock.Setup(x => x.PhoneNumbersRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<PhoneNumber, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync((PhoneNumber)null);

        // Act
        await _service.CreateNaturalPersonAsync(request);

        // Assert
        _uowMock.Verify(x => x.NaturalPersonsRepository.CreateAsync(It.IsAny<NaturalPerson>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateNaturalPersonAsync_WhenPersonExists_ThrowsNaturalPersonExistsException()
    {
        // Arrange
        var request = new CreateNaturalPersonRequest { PersonalNumber = "123456" };
        _uowMock.Setup(x => x.NaturalPersonsRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<NaturalPerson, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new NaturalPerson());

        // Act
        // Assert
        await Assert.ThrowsAsync<NaturalPersonExistsException>(() => 
            _service.CreateNaturalPersonAsync(request));
    }

    [Fact]
    public async Task UpdateNaturalPersonAsync_WhenPersonExists_UpdatesPerson()
    {
        // Arrange
        var id = 1;
        var request = new UpdateNaturalPersonRequest
        {
            FirstName = "John",
            LastName = "Doe",
            PersonalNumber = "123456"
        };

        var existingPerson = new NaturalPerson { Id = id };
        _uowMock.Setup(x => x.NaturalPersonsRepository.GetOneByIdAsync(id, It.IsAny<bool>()))
            .ReturnsAsync(existingPerson);
        _uowMock.Setup(x => x.PhoneNumbersRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<PhoneNumber, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync((PhoneNumber)null);

        // Act
        await _service.UpdateNaturalPersonAsync(id, request);

        // Assert
        Assert.Equal(request.FirstName, existingPerson.FirstName);
        Assert.Equal(request.LastName, existingPerson.LastName);
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateNaturalPersonImageAsync_WhenPersonExists_UpdatesImage()
    {
        // Arrange
        var id = 1;
        var request = new UpdateNaturalPersonImageRequest
        {
            Image = Mock.Of<IFormFile>()
        };

        var imagePath = "path/to/image.jpg";
        var existingPerson = new NaturalPerson { Id = id };

        _uowMock.Setup(x => x.NaturalPersonsRepository.GetOneByIdAsync(id, It.IsAny<bool>()))
            .ReturnsAsync(existingPerson);
        _fileServiceMock.Setup(x => x.UploadFileAsync(request.Image))
            .ReturnsAsync(imagePath);

        // Act
        await _service.UpdateNaturalPersonImageAsync(id, request);

        // Assert
        Assert.Equal(imagePath, existingPerson.ImageUrl);
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteNaturalPersonAsync_WhenPersonExists_DeletesPerson()
    {
        // Arrange
        var id = 1;
        _uowMock.Setup(x => x.NaturalPersonsRepository.DeleteSingleAsync(id))
            .ReturnsAsync(true);

        // Act
        await _service.DeleteNaturalPersonAsync(id);

        // Assert
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteNaturalPersonAsync_WhenPersonNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = 1;
        _uowMock.Setup(x => x.NaturalPersonsRepository.DeleteSingleAsync(id))
            .ReturnsAsync(false);

        // Act
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.DeleteNaturalPersonAsync(id));
    }
}