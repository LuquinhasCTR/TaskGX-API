using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using TaskGX.API.Repositories;
using TaskGX.API.Services;
using TaskGX.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TaskGXContext>(options => options.UseMySQL(connectionString));

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations();

builder.Services.AddProblemDetails();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var validationProblem = new ValidationProblemDetails(context.ModelState)
        {
            Title = "A requisicao contem dados invalidos.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        return new BadRequestObjectResult(validationProblem);
    };
});

builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<EmailChangeService>();
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings not found.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Cole apenas o token JWT.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionFeature?.Error;
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");

        if (exception != null)
        {
            logger.LogError(
                exception,
                "Erro nao tratado em {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Ocorreu um erro interno ao processar a requisicao."
        };

        if (app.Environment.IsDevelopment() && exception != null)
        {
            problem.Detail = exception.Message;
            problem.Extensions["exceptionType"] = exception.GetType().FullName;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapGet("/swagger/v1/swagger-fixed.json", (ISwaggerProvider swaggerProvider) =>
    {
        var swaggerDocument = swaggerProvider.GetSwagger("v1");
        using var stringWriter = new StringWriter();
        var openApiWriter = new OpenApiJsonWriter(stringWriter);

        swaggerDocument.SerializeAsV3(openApiWriter);

        var swaggerJson = FixSwaggerSecurityRequirements(stringWriter.ToString());
        return Results.Text(swaggerJson, "application/json");
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger-fixed.json", "TaskGX v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    name = "TaskGX API",
    status = "ok",
    environment = app.Environment.EnvironmentName
}));

app.MapControllers();
app.Run();

static string FixSwaggerSecurityRequirements(string swaggerJson)
{
    return Regex.Replace(
        swaggerJson,
        "\"security\"\\s*:\\s*\\[\\s*\\{\\s*\\}\\s*\\]",
        "\"security\": [{\"Bearer\": []}]");
}
