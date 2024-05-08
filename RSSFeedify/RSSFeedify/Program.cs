using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PostgreSQL.Data;
using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository;
using RSSFeedify.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();

// Register your DbContext
builder.Services.AddDbContext<ApplicationDbContext>();

// Register IRSSFeedItemRepository implementation
builder.Services.AddScoped<IRSSFeedItemRepository>(serviceProvider =>
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var dbSet = context.Set<RSSFeedItem>();
    return new RSSFeedItemRepository(context, dbSet);
});

// Register IRSSFeedRepository implementation
builder.Services.AddScoped<IRepository<RSSFeed>>(serviceProvider =>
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var dbSet = context.Set<RSSFeed>();
    return new Repository<RSSFeed>(context, dbSet);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// (...)
builder.Services.AddHostedService<RSSFeedPollingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
