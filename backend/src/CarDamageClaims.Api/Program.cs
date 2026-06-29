using System.Text;
using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.Localization;
using CarDamageClaims.Api.Models;
using CarDamageClaims.Api.Services;
using CarDamageClaims.Api.Services.AdminRequests;
using CarDamageClaims.Api.Services.Email;
using CarDamageClaims.Api.Services.ImageAnalysis;
using CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;
using CarDamageClaims.Api.Services.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
const long maxMultipartBodyBytes = 24 * 1024 * 1024;

var openAiApiKeyFromEnv = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (!string.IsNullOrWhiteSpace(openAiApiKeyFromEnv))
{
    builder.Configuration["OpenAi:ApiKey"] = openAiApiKeyFromEnv;
}

var openAiModelFromEnv = Environment.GetEnvironmentVariable("OPENAI_MODEL");
if (!string.IsNullOrWhiteSpace(openAiModelFromEnv))
{
    builder.Configuration["OpenAi:Model"] = openAiModelFromEnv;
}

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxMultipartBodyBytes;
});

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var lang = LanguageResolver.Resolve(context.HttpContext);

        var errors = context
            .ModelState.Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry =>
                    (entry.Value?.Errors ?? [])
                        .Select(_ =>
                            lang == AppLanguage.En ? "Invalid value." : "Некорректное значение."
                        )
                        .ToArray()
            );

        var payload = new
        {
            message = lang == AppLanguage.En ? "Validation failed." : "Ошибка валидации.",
            errors,
        };

        return new BadRequestObjectResult(payload);
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("Content-Disposition");
        }
    );
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAi"));
builder.Services.AddScoped<DamageAnalysisPrompts>();
builder.Services.AddScoped<RepairPricingPrompts>();
builder.Services.AddScoped<RepairPricingResponseReader>();
builder.Services.AddScoped<RepairCostMatcher>();
builder.Services.AddScoped<DamageInferenceService>();
builder.Services.AddScoped<DamagePartNormalizer>();
builder.Services.AddScoped<OpenAiResponsesClient>();
builder.Services.AddScoped<OpenAiResponsesPayload>();
builder.Services.AddScoped<OpenAiResponseContentReader>();
builder.Services.AddScoped<MockImageAnalysisService>();
builder.Services.AddScoped<OpenAiImageAnalysisService>();

builder.Services.AddScoped<IImageAnalysisService>(serviceProvider =>
{
    var aiOptions = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    return string.IsNullOrWhiteSpace(aiOptions.ApiKey)
        ? serviceProvider.GetRequiredService<MockImageAnalysisService>()
        : serviceProvider.GetRequiredService<OpenAiImageAnalysisService>();
});
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<MockEmailService>();
builder.Services.AddScoped<SmtpEmailService>();
builder.Services.AddScoped<ImageSizeReader>();
builder.Services.AddScoped<RequestDocxReportWriter>();
builder.Services.AddScoped<RequestListExcelWriter>();
builder.Services.AddScoped<AdminRequestPresenter>();
builder.Services.AddScoped<AdminRequestQueryService>();
builder.Services.AddScoped<AdminRequestUpdateService>();
builder.Services.AddScoped<AdminRequestExportService>();
builder.Services.AddScoped<AdminRequestDecisionService>();
builder.Services.AddScoped<AdminRequestReanalysisService>();
builder.Services.AddScoped<UploadedFileValidator>();
builder.Services.AddScoped<RequestPhotoStorageService>();
builder.Services.AddScoped<RequestSubmissionService>();

builder.Services.AddScoped<IEmailService>(serviceProvider =>
{
    var emailOptions = serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;
    var isSmtpConfigured = IsSmtpConfigured(emailOptions);

    return isSmtpConfigured
        ? serviceProvider.GetRequiredService<SmtpEmailService>()
        : serviceProvider.GetRequiredService<MockEmailService>();
});

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];

if (
    string.IsNullOrWhiteSpace(jwtIssuer)
    || string.IsNullOrWhiteSpace(jwtAudience)
    || string.IsNullOrWhiteSpace(jwtKey)
)
{
    throw new InvalidOperationException("JWT configuration is missing.");
}

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrManager", policy => policy.RequireRole("Admin", "Manager"));
});

var app = builder.Build();

await EnsureDatabaseAndSeedAsync(app);

var resolvedAiOptions = app.Services.GetRequiredService<IOptions<OpenAiOptions>>().Value;
if (string.IsNullOrWhiteSpace(resolvedAiOptions.ApiKey))
{
    app.Logger.LogWarning("OpenAI API key is not configured. Image analysis uses mock service.");
}

var resolvedEmailOptions = app.Services.GetRequiredService<IOptions<EmailOptions>>().Value;
var isSmtpConfigured = IsSmtpConfigured(resolvedEmailOptions);

if (!isSmtpConfigured)
{
    app.Logger.LogWarning("SMTP is not configured. Email sending uses mock service.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

var storageDirectory = Path.Combine(app.Environment.ContentRootPath, "storage");
Directory.CreateDirectory(storageDirectory);
app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(storageDirectory),
        RequestPath = "/storage",
    }
);

app.UseCors("Frontend");

app.Use(
    async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (InvalidDataException ex)
            when (ex.Message.Contains(
                    "Multipart body length limit",
                    StringComparison.OrdinalIgnoreCase
                )
            )
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var lang = LanguageResolver.Resolve(context);
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                new
                {
                    message = lang == AppLanguage.En
                        ? "Uploaded data exceeds the allowed multipart size limit."
                        : "Размер загружаемых данных превышает допустимый лимит multipart-запроса.",
                }
            );
        }
        catch (BadHttpRequestException ex)
            when (ex.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var lang = LanguageResolver.Resolve(context);
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                new
                {
                    message = lang == AppLanguage.En
                        ? "Request body is too large."
                        : "Тело запроса слишком большое.",
                }
            );
        }
    }
);

app.UseAuthentication();

app.Use(
    async (context, next) =>
    {
        await next();

        if (
            context.Response.StatusCode == StatusCodes.Status401Unauthorized
            && !context.Response.HasStarted
        )
        {
            var lang = LanguageResolver.Resolve(context);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                new { message = LocalizedMessages.Unauthorized(lang) }
            );
        }
    }
);

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task EnsureDatabaseAndSeedAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.MigrateAsync();

    const string defaultAdminEmail = "admin@example.com";
    const string defaultAdminPassword = "123";

    var existingAdmin = await dbContext.Users.AnyAsync(x => x.Email == defaultAdminEmail);
    if (existingAdmin)
    {
        return;
    }

    var now = DateTime.UtcNow;
    dbContext.Users.Add(
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            MiddleName = null,
            Email = defaultAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultAdminPassword),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        }
    );

    await dbContext.SaveChangesAsync();
}

static bool IsSmtpConfigured(EmailOptions options)
{
    return !string.IsNullOrWhiteSpace(options.Host)
        && options.Port > 0
        && !string.IsNullOrWhiteSpace(options.Username)
        && !string.IsNullOrWhiteSpace(options.Password);
}
