using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using autoease_backend.Data;
using autoease_backend.Services;
using Microsoft.EntityFrameworkCore;

namespace autoease_backend.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(IServiceProvider services, ILogger<NotificationBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("NotificationBackgroundService running at: {time}", DateTimeOffset.Now);

                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        // 1. Low stock (threshold < 10)
                        var lowStockParts = await dbContext.Parts
                            .Where(p => p.StockQuantity < 10)
                            .ToListAsync(stoppingToken);

                        foreach (var part in lowStockParts)
                        {
                            var adminEmail = "aayahi175@gmail.com";
                            var subject = $"Low Stock Alert: {part.Name}";
                            var body = $"Part {part.Name} has low stock. Current quantity: {part.StockQuantity}";
                            
                            await emailService.SendEmailAsync(adminEmail, subject, body);
                        }

                        // 2. Outstanding credit balances older than one month
                        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
                        var outstandingInvoices = await dbContext.Invoices
                            .Include(i => i.Customer)
                            .Where(i => i.PaymentStatus == "Outstanding" || i.PaymentStatus == "Pending")
                            .Where(i => i.InvoiceDate < oneMonthAgo)
                            .ToListAsync(stoppingToken);

                        foreach (var invoice in outstandingInvoices)
                        {
                            if (invoice.Customer != null && !string.IsNullOrEmpty(invoice.Customer.Email))
                            {
                                var subject = $"Outstanding Balance Reminder - Invoice #{invoice.Id}";
                                var body = $"Dear {invoice.Customer.Name}, you have an outstanding balance of {invoice.TotalAmount} for invoice #{invoice.Id} dated {invoice.InvoiceDate.ToShortDateString()}. Please settle it as soon as possible.";
                                
                                await emailService.SendEmailAsync(invoice.Customer.Email, subject, body);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing notification logic.");
                }

                // Run once a day
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);


            }
        }
    }
}
