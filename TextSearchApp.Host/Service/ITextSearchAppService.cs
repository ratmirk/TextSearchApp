using System.Collections.Generic;
using System.Threading.Tasks;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Host.Service;

/// <summary>
/// Интерфейс сервиса TextSearchAppService
/// </summary>
public interface ITextSearchAppService
{
    /// <summary>
    /// Поиск документов по тексту.
    /// </summary>
    /// <param name="text"> Текст. </param>
    Task<List<DocumentText>> SearchDocumentsByTextAsync(string text);

    /// <summary>
    /// Удалить документ по идентификатору.
    /// </summary>
    /// <param name="id"> Идентификатор. </param>
    /// <exception cref="KeyNotFoundException"></exception>
    Task DeleteDocumentAsync(long id);

    /// <summary>
    /// Добавить документ.
    /// </summary>
    /// <param name="document"> Объект документа. </param>
    Task AddDocumentAsync(DocumentText document);

    /// <summary>
    /// Получить документ по идентификатору.
    /// </summary>
    /// <param name="id"> Идентификатор. </param>
    Task<DocumentText> GetById(long id);
}