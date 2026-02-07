using System.Text;
using Ayurveda_AI_Backend.Infrastructure.Database;
using Ayurveda_AI_Backend.Infrastructure.Supabase;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Repository.Repositories;
using Ayurveda_AI_Backend.Service.Interfaces;
using Ayurveda_AI_Backend.Service.Services;
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
        services.AddControllers();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<IQuizQuestionService, QuizQuestionService>();
        services.AddHttpClient<IGeminiService, GeminiService>();

        services.Configure<GeminiOptions>(_configuration.GetSection("Gemini"));
        services.Configure<SupabaseOptions>(_configuration.GetSection("Supabase"));
        services.AddSingleton<ISupabaseClientProvider, SupabaseClientProvider>();

        var supabaseJwtSecret = _configuration.GetValue<string>("Supabase:JwtSecret") ?? string.Empty;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseJwtSecret));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = !string.IsNullOrWhiteSpace(supabaseJwtSecret),
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
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

        app.UseHttpsRedirection();
        app.UseCors("DefaultCors");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
