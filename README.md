# Campus Étudiant — ASP.NET Core version

This project preserves the uploaded website's HTML, CSS, visual styling, page content and existing browser-side interactions. ASP.NET Core/C# adds a backend endpoint for the Contact and Student Application forms.

## Requirements
- .NET 8 SDK (or change the target framework in the `.csproj` if using a newer supported SDK)

## Run locally
1. Open a terminal in this folder.
2. Run `dotnet restore`
3. Run `dotnet run`
4. Open the local URL printed by ASP.NET Core.

## Project structure
- `wwwroot/index.html` — original website, including its embedded CSS and JavaScript.
- `Program.cs` — C# backend API at `POST /api/forms`.
- `App_Data/submissions.jsonl` — created automatically when a form is submitted.

## Important notes
- The browser-side JavaScript remains because the original site uses it for page routing, menus, form steps, and validation. HTML and CSS alone cannot provide those behaviors; C# handles server-side submissions.
- This is a starter implementation. Submissions contain personal information and are written to a local file. Before publishing, configure appropriate access controls, retention/deletion procedures, HTTPS, anti-spam/rate limiting, and secure database storage. Do not expose `App_Data` as a static web directory.
- Configure the actual contact email/social links in `CONFIG` inside `wwwroot/index.html`.
