using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using NUnit.Framework;
using TextSearchApp.Data;
using TextSearchApp.Data.Entities;
using TextSearchApp.Host.Service;
using TextSearchApp.Tests.Common;

namespace TextSearchApp.Tests;

public class TextSearchAppServiceTests
{
    private TextSearchAppDbContext _dbConext;
    private Mock<IConfigurationRoot> _configurationMock;
    private ILogger<TextSearchAppService> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _dbConext = new TextSearchAppDbContext(new DbContextOptionsBuilder().UseInMemoryDatabase("test").Options);
        _loggerMock = Mock.Of<ILogger<TextSearchAppService>>();
        _configurationMock = new Mock<IConfigurationRoot>();

        _configurationMock.Setup(_ => _["ELKConfiguration:index"]).Returns("test");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SearchDocumentsTest(bool isValid)
    {
        // Arrange
        var expectedResult = new List<DocumentText>
        {
            new() {Id = Generator.FakerRu.Random.Long(), Text = Generator.FakerRu.Lorem.Text()},
            new() {Id = Generator.FakerRu.Random.Long(), Text = Generator.FakerRu.Lorem.Text()},
        };
        await _dbConext.AddRangeAsync(expectedResult);
        await _dbConext.SaveChangesAsync();
        var elasticMock = new Mock<IElasticClient>();

        var response = new Mock<ISearchResponse<DocumentText>>();
        response.SetupGet(x => x.IsValid).Returns(isValid);
        response.SetupGet(x => x.Documents).Returns(expectedResult);
        elasticMock.Setup(x =>
            x.SearchAsync(It.IsAny<Func<SearchDescriptor<DocumentText>, ISearchRequest>>(),
                It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));

        // Act
        var service = new TextSearchAppService(_dbConext, elasticMock.Object, _configurationMock.Object, _loggerMock);
        var result = await service.SearchDocumentsByTextAsync("test");

        // Assert
        _ = isValid ? result.Should().BeEquivalentTo(expectedResult) : result.Should().BeEmpty();
    }


    [Test]
    public async Task DeleteDocumentsTest()
    {
        // Arrange
        var entities = new List<DocumentText>
        {
            new() {Id = Generator.FakerRu.Random.Long(), Text = Generator.FakerRu.Lorem.Text()},
        };
        await _dbConext.AddRangeAsync(entities);
        await _dbConext.SaveChangesAsync();
        var elasticMock = new Mock<IElasticClient>();

        var response = new Mock<DeleteResponse>();
        response.SetupGet(x => x.IsValid).Returns(true);
        elasticMock.Setup(x =>
                x.DeleteAsync(It.IsAny<IDeleteRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(response.Object));

        // Act
        var service = new TextSearchAppService(_dbConext, elasticMock.Object, _configurationMock.Object, _loggerMock);
        await service.DeleteDocumentAsync(entities.First().Id);

        // Assert
        var foundEntity = await _dbConext.Documents.FindAsync(entities.First().Id);
        foundEntity.Should().BeNull();
        elasticMock.Invocations.Count.Should().Be(1);
    }
}