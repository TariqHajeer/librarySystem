using LibrarySystem.BusinessLogic;
using LibrarySystem.DataAccess;
using LibrarySystem.Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Required for minimal APIs and Swagger
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
builder.Services.InjectApplication(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
builder.Services.InjectDataAccessLayer(options =>

{
    options.ConnectionString = connectionString;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();          // Enable Swagger middleware
    app.UseSwaggerUI(c =>      // Enable Swagger UI
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library System API V1");
        c.RoutePrefix = string.Empty; // Swagger UI at app root: https://localhost:5001/
    });
}
// Use CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.MapControllers();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Run();

