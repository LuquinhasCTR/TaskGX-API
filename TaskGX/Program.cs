using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskGX.API.Repositories;
using TaskGX.API.Services;
using TaskGX.Data;

var builder = WebApplication.CreateBuilder(args);

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TaskGXContext>(options =>
    options.UseMySQL(connectionString)
);

// Email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// DI
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<TokenService>();

// CORS (dev)
builder.Services.AddCors(o =>
{
    o.AddPolicy("dev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key não configurado no appsettings.json.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskGX";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TaskGX";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// Controllers + Swagger (simples)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("dev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();