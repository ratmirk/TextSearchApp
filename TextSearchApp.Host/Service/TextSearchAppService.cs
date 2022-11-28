using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using TextSearchApp.Data;
using TextSearchApp.Data.Entities;
using TextSearchApp.Host.Common;

namespace TextSearchApp.Host.Service;

/// <summary>
/// Сервис TextSearch.
/// </summary>
public class TextSearchAppService
{
    private readonly TextSearchAppDbContext _dbContext;
    private readonly IElasticClient _elasticClient;
    private readonly string _index;
    private ILogger<TextSearchAppService> _logger;

    /// <summary>
    /// Конструктор TextSearchAppService.
    /// </summary>
    /// <param name="dbContext"> Контекст базы. </param>
    /// <param name="elasticClient"> Клиент IElasticClient. </param>
    /// <param name="configuration"> Конфигурация. </param>
    /// <param name="logger"> Logger. </param>
    public TextSearchAppService(TextSearchAppDbContext dbContext, IElasticClient elasticClient, IConfiguration configuration, ILogger<TextSearchAppService> logger)
    {
        _dbContext = dbContext.EnsureNotNull(nameof(dbContext));
        _elasticClient = elasticClient.EnsureNotNull(nameof(elasticClient));
        _index = configuration["ELKConfiguration:index"].EnsureNotNull(nameof(configuration));;
        _logger = logger.EnsureNotNull(nameof(logger));;
    }

    /// <summary>
    /// Поиск документов по тексту.
    /// </summary>
    /// <param name="text"> Текст. </param>
    public async Task<List<DocumentText>> SearchDocumentsByText(string text)
    {
        _logger.LogInformation("Поиск документа по тексту {Text}", text);
        var response = await _elasticClient.SearchAsync<DocumentText>(s => s
            .From(0)
            .Size(20)
            .Query(q => q
                .Term(t => t.Text, text)
            )
        );

        if (!response.IsValid)
        {
            _logger.LogError("Elastic вернул невалидный ответ по поиску: {Text}", text);
            return new List<DocumentText>();
        }

        var documentIds = response.Documents.Select(x => x.Id);

        return await _dbContext.Documents.Where(x => documentIds.Contains(x.Id)).OrderBy(x => x.CreatedDate).ToListAsync();

    }

    /// <summary>
    /// Удалить документ по идентификатору.
    /// </summary>
    /// <param name="id"> Идентификатор. </param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteDocumentAsync(long id)
    {
        _logger.LogInformation("Удаление документа id: {Id}", id);
        var document = await _dbContext.Documents.FindAsync(id);
        if (document != null)
        {
            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();
            await _elasticClient.DeleteAsync(new DeleteRequest(_index, document.Id));
        }
        else
        {
            _logger.LogError("Документ не найден в базе по id: {Id}", id);
            throw new KeyNotFoundException("Документ не найден в базе");
        }
    }

    /// <summary>
    /// Добавить документ.
    /// </summary>
    /// <param name="document"> Объект документа. </param>
    public async Task AddDocumentAsync(DocumentText document)
    {
        _logger.LogInformation("Добавление документа");
        var doc = await _dbContext.AddAsync(document);
        await _dbContext.SaveChangesAsync();

        var response = await _elasticClient.IndexDocumentAsync(doc.Entity);

        if (!response.IsValid)
        {
            _logger.LogError("Elastic вернул невалидный ответ по добавлению документа {@Document} в индекс:", doc.Entity);
        }

        _logger.LogInformation("Index document with id: {Id} succeeded", response.Id);
    }

    /// <summary>
    /// Получить документ по идентификатору.
    /// </summary>
    /// <param name="id"> Идентификатор. </param>
    public async Task<DocumentText> GetById(long id)
    {
        _logger.LogInformation("Получение документа по id");
        var response = await _elasticClient.GetAsync<DocumentText>(id, idx => idx.Index(_index));

        if (!response.IsValid)
        {
            _logger.LogError("Elastic вернул невалидный ответ по получению по id: {Id}", id);
            return null;
        }

        return await _dbContext.Documents.FindAsync(response.Source.Id);
    }
}