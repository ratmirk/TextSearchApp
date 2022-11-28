using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TextSearchApp.Data.Entities;
using TextSearchApp.Host.Controllers;
using TextSearchApp.Host.Service;
using TextSearchApp.Tests.Common;

namespace TextSearchApp.Tests;

public class TextSearchAppControllerTests
{
    [Test]
    public async Task SearchDocumentsTest_WithValidRequest_ShouldReturnResult()
    {
        // Arrange
        var expectedResult = new List<DocumentText>
        {
            new() {Id = Generator.FakerRu.Random.Long(), Text = Generator.FakerRu.Lorem.Text()},
            new() {Id = Generator.FakerRu.Random.Long(), Text = Generator.FakerRu.Lorem.Text()},
        };
        var service = new Mock<ITextSearchAppService>();
        service.Setup(x => x.SearchDocumentsByTextAsync(It.IsAny<string>())).Returns(Task.FromResult(expectedResult));
        var controller = new TextSearchAppController(service.Object);

        // Act
        var result = await controller.SearchDocuments("test");

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task SearchDocumentsTest_WithNullText_ShouldThrow()
    {
        // Arrange
        var service = new Mock<ITextSearchAppService>();
        var controller = new TextSearchAppController(service.Object);

        // Act & Assert
        var act = () => controller.SearchDocuments(null);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [TestCase("")]
    [TestCase(" ")]
    public async Task SearchDocumentsTest_WithEmptyText_ShouldThrow(string text)
    {
        // Arrange
        var service = new Mock<ITextSearchAppService>();
        var controller = new TextSearchAppController(service.Object);

        // Act & Assert
        var act = () => controller.SearchDocuments(text);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task DeleteDocumentTest_WithExistingId_ShouldReturnOk()
    {
        // Arrange
        var service = new Mock<ITextSearchAppService>();
        service.Setup(x => x.DeleteDocumentAsync(It.IsAny<long>())).Returns(Task.CompletedTask);
        var controller = new TextSearchAppController(service.Object);

        // Act
        var result = await controller.DeleteDocument(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task DeleteDocumentTest_WithNotExistingId_ShouldThrow()
    {
        // Arrange
        var service = new Mock<ITextSearchAppService>();
        var controller = new TextSearchAppController(service.Object);
        service.Setup(x => x.DeleteDocumentAsync(It.IsAny<long>())).Throws<KeyNotFoundException>();

        // Act & Assert
        var act = () => controller.DeleteDocument(1);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}