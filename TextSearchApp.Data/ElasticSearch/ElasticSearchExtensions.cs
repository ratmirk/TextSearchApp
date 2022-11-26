﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Data.ElasticSearch;

public static class ElasticSearchExtensions
{
    public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["ELKConfiguration:Uri"];
        var defaultIndex = configuration["ELKConfiguration:index"];

        var settings = new ConnectionSettings(new Uri(url))
            .PrettyJson()
            .DefaultIndex(defaultIndex);

        var client = new ElasticClient(settings);

        services.AddSingleton<IElasticClient>(client);

        CreateIndex(client, defaultIndex);
    }

    private static void AddDefaultMappings(ConnectionSettings settings)
    {
        settings
            .DefaultMappingFor<DocumentText>(m => m);
    }

    private static void CreateIndex(IElasticClient client, string indexName)
    {
        var createIndexResponse = client.Indices.Create(indexName,
            index => index.Map<DocumentText>(x => x.AutoMap())
        );
    }
}