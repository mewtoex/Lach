using WhatsAppService.Data;
using WhatsAppService.Services;
using Lach.Shared.Messaging.Interfaces;
using Lach.Shared.Messaging.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<WhatsAppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite") ?? "Data Source=whatsapp.db"));

// Message Bus
builder.Services.AddSingleton<IMessageBus, RabbitMQMessageBus>();

// WhatsApp Services
builder.Services.Configure<WhatsAppWebOptions>(builder.Configuration.GetSection("WhatsAppWeb"));
builder.Services.AddHttpClient<IWhatsAppWebService, WhatsAppWebService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { status = "Healthy", service = "WhatsAppService", timestamp = DateTime.UtcNow });

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
    context.Database.EnsureCreated();
}

app.Run(); 