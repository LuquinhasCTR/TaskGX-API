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

var stringConexao = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("String de conexao 'DefaultConnection' nao encontrada.");

builder.Services.AddDbContext<TaskGXContext>(options => options.UseMySQL(stringConexao));

builder.Services
    .AddOptions<ConfiguracoesJwt>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<ConfiguracoesEmail>()
    .Bind(builder.Configuration.GetSection("ConfiguracoesEmail"))
    .ValidateDataAnnotations();

builder.Services.AddProblemDetails();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = contexto =>
    {
        var problemaValidacao = new ValidationProblemDetails(contexto.ModelState)
        {
            Title = "A requisicao contem dados invalidos.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        return new BadRequestObjectResult(problemaValidacao);
    };
});

builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<CadastroService>();
builder.Services.AddScoped<VerificacaoService>();
builder.Services.AddScoped<AlteracaoEmailService>();
builder.Services.AddScoped<EnvioEmailService>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", politica =>
        politica.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var configuracoesJwt = builder.Configuration.GetSection("Jwt").Get<ConfiguracoesJwt>()
    ?? throw new InvalidOperationException("Configuracoes JWT nao encontradas.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuracoesJwt.Emissor,
            ValidateAudience = true,
            ValidAudience = configuracoesJwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracoesJwt.Chave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var esquemaBearer = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Cole apenas o token JWT.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", esquemaBearer);
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseExceptionHandler(aplicacaoExcecao =>
{
    aplicacaoExcecao.Run(async contexto =>
    {
        var recursoExcecao = contexto.Features.Get<IExceptionHandlerPathFeature>();
        var excecao = recursoExcecao?.Error;
        var registrador = contexto.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("TratadorGlobalExcecao");

        if (excecao != null)
        {
            registrador.LogError(
                excecao,
                "Erro nao tratado em {Metodo} {Caminho}",
                contexto.Request.Method,
                contexto.Request.Path);
        }

        var problema = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Ocorreu um erro interno ao processar a requisicao."
        };

        if (app.Environment.IsDevelopment() && excecao != null)
        {
            problema.Detail = excecao.Message;
            problema.Extensions["tipoExcecao"] = excecao.GetType().FullName;
        }

        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await contexto.Response.WriteAsJsonAsync(problema);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapGet("/swagger/v1/swagger-corrigido.json", (ISwaggerProvider provedorSwagger) =>
    {
        var documentoSwagger = provedorSwagger.GetSwagger("v1");
        using var escritorTexto = new StringWriter();
        var escritorOpenApi = new OpenApiJsonWriter(escritorTexto);

        documentoSwagger.SerializeAsV3(escritorOpenApi);

        var jsonSwagger = CorrigirRequisitosSegurancaSwagger(escritorTexto.ToString());
        return Results.Text(jsonSwagger, "application/json");
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger-corrigido.json", "TaskGX v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    nome = "TaskGX API",
    status = "ok",
    ambiente = app.Environment.EnvironmentName
}));

app.MapControllers();
app.Run();

static string CorrigirRequisitosSegurancaSwagger(string jsonSwagger)
{
    return Regex.Replace(
        jsonSwagger,
        "\"security\"\\s*:\\s*\\[\\s*\\{\\s*\\}\\s*\\]",
        "\"security\": [{\"Bearer\": []}]");
}
