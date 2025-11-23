using backend.Data;
using backend.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register database connection factory
var connectionString = "Data Source=Data/team-tracker.db";
builder.Services.AddSingleton<IDbConnectionFactory>(sp => new SqliteConnectionFactory(connectionString));

// Register repositories
builder.Services.AddScoped<backend.Data.Repositories.ITeamMemberRepository, backend.Data.Repositories.TeamMemberRepository>();
builder.Services.AddScoped<backend.Data.Repositories.IGoalRepository, backend.Data.Repositories.GoalRepository>();

// Register services
builder.Services.AddScoped<backend.Services.ITeamMemberService, backend.Services.TeamMemberService>();
builder.Services.AddScoped<backend.Services.IGoalService, backend.Services.GoalService>();

var app = builder.Build();

// Run database migrations
using (var scope = app.Services.CreateScope())
{
    var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    using var connection = connectionFactory.CreateConnection();

    var migrationScript = File.ReadAllText("Data/Migrations/001_InitialSchema.sql");
    using var command = connection.CreateCommand();
    command.CommandText = migrationScript;
    command.ExecuteNonQuery();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();
