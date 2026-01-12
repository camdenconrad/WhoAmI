using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WhoAmI.ViewModels;

namespace WhoAmI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddScoped<MainViewModel>();

        // Verify test data loads correctly
        try
        {
            var traits = Data.TestData.GetTraits();
            var vignettes = Data.TestData.GetVignettes();
            Console.WriteLine($"Loaded {traits.Length} traits and {vignettes.Length} vignettes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading test data: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // Only use HTTPS redirect in development with certificates
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        app.Run($"http://0.0.0.0:{port}");
    }
}
