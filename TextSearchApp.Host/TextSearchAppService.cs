using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using TextSearchApp.Data;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Host;

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
    /// Конструктор TextSearchAppService
    /// </summary>
    /// <param name="dbContext"> Контекст базы. </param>
    /// <param name="elasticClient"> Клиент IElasticClient. </param>
    /// <param name="configuration"> Конфигурация. </param>
    /// <param name="logger"> Logger. </param>
    public TextSearchAppService(TextSearchAppDbContext dbContext, IElasticClient elasticClient, IConfiguration configuration, ILogger<TextSearchAppService> logger)
    {
        _dbContext = dbContext;
        _elasticClient = elasticClient;
        _index = configuration["ELKConfiguration:index"];
        _logger = logger;
    }

    /// <summary>
    /// Поиск документов по тексту
    /// </summary>
    /// <param name="text"> Текст. </param>
    public async Task<List<DocumentText>> SearchDocumentsByText(string text)
    {
        var response = await _elasticClient.SearchAsync<DocumentText>(s => s
            .From(0)
            .Size(20)
            .Sort(ss => ss.Ascending(p => p.CreatedDate))
            .Query(q => q
                .Term(t => t.Text, text)
            )
        );

        if (response.IsValid)
        {
            return response.Documents.ToList();
        }

        return _dbContext.Documents.Where(x => x.Text.Contains(text)).OrderBy(x => x.CreatedDate).Take(20).ToList();
    }

    /// <summary>
    /// Удалить документ по идентификатору.
    /// </summary>
    /// <param name="id"> Идентификатор. </param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteDocumentAsync(long id)
    {
        var document = await _dbContext.Documents.FindAsync(id);
        if (document != null)
        {
            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();
            await _elasticClient.DeleteAsync(new DeleteRequest(_index, document.Id));
        }
        else
        {
            throw new KeyNotFoundException();
        }
    }

    /// <summary>
    /// Добавить документ.
    /// </summary>
    /// <param name="document"> Объект документа. </param>
    public async Task AddDocumentAsync(DocumentText document)
    {
        var doc = await _dbContext.AddAsync(document);
        await _dbContext.SaveChangesAsync();

        var response = await _elasticClient.IndexDocumentAsync(doc.Entity);

        if (response.IsValid)
        {
            _logger.LogInformation("Index document with {Id} succeeded", response.Id);
        }
    }

    /// <summary>
    /// Получить документ по идентификатору.
    /// </summary>
    /// <param name="id"> Идентификатор. </param>
    public async Task<DocumentText> GetById(long id)
    {
        var response = await _elasticClient.GetAsync<DocumentText>(id, idx => idx.Index(_index));

        return response.IsValid ? response.Source : null;
    }
}