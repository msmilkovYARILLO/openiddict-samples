using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<DbContext>(options =>
    {
        options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-mimban-client.sqlite3")}");
        options.UseOpenIddict();
    })
    .AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the Entity Framework Core stores and models.
        // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
        options.UseEntityFrameworkCore()
            .UseDbContext<DbContext>();
    })

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Note: this sample uses the authorization code flow,
        // but you can enable the other flows if necessary.
        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        // Add the operating system integration.
        options.UseSystemIntegration();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
            .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration matching the client application definition in the server project.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44383/", UriKind.Absolute),

            ClientId = "console_app",
            RedirectUri = new Uri("/", UriKind.Relative)
        });
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    });

builder.Services
    .AddAuthorization();

var app = builder.Build();

app.UseAuthentication()
    .UseAuthorization();

app.MapGet("/test", () => Results.Challenge(null, [OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme]));

app.Run();
