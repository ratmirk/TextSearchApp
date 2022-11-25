using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedDb(TextSearchAppDbContext dbContext)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        };

        var fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Seed\\posts.csv";
        using var streamReader = File.OpenText(fileName);
        using var csvReader = new CsvReader(streamReader, csvConfig);

        while (await csvReader.ReadAsync())
        {
            var text = csvReader.GetField(0);
            var createdDateText = csvReader.GetField(1);
            var isParsed = DateTime.TryParse(createdDateText, out var createdDate);
            var rubrics = csvReader.GetField(2)?.Split(",").Select(x => Regex.Match(x, "'.*'").Value).ToArray();

            await dbContext.Documents.AddAsync(new DocumentText
                {Text = text, CreatedDate = isParsed ? createdDate : null, Rubrics = rubrics});
        }

        await dbContext.SaveChangesAsync();
    }
}