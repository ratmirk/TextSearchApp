using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextSearchApp.Host
{
    public class TextSearchAppService
    {
        public async Task<string> SearchDocumentsByText(string text)
        {
            await Task.Delay(1);
            return "Search Test";
        }

        public async Task DeleteDocument(string id)
        {
            await Task.Delay(1);
            if (id == "1")
            {
                return;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }
    }
}