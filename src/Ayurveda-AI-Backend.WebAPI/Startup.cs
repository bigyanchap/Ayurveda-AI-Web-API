using System.Text.Json.Serialization;
using Ayurveda_AI_Backend.Infrastructure.Database;
using Ayurveda_AI_Backend.Infrastructure.Supabase;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Repository.Repositories;
using Ayurveda_AI_Backend.Service.Interfaces;
using Ayurveda_AI_Backend.Service.Services;
using Ayurveda_AI_Backend.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Ayurveda_AI_Backend.WebAPI;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<IQuizQuestionService, QuizQuestionService>();
        services.AddHttpClient<IGeminiService, GeminiService>();
        services.AddHttpClient(); // IHttpClientFactory for AuthController

        services.Configure<GeminiOptions>(_configuration.GetSection("Gemini"));
        services.Configure<SupabaseOptions>(_configuration.GetSection("Supabase"));
        services.AddSingleton<ISupabaseClientProvider, SupabaseClientProvider>();

        var supabaseUrl = _configuration.GetValue<string>("Supabase:Url") ?? "";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Use Supabase's JWKS endpoint for ES256 token validation
                options.Authority = $"{supabaseUrl}/auth/v1";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"{supabaseUrl}/auth/v1",
                    ValidateAudience = true,
                    ValidAudiences = new[] { "authenticated" },
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    NameClaimType = "sub",
                    RoleClaimType = "role",
                };

                // Keep Supabase claims as-is (sub, email, role)
                options.MapInboundClaims = false;
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
        });

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("DefaultCors");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // Auto-create local User record on first authenticated request
        app.UseMiddleware<EnsureUserMiddleware>();

        app.MapControllers();
    }
}
