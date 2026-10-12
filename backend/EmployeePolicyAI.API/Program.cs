using EmployeePolicyAI.API.Data;
using EmployeePolicyAI.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddDbContext<EmployeePolicyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<OllamaEmbeddingService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
    client.Timeout = TimeSpan.FromMinutes(2);
});
builder.Services.AddHttpClient<OllamaAnswerService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddScoped<IEmployeePolicyService, EmployeePolicyService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<DocumentChunkService>();
builder.Services.AddSingleton<InMemoryVectorStore>();
builder.Services.AddScoped<DocumentIndexingService>();
builder.Services.AddScoped<DocumentChunkRepository>();
builder.Services.AddHostedService<DocumentIndexLoader>();

//builder.Services.AddScoped<OpenAIEmbeddingService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("ReactFrontend");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
