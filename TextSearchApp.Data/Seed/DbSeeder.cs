using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Nest;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedDb(TextSearchAppDbContext dbContext, IElasticClient elasticClient, IConfiguration configuration)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = bool.Parse(configuration["SeedSettings:HasHeaderRecord"]),
            Delimiter = configuration["SeedSettings:CsvDelimiter"]
        };

        var fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + configuration["SeedSettings:File"];
        using var streamReader = File.OpenText(fileName);
        using var csvReader = new CsvReader(streamReader, csvConfig);

        if (csvConfig.HasHeaderRecord)
        {
            _ = await csvReader.ReadAsync();
        }

        while (await csvReader.ReadAsync())
        {
            var text = csvReader.GetField(0);
            var createdDateText = csvReader.GetField(1);
            var isParsed = DateTime.TryParse(createdDateText, out var createdDate);
            var rubrics = csvReader.GetField(2)?.Split(",").Select(x => Regex.Match(x, "'(.*)'").Groups[1].Value).ToArray();

            var doc = await dbContext.Documents.AddAsync(new DocumentText
                {Text = text, CreatedDate = isParsed ? createdDate.ToUniversalTime() : null, Rubrics = rubrics});

            await elasticClient.IndexDocumentAsync(doc.Entity);
        }

        await dbContext.SaveChangesAsync();
    }
}