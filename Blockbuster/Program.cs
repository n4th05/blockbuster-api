using Blockbuster.Application.Interfaces;
using Blockbuster.Application.Services;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Domain.Services;
using Blockbuster.Infrastructure.Data;
using Blockbuster.Infrastructure.Repositories;
using Dapper;
using Scalar.AspNetCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Replace EF Core with Dapper connection factory
builder.Services.AddSingleton<IDbConnectionFactory>(sp =>
    new PostgresConnectionFactory(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieDomainService, MovieDomainService>();
builder.Services.AddScoped<IMovieAppService, MovieAppService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserDomainService, UserDomainService>();
builder.Services.AddScoped<IUserAppService, UserAppService>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IRentalDomainService, RentalDomainService>();
builder.Services.AddScoped<IRentalAppService, RentalAppService>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var connectionFactory = services.GetRequiredService<IDbConnectionFactory>();
        using var connection = connectionFactory.CreateConnection();
        await DatabaseInitializer.InitializeAsync(connection);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

// Usar a política de CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

static async Task InitializeDatabase(IDbConnection connection)
{
    const string createTablesScript = @"
        CREATE TABLE IF NOT EXISTS Users (
            Id SERIAL PRIMARY KEY,
            Name VARCHAR(255) NOT NULL,
            Email VARCHAR(255) NOT NULL UNIQUE,
            Phone VARCHAR(50) NOT NULL,
            CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        );

        CREATE TABLE IF NOT EXISTS Movies (
            Id SERIAL PRIMARY KEY,
            Title VARCHAR(255) NOT NULL,
            Description TEXT NOT NULL,
            Value DECIMAL(18,2) NOT NULL,
            CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            DeletedAt TIMESTAMP NULL
        );

        CREATE TABLE IF NOT EXISTS Rentals (
            UserId INT NOT NULL,
            MovieId INT NOT NULL,
            StartDate TIMESTAMP NOT NULL,
            EndDate TIMESTAMP NOT NULL,
            PRIMARY KEY (UserId, MovieId),
            FOREIGN KEY (UserId) REFERENCES Users(Id),
            FOREIGN KEY (MovieId) REFERENCES Movies(Id)
        );";

    await connection.ExecuteAsync(createTablesScript);
}