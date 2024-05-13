    using PostgreSQL.Data;
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
        //var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        //var dbSet = context.Set<RSSFeedItem>();
        return new RSSFeedItemRepository(serviceProvider.GetRequiredService<IConfiguration>());
    });

    // Register IRSSFeedRepository implementation
    builder.Services.AddScoped<IRSSFeedRepository>(serviceProvider =>
    {
        //var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        //var dbSet = context.Set<RSSFeed>();
        return new RSSFeedRepository(serviceProvider.GetRequiredService<IConfiguration>());
    });

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Register hosted service
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
