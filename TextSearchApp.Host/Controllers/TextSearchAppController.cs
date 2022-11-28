using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TextSearchApp.Data.Entities;
using TextSearchApp.Host.Common;
using TextSearchApp.Host.Service;

namespace TextSearchApp.Host.Controllers;

/// <inheritdoc />
[ApiController]
[Route("api/text-search")]
[AppExceptionFilter]
public class TextSearchAppController : ControllerBase
{
    private readonly ITextSearchAppService _textSearchService;

    /// <inheritdoc />
    public TextSearchAppController(ITextSearchAppService textSearchService)
    {
        _textSearchService = textSearchService.EnsureNotNull(nameof(textSearchService));;
    }

    /// <summary>
    /// Поиск по тексту.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("search-documents")]
    public async Task<List<DocumentText>> SearchDocuments([FromQuery]string text)
    {
        text.EnsureNotNullOrWhiteSpace(nameof(text));
        return await _textSearchService.SearchDocumentsByTextAsync(text);
    }

    /// <summary>
    /// Получить по Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("get-document/{id}")]
    public async Task<DocumentText> GetDocument(long id)
    {
        id.EnsureNotNull(nameof(id));
        return await _textSearchService.GetById(id);
    }

    /// <summary>
    /// Добавить документ в базу.
    /// </summary>
    /// <param name="document"> Документ для добавления. </param>
    [HttpPost]
    [Route("add-document")]
    public async Task<IActionResult> AddDocument([FromBody]DocumentText document)
    {
        document.EnsureNotNull(nameof(document));
        document.Text.EnsureNotNullOrWhiteSpace(nameof(document.Text));

        await _textSearchService.AddDocumentAsync(document);
        return Ok(document);
    }

    /// <summary>
    /// Удалить документ из базы.
    /// </summary>
    /// <param name="id"> Id Документа. </param>
    [HttpDelete]
    [Route("delete-document/{id}")]
    public async Task<IActionResult> DeleteDocument(long id)
    {
        id.EnsureNotNull(nameof(id));
        await _textSearchService.DeleteDocumentAsync(id);

        return Ok();
    }
}