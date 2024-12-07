using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Portfolio.Api;
using Portfolio.Api.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped(_ =>
    new ConnectionString(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(_ =>
    new SqliteConnection(builder.Configuration.GetConnectionString("SQLiteConnection")));

var dbFilePath = builder.DataSource;
// Ensure the directory exists
var directory = Path.GetDirectoryName(dbFilePath);
if (!Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
}

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMyIdentityRepository, MyIdentityRepository>();
builder.Services.AddScoped<IPowerBIDataRepository, PowerBIDataRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database initialization
SQLiteDatabaseInitialiser.Initialise(builder.Configuration.GetConnectionString("SQLiteConnection"));


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

