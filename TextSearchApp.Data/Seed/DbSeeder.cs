using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Data.Seed;

/// <summary>
/// Класс-помощник для импорта данных в базу из файла.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Заполнить базу.
    /// </summary>
    public static async Task SeedDb(
        TextSearchAppDbContext dbContext,
        IElasticClient elasticClient,
        IConfiguration configuration,
        ILogger logger)
    {
        logger.LogInformation("Start seeding DB");
        await dbContext.Database.EnsureCreatedAsync();

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

            await dbContext.Documents.AddAsync(new DocumentText
                {Text = text, CreatedDate = isParsed ? createdDate.ToUniversalTime() : null, Rubrics = rubrics});
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeding DB completed");
        IndexDocuments(configuration, dbContext, elasticClient, logger);
    }

    private static void IndexDocuments(IConfiguration configuration, TextSearchAppDbContext dbContext, IElasticClient elasticClient, ILogger logger)
    {
         bool.TryParse(configuration["SeedSettings:IsNeedToIndex"], out var isNeedToIndex);

         if (isNeedToIndex)
         {
             var sw = new Stopwatch();
             sw.Start();
             logger.LogInformation("Start indexing DB in ElasticSearch");

             elasticClient.BulkAll(dbContext.Documents.AsEnumerable(), b => b
                     .BackOffTime(new Time(TimeSpan.FromSeconds(5)))
                     .BackOffRetries(2)
                     .RefreshOnCompleted()
                     .MaxDegreeOfParallelism(4)
                     .Size(500)
                     .ContinueAfterDroppedDocuments()
                     .DroppedDocumentCallback((d, text) =>
                     {
                         if (d.Error != null)
                             logger.LogError("Unable to index document id: {Id} error: {Error}", text.Id, d.Error);

                     }))
                 .Wait(TimeSpan.FromMinutes(5),
                     next => { logger.LogInformation("Next {Count} documents indexed", next.Items.Count); });

             sw.Stop();
             logger.LogInformation("Indexing DB in ElasticSearch completed in {Time}", sw.Elapsed);
         }
    }
}