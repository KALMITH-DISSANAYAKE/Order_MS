using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<OrderMSDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderMSDbContext>();

            int orderReqId = 8;
            var orderRequest = await context.OrderRequests
                .Include(or => or.TransportAssignments)
                .FirstOrDefaultAsync(or => or.OrderReqId == orderReqId);

            if (orderRequest != null)
            {
                Console.WriteLine($"OrderReqId: {orderRequest.OrderReqId}");
                Console.WriteLine($"Assignments Count: {orderRequest.TransportAssignments.Count}");
                foreach(var ta in orderRequest.TransportAssignments)
                {
                    Console.WriteLine($"  AssignmentId: {ta.AssignmentId}, ConnectionId: {ta.ConnectionId}");
                }
            }
            else
            {
                Console.WriteLine("OrderRequest not found.");
            }
        }
    }
}
