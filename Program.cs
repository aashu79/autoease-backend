using autoease_backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using autoease_backend.Data.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddControllers();

// Required for app.UseAuthorization()
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token here."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database connection (single registration)
builder.Services.AddDbContext<autoease_backend.Data.AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
           // Log pending-model-changes warning instead of letting it surface as an exception
           .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<autoease_backend.Data.AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_secret_key_needs_to_be_long_enough"))
    };
});

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

using (var scope = app.Services.CreateScope())
{
    var _dbContext = scope.ServiceProvider.GetRequiredService<autoease_backend.Data.AppDbContext>();
    try
    {
        _dbContext.Database.Migrate();
    }
    catch (InvalidOperationException ex)
    {
        // Do not crash the app here; surface a clear message so developer can run migrations manually.
        Console.Error.WriteLine("EF Core detected pending model changes. Create and apply a new migration before updating the database. \nRun: 'dotnet ef migrations add <Name>' and 'dotnet ef database update'.\nDetails: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
// Use the named CORS policy that allows all origins, methods, and headers
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();