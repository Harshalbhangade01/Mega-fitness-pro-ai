using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

public class AIController : Controller
{
    private readonly AppDbContext _c;
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public AIController(AppDbContext c, IConfiguration config, IHttpClientFactory httpFactory)
    {
        _c = c;
        _config = config;
        _http = httpFactory.CreateClient();
    }

    private int? Uid => HttpContext.Session.GetInt32("uid");

    public IActionResult Tips()
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        ViewBag.UserName = HttpContext.Session.GetString("uname");
        ViewBag.LatestProgress = _c.ProgressRecords
            .Where(p => p.UserId == uid)
            .OrderByDescending(p => p.RecordDate)
            .FirstOrDefault();
        ViewBag.AttendanceCount = _c.Attendances.Count(a => a.UserId == uid);
        ViewBag.MyPlans = _c.UserPlans.Where(x => x.UserId == uid).ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] FitBotRequest req)
    {
        if (Uid == null) return Unauthorized();

        var apiKey = _config["Groq:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            return BadRequest(new { error = "API key not configured. Please set Groq:ApiKey in appsettings.json" });

        // Groq uses OpenAI-compatible format
        var payload = new
        {
            model = "llama-3.3-70b-versatile",
            max_tokens = 1000,
            messages = new[]
            {
                new { role = "system", content = req.SystemPrompt },
                new { role = "user", content = req.UserMessage }
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

        var response = await _http.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, new { error = "Groq API error: " + responseBody });

        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return Ok(new { reply = text });
    }
}

public class FitBotRequest
{
    public string UserMessage { get; set; } = "";
    public string SystemPrompt { get; set; } = "";
}
