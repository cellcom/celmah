using Celmah;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Host.UseCelmah(elmah => elmah.UseCelmahExceptionPage());

        var app = builder.Build();

        app.UseExceptionHandler("/Error");
        app.UseCelmahMiddleware();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        app.UseRouting();

        app.MapRazorPages();
        app.MapCelmah();

        app.Run();
    }
}