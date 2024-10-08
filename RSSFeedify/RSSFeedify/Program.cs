using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PostgreSQL.Data;
using RSSFeedify.Controllers.Helpers;
using RSSFeedify.Models;
using RSSFeedify.Repository;
using RSSFeedify.Services;
using RSSFeedifyCommon.Services;
using Serilog;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure logging.
Log.Logger = new LoggingService(builder.Configuration).Logger;
builder.Host.UseSerilog();

// Load environment variables from .env file.
DotNetEnv.Env.Load();

// Add services to the container.
builder.Services.AddControllers();

// Configure PostgreSQL connection string
var defaultConnectionString = Environment.GetEnvironmentVariable("RSSFEEDIFY_PG_DB");

// Register your DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(defaultConnectionString));

// Register Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer =>
{
    var configuration = ConfigurationOptions.Parse(Environment.GetEnvironmentVariable("RSSFEEDIFY_REDIS_DB"), true);
    return ConnectionMultiplexer.Connect(configuration);
}); // add LoggerFactory, see https://stackexchange.github.io/StackExchange.Redis/Configuration

// Configure Authentication with JWT
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("RSSFEEDIFY_JWT_KEY"))),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configure Identity options for strong passwords
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

// Configure customized ModelState error messages format.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        return ControllersHelper.GetFormattedModelStateErrorMessage(actionContext);
    };
});

// Register RoleManager.
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Register IRSSFeedItemRepository implementation.
builder.Services.AddScoped<IRSSFeedItemRepository>(serviceProvider =>
{
    return new RSSFeedItemRepository(serviceProvider.GetRequiredService<IConfiguration>());
});

// Register IRSSFeedRepository implementation.
builder.Services.AddScoped<IRSSFeedRepository>(serviceProvider =>
{
    return new RSSFeedRepository(serviceProvider.GetRequiredService<IConfiguration>());
});

// Add Swagger with JWT Bearer Authorization
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RSSFeedify", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Register hosted service for polling of RSSFeeds.
builder.Services.AddHostedService<RSSFeedPollingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use exceptions and errors middleware.
app.UseExceptionHandler("/api/Error/error");

// Register JWT blacklisting middleware.
app.UseMiddleware<JWTBlacklistService>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Run automatic database migration.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    if (context.Database.GetPendingMigrations().Any())

    {
        context.Database.Migrate();
    }
}

// Run database seeder.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await Seeder.Run(services);
}

app.Run();
