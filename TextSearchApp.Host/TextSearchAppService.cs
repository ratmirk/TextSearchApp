using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using TextSearchApp.Data;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Host;

public class TextSearchAppService
{
    private readonly TextSearchAppDbContext _dbContext;
    private readonly IElasticClient _elasticClient;

    public TextSearchAppService(TextSearchAppDbContext dbContext, IElasticClient elasticClient)
    {
        _dbContext = dbContext;
    }

    public List<DocumentText> SearchDocumentsByText(string text)
    {
        return _dbContext.Documents.Where(x => x.Text.Contains(text)).OrderBy(x => x.CreatedDate).Take(20).ToList();
    }

    public async Task DeleteDocumentAsync(long id)
    {
        var document = await _dbContext.Documents.FindAsync(id);
        if (document != null)
        {
            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException();
        }
    }

    public async Task AddDocumentAsync(DocumentText document)
    {
        await _dbContext.AddAsync(document);
    }
}