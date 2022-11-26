using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TextSearchApp.Data;
using TextSearchApp.Data.ElasticSearch;

namespace TextSearchApp.Host;

/// <summary>
/// Startup.
/// </summary>
public class Startup
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// New instance of Startup.
    /// </summary>
    /// <param name="configuration"> IConfiguration. </param>
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// ConfigureServices.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TextSearchApp",
                Version = "v1",
                Description = "Поисковик по текстам документов",
            });
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        services.AddScoped<TextSearchAppService>();

        //Db
        services.AddDbContext<TextSearchAppDbContext>(
            options => options.UseNpgsql(_configuration["ConnectionString"]));
        services.AddElasticsearch(_configuration);
    }

    /// <summary>
    /// Configure.
    /// </summary>
    /// <param name="app"> IApplicationBuilder. </param>
    /// <param name="env"> IWebHostEnvironment. </param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                opt.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}