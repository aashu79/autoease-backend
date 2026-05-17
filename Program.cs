using autoease_backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Required for app.UseAuthorization()
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Database connection
builder.Services.AddDbContext<autoease_backend.Data.AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? ""));


// Email service
builder.Services.AddTransient<autoease_backend.Services.IEmailService, autoease_backend.Services.EmailService>();

// ✅ Use full namespace since interfaces live inside the service files
builder.Services.AddScoped<autoease_backend.Services.IPurchaseInvoiceService,
                           autoease_backend.Services.PurchaseInvoiceService>();

builder.Services.AddScoped<autoease_backend.Services.ISalesInvoiceService,
                           autoease_backend.Services.SalesInvoiceService>();

builder.Services.AddHostedService<NotificationBackgroundService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();