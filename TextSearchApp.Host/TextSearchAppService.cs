using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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

    /// <summary>
    /// Конструктор TextSearchAppServiceю
    /// </summary>
    /// <param name="dbContext"> Контекст базы. </param>
    /// <param name="elasticClient"> Клиент IElasticClient. </param>
    /// <param name="configuration"> Конфигурация. </param>
    public TextSearchAppService(TextSearchAppDbContext dbContext, IElasticClient elasticClient, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _elasticClient = elasticClient;
        _index = configuration["ELKConfiguration:index"];
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
            Console.WriteLine($"Index document with ID {response.Id} succeeded.");
        }
    }

    /// <summary>
    /// Получить документ по идентификатору.
    /// </summary>
    /// <param name="id"> Идентификатор. </param>
    public async Task<DocumentText> GetById(long id)
    {
        // var response = await _elasticClient.SearchAsync<DocumentText>(s => s
        //     .Query(q => q
        //         .Term(t => t.Id, id)
        //     )
        // );

        var response = await _elasticClient.GetAsync<DocumentText>(id, idx => idx.Index(_index));

        return response.IsValid ? response.Source : null;
    }
}