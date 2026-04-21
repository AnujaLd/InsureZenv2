using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using Serilog;
using InsureZenv2.src.Data;
using InsureZenv2.src.Authentication;
using InsureZenv2.src.Repositories;
using InsureZenv2.src.Services;
using InsureZenv2.src.Validators;
using InsureZenv2.src.Mappers;
using InsureZenv2.src.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Add Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MakerOnly", policy =>
        policy.RequireClaim("Role", "Maker"));
    
    options.AddPolicy("CheckerOnly", policy =>
        policy.RequireClaim("Role", "Checker"));
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add FluentValidation
builder.Services.AddScoped<IValidator<ClaimIngestDto>, ClaimIngestDtoValidator>();
builder.Services.AddScoped<IValidator<MakerReviewSubmitDto>, MakerReviewSubmitDtoValidator>();
builder.Services.AddScoped<IValidator<CheckerReviewSubmitDto>, CheckerReviewSubmitDtoValidator>();
builder.Services.AddScoped<IValidator<UserLoginDto>, UserLoginDtoValidator>();
builder.Services.AddScoped<IValidator<UserRegisterDto>, UserRegisterDtoValidator>();

// Add custom services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Add repositories
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClaimReviewRepository, ClaimReviewRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Add application services
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
