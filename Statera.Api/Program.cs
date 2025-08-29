// Program.cs
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Statera.Api.Data;
// optional: for safer connection logging
// using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Controllers + enums as strings
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Nice EF error pages (dev only)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Auto-create DB/apply migrations in dev
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Optional: log which server/db we’re hitting WITHOUT leaking creds
        // var csb = new SqlConnectionStringBuilder(db.Database.GetDbConnection().ConnectionString);
        // app.Logger.LogInformation("Migrating DB on {Server}/{Database}", csb.DataSource, csb.InitialCatalog);
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var c = ("Conn(Default): " + cfg.GetConnectionString("Default"));

        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database migration failed.");
        throw;
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
