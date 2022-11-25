using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TextSearchApp.Data;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Host
{
    public class TextSearchAppService
    {
        private TextSearchAppDbContext _dbContext;

        public TextSearchAppService(TextSearchAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<DocumentText> SearchDocumentsByText(string text)
        {
            return _dbContext.Documents.Where(x => x.Text.Contains(text)).OrderBy(x => x.CreatedDate).Take(20).ToList();
        }

        public async Task DeleteDocument(long id)
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
    }
}