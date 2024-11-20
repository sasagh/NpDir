using System.Linq.Expressions;
using Microsoft.Extensions.Localization;
using Moq;
using NpDirectory.Application;
using NpDirectory.Application.Exceptions;
using NpDirectory.Application.Requests;
using NpDirectory.Application.Services;
using NpDirectory.Domain.Enum;
using NpDirectory.Domain.Models;

namespace NpDirectory.UnitTests;

public class RelationServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IStringLocalizer> _localizerMock;
    private readonly IRelationService _service;

    public RelationServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _localizerMock = new Mock<IStringLocalizer>();
        _service = new RelationService(_uowMock.Object, _localizerMock.Object);

        _localizerMock.Setup(x => x["Error.NaturalPerson.NotFound"])
            .Returns(new LocalizedString("Error.NaturalPerson.NotFound", "Person not found"));
        _localizerMock.Setup(x => x["Error.Relation.AlreadyExists"])
            .Returns(new LocalizedString("Error.Relation.AlreadyExists", "Relation exists"));
        _localizerMock.Setup(x => x["Error.Relation.NotFound"])
            .Returns(new LocalizedString("Error.Relation.NotFound", "Relation not found"));
    }

    [Fact]
    public async Task CreateRelationAsync_WhenBothPersonsExistAndRelationDoesNot_CreatesRelation()
    {
        // Arrange
        var request = new CreateRelationRequest
        {
            FromId = 1,
            ToId = 2,
            RelationType = RelationType.Acquaintance
        };

        _uowMock.Setup(x => x.NaturalPersonsRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<NaturalPerson, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new NaturalPerson());
        
        _uowMock.Setup(x => x.RelationsesRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<Relation, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync((Relation)null);

        // Act
        await _service.CreateRelationAsync(request);

        // Assert
        _uowMock.Verify(x => x.RelationsesRepository.CreateAsync(It.Is<Relation>(r =>
            r.NaturalPersonId == request.FromId &&
            r.RelatedPersonId == request.ToId &&
            r.Type == request.RelationType)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateRelationAsync_WhenFirstPersonNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new CreateRelationRequest { FromId = 1, ToId = 2 };
        
        _uowMock.Setup(x => x.NaturalPersonsRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<NaturalPerson, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync((NaturalPerson)null);

        // Act
        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateRelationAsync(request));
        Assert.Contains("1", exception.Message);
    }

    [Fact]
    public async Task CreateRelationAsync_WhenSecondPersonNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new CreateRelationRequest { FromId = 1, ToId = 2 };
        var setupSequence = new List<NaturalPerson> { new NaturalPerson(), null };
        var sequenceIndex = 0;
        
        _uowMock.Setup(x => x.NaturalPersonsRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<NaturalPerson, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(() => setupSequence[sequenceIndex++]);

        // Act
        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateRelationAsync(request));
        Assert.Contains("2", exception.Message);
    }

    [Fact]
    public async Task CreateRelationAsync_WhenRelationExists_ThrowsRelationExistsException()
    {
        // Arrange
        var request = new CreateRelationRequest { FromId = 1, ToId = 2 };
        
        _uowMock.Setup(x => x.NaturalPersonsRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<NaturalPerson, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new NaturalPerson());
        
        _uowMock.Setup(x => x.RelationsesRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<Relation, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new Relation());

        // Act
        // Assert
        await Assert.ThrowsAsync<RelationExistsException>(() =>
            _service.CreateRelationAsync(request));
    }

    [Fact]
    public async Task DeleteRelationAsync_WhenRelationExists_DeletesRelation()
    {
        // Arrange
        var relationId = 1;
        _uowMock.Setup(x => x.RelationsesRepository.DeleteSingleAsync(relationId))
            .ReturnsAsync(true);

        // Act
        await _service.DeleteRelationAsync(relationId);

        // Assert
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteRelationAsync_WhenRelationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var relationId = 1;
        _uowMock.Setup(x => x.RelationsesRepository.DeleteSingleAsync(relationId))
            .ReturnsAsync(false);

        // Act
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteRelationAsync(relationId));
    }

    [Fact]
    public async Task DeleteRelationAsync_ByIds_WhenRelationExists_DeletesRelation()
    {
        // Arrange
        var fromId = 1;
        var toId = 2;
        var relation = new Relation { Id = 3 };

        _uowMock.Setup(x => x.RelationsesRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<Relation, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(relation);
        
        _uowMock.Setup(x => x.RelationsesRepository.DeleteSingleAsync(relation.Id))
            .ReturnsAsync(true);

        // Act
        await _service.DeleteRelationAsync(fromId, toId);

        // Assert
        _uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteRelationAsync_ByIds_WhenRelationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var fromId = 1;
        var toId = 2;

        _uowMock.Setup(x => x.RelationsesRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<Relation, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync((Relation)null);

        // Act
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteRelationAsync(fromId, toId));
    }

    [Fact]
    public async Task DeleteRelationAsync_ByIds_WhenRelationFoundButDeleteFails_ThrowsNotFoundException()
    {
        // Arrange
        var fromId = 1;
        var toId = 2;
        var relation = new Relation { Id = 3 };

        _uowMock.Setup(x => x.RelationsesRepository.GetOneByFilterAsync(It.IsAny<Expression<Func<Relation, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(relation);
        
        _uowMock.Setup(x => x.RelationsesRepository.DeleteSingleAsync(relation.Id))
            .ReturnsAsync(false);

        // Act
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteRelationAsync(fromId, toId));
    }
}