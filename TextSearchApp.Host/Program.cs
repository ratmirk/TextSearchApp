using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TextSearchApp.Data;
using TextSearchApp.Data.Seed;

namespace TextSearchApp.Host;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunWithSeed(args);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    private static async Task RunWithSeed(this IHost host, string[] args)
    {
        if (args.Length > 0 && args[0].Equals("seed", StringComparison.InvariantCultureIgnoreCase))
        {
            using var scope = host.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TextSearchAppDbContext>();
            await DbSeeder.SeedDb(context);
            await host.RunAsync();
        }
        else
        {
            await host.RunAsync();
        }
    }
}