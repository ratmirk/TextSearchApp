using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Host.Controllers;

/// <inheritdoc />
[ApiController]
[Route("api/text-search")]
public class TextSearchAppController : ControllerBase
{
    private readonly TextSearchAppService _textSearchService;

    /// <inheritdoc />
    public TextSearchAppController(TextSearchAppService textSearchService)
    {
        _textSearchService = textSearchService;
    }

    /// <summary>
    /// Поиск по тексту.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("search-documents")]
    public async Task<List<DocumentText>> SearchDocuments(string text)
    {
        return await _textSearchService.SearchDocumentsByText(text);
    }

    /// <summary>
    /// Получить по Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("get-document")]
    public async Task<DocumentText> SearchDocuments(long id)
    {
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
        await _textSearchService.AddDocumentAsync(document);
        return Ok(document);
    }

    /// <summary>
    /// Удалить документ из базы.
    /// </summary>
    /// <param name="id"> Id Документа. </param>
    [HttpDelete]
    [Route("delete-document")]
    public async Task<IActionResult> DeleteDocument(long id)
    {
        try
        {
            await _textSearchService.DeleteDocumentAsync(id);
        }
        catch (KeyNotFoundException e)
        {
            return BadRequest("Такой документ не найден");
        }
        catch (Exception e)
        {
            return Problem("Неизвестная ошибка");
        }

        return Ok();
    }
}