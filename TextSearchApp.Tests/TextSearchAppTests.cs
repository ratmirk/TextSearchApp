using System;
using System.Collections.Generic;
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
using TextSearchApp.Host;
using TextSearchApp.Host.Controllers;

namespace TextSearchApp.Tests;

public class TextSearchAppControllerTests
{
    private TextSearchAppDbContext _dbConext;
    private Mock<IElasticClient> _elasticMock;
    private Mock<IConfigurationRoot> _configurationMock;
    private ILogger<TextSearchAppService> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _dbConext = new TextSearchAppDbContext(new DbContextOptionsBuilder().UseInMemoryDatabase("test").Options);
        _elasticMock = new Mock<IElasticClient>();
        _loggerMock = Mock.Of<ILogger<TextSearchAppService>>();
        _configurationMock = new Mock<IConfigurationRoot>();

        _configurationMock.Setup<IConfiguration>(c => c.GetSection("ELKConfiguration"))
            .Returns(new ConfigurationSection(_configurationMock.Object, "ELKConfiguration"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SearchDocumentsTest(bool isValid)
    {
        var expectedResult = new List<DocumentText>
        {
            new() {Id = 1, Text = "test"},
            new() {Id = 2, Text = "test"},
        };
        var response = new Mock<ISearchResponse<DocumentText>>();
        response.SetupGet(x => x.IsValid).Returns(isValid);
        response.SetupGet(x => x.Documents).Returns(expectedResult);
        _elasticMock.Setup(x =>
            x.SearchAsync<DocumentText>(It.IsAny<Func<SearchDescriptor<DocumentText>, ISearchRequest>>(),
                It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));

        var service = new TextSearchAppService(_dbConext, _elasticMock.Object, _configurationMock.Object, _loggerMock);
        var controller = new TextSearchAppController(service);
        var result = await controller.SearchDocuments("test");
        _ = isValid ? result.Should().BeEquivalentTo(expectedResult) : result.Should().BeEmpty();
    }
}