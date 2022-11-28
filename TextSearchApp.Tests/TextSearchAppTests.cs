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
using TextSearchApp.Host.Service;

namespace TextSearchApp.Tests;

public class TextSearchAppServiceTests
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
        var rnd = new Random();
        var expectedResult = new List<DocumentText>
        {
            new() {Id = rnd.Next(), Text = "test"},
            new() {Id = rnd.Next(), Text = "test"},
        };
        await _dbConext.AddRangeAsync(expectedResult);
        await _dbConext.SaveChangesAsync();

        var response = new Mock<ISearchResponse<DocumentText>>();
        response.SetupGet(x => x.IsValid).Returns(isValid);
        response.SetupGet(x => x.Documents).Returns(expectedResult);
        _elasticMock.Setup(x =>
            x.SearchAsync(It.IsAny<Func<SearchDescriptor<DocumentText>, ISearchRequest>>(),
                It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));

        var service = new TextSearchAppService(_dbConext, _elasticMock.Object, _configurationMock.Object, _loggerMock);
        var result = await service.SearchDocumentsByTextAsync("test");
        _ = isValid ? result.Should().BeEquivalentTo(expectedResult) : result.Should().BeEmpty();
    }
}