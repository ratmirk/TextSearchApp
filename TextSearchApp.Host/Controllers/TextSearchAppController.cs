using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Host.Controllers;

[ApiController]
[Route("api/text-search")]
public class TextSearchAppController : ControllerBase
{
    private readonly TextSearchAppService _textSearchService;

    public TextSearchAppController(TextSearchAppService textSearchService)
    {
        _textSearchService = textSearchService;
    }

    [HttpGet]
    [Route("search-documents")]
    public List<DocumentText> SearchDocuments(string text)
    {
        return _textSearchService.SearchDocumentsByText(text);
    }

    [HttpDelete]
    [Route("delete-document")]
    public async Task<IActionResult> DeleteDocument(long id)
    {
        try
        {
            await _textSearchService.DeleteDocument(id);
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