using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/forms", async (HttpRequest request, IWebHostEnvironment env) =>
{
    JsonDocument body;
    try
    {
        body = await JsonDocument.ParseAsync(request.Body);
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { ok = false, message = "Invalid JSON." });
    }

    using (body)
    {
        var root = body.RootElement;
        if (!root.TryGetProperty("kind", out var kindElement) ||
            kindElement.ValueKind != JsonValueKind.String)
            return Results.BadRequest(new { ok = false, message = "Missing form type." });

        var kind = kindElement.GetString();
        if (kind is not ("contact" or "application"))
            return Results.BadRequest(new { ok = false, message = "Unsupported form type." });

        if (!root.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Object)
            return Results.BadRequest(new { ok = false, message = "Missing form data." });

        string GetString(JsonElement obj, string name) =>
            obj.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()?.Trim() ?? "" : "";

        if (kind == "contact")
        {
            var name = GetString(data, "name");
            var email = GetString(data, "email");
            var message = GetString(data, "message");

            if (name.Length is < 2 or > 150 ||
                !Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$") ||
                message.Length is < 2 or > 10000)
                return Results.BadRequest(new { ok = false, message = "Please check the required fields." });
        }
        else
        {
            var fullName = GetString(data, "fullName");
            var email = GetString(data, "email");
            var consent = data.TryGetProperty("consent", out var c) && c.ValueKind == JsonValueKind.True;
            if (fullName.Length < 2 || !Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$") || !consent)
                return Results.BadRequest(new { ok = false, message = "Required application fields or consent are missing." });
        }

        // Demo storage: one JSON record per line. Configure protected storage/access before production use.
        var dataDirectory = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        var filePath = Path.Combine(dataDirectory, "submissions.jsonl");
        var record = new
        {
            id = Guid.NewGuid(),
            receivedAtUtc = DateTimeOffset.UtcNow,
            kind,
            data
        };
        var line = JsonSerializer.Serialize(record) + Environment.NewLine;
        await System.IO.File.AppendAllTextAsync(filePath, line, Encoding.UTF8);

        return Results.Ok(new { ok = true, message = "Submission received." });
    }
});

app.Run();
