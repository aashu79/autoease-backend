using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<autoease_backend.Data.AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? ""));

builder.Services.AddTransient<autoease_backend.Services.IEmailService, autoease_backend.Services.EmailService>();
builder.Services.AddTransient<autoease_backend.Services.IEmailService, autoease_backend.Services.EmailService>();
builder.Services.AddTransient<autoease_backend.Services.IFinancialReportService, autoease_backend.Services.FinancialReportService>();
builder.Services.AddTransient<autoease_backend.Services.ICustomerReportService, autoease_backend.Services.CustomerReportService>();
builder.Services.AddTransient<autoease_backend.Services.IInvoiceEmailService, autoease_backend.Services.InvoiceEmailService>();

builder.Services.AddControllers();
builder.Services.AddLogging();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
