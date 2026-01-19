using HytaleModLister.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register services
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IModExtractorService, ModExtractorService>();
builder.Services.AddSingleton<IModMatcherService, ModMatcherService>();
builder.Services.AddHttpClient<ICurseForgeService, CurseForgeService>();
builder.Services.AddSingleton<IModRefreshService, ModRefreshService>();
builder.Services.AddSingleton<RefreshSchedulerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RefreshSchedulerService>());

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
