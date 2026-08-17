using EnterpriseAI.Api.Services;
using EnterpriseAI.Api.Configuration;
using EnterpriseAI.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddHttpClient("Ollama", client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});
builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection("Ollama"));

builder.Services.AddScoped<
    IConversationService,
    ConversationService>();

builder.Services.AddScoped<
    IEmbeddingService,
    OllamaEmbeddingService>();

builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IRagService, RagService>();

builder.Services.AddDbContext<EnterpriseAiDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IChatService,ChatService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();