using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<autoease_backend.Data.AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? ""));

builder.Services.AddTransient<autoease_backend.Services.IEmailService,
                              autoease_backend.Services.EmailService>();
builder.Services.AddScoped<autoease_backend.Services.IPurchaseInvoiceService,
                           autoease_backend.Services.PurchaseInvoiceService>();
builder.Services.AddScoped<autoease_backend.Services.ISalesInvoiceService,
                           autoease_backend.Services.SalesInvoiceService>();

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
