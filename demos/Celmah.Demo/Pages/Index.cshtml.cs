using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Celmah.Demo.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        _logger.LogTrace("Test");
        _logger.LogDebug("Test");
        _logger.LogError("Test");
        _logger.LogInformation("Test");
        _logger.LogWarning("Test");
        _logger.LogCritical(new InvalidOperationException("Test"), "Test");

        await this.HttpContext.RaiseErrorAsync(new Exception("test2"));

        var r = 0;
        _ = 100 / r;
    }
}
