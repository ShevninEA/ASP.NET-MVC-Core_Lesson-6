using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autofac.Configuration;
using Lesson6.Services;
using Lesson6.Services.Impl;
using ASP.NET_MVC_Core_Lesson_6.Models.Reports;
using ASP.NET_MVC_Core_Lesson_6.Services.Impl;
using ASP.NET_MVC_Core_Lesson_6.Services;
using ASP.NET_MVC_Core_Lesson_6.Extention;

namespace Lesson6
{
    internal class Program
    {
        private static Random random = new Random();

        private static IHost? _host;

        public static IHost Hosting => _host ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())


            .ConfigureContainer<ContainerBuilder>(container => // Autofac
            {

                var config = new ConfigurationBuilder()
                        .AddJsonFile("autofac.config.json", true, false);
                var module = new ConfigurationModule(config.Build());
                var builder = new ContainerBuilder();
                builder.RegisterModule(module);

            })
            .ConfigureHostConfiguration(options =>
                options.AddJsonFile("appsettings.json"))
            .ConfigureAppConfiguration(options =>
                options.AddJsonFile("appsettings.json")
                .AddXmlFile("appsettings.xml", true)
                .AddIniFile("appsettings.ini", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args))
            .ConfigureLogging(options =>
                options.ClearProviders()
                    .AddConsole()
                    .AddDebug())
            .ConfigureServices(ConfigureServices);


        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            #region Register Base Services

            // Стандартный способ регистрации сервиса (Microsoft.Extensions.DependencyInjection)
            services.AddTransient<IOrderService, OrderService>();


            #endregion

            #region Configure EF DBContext Service

            services.AddDbContext<OrdersDbContext>(options =>
            {
                options.UseSqlServer(host.Configuration["Settings:DatabaseOptions:ConnectionString"]);
            });

            #endregion
        }

        public static IServiceProvider Services => Hosting.Services;

        static async Task Main(string[] args)
        {
            var host = Hosting;
            await host.StartAsync();
            await PrintBuyersAsync();
            Console.ReadKey(true);
            await host.StopAsync();
        }

        private static async Task PrintBuyersAsync()
        {
            await using (var servicesScope = Services.CreateAsyncScope())
            {
                var services = servicesScope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<OrdersDbContext>();

                await context.Database.MigrateAsync();

                foreach (var buyer in context.Buyers)
                {
                    logger.LogInformation($"Покупатель >>> {buyer.Id} {buyer.LastName} {buyer.Name} {buyer.Patronymic} {buyer.Birthday.ToShortDateString()}");
                }

                var orderService = services.GetRequiredService<IOrderService>();


                await orderService.CreateAsync(random.Next(1, 6), "123, Russia, Address", "+79001112233", new (int, int)[] {
                    new ValueTuple<int, int>(1, 1)
                });


                var catalog = new ProductsCatalog
                {
                    Name = "Каталог товаров",
                    Description = "Актуальный список товаров на дату",
                    CreationDate = DateTime.Now,
                    Products = context.Products
                };

                string templateFile = "Templates/DefaultTempate.docx";
                IProductReport report = new ProductReportWord(templateFile);

                CreateReport(report, catalog, "Report.docx");

                Console.ReadKey(true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportGenerator">Объект - генератор отчета</param>
        /// <param name="catalog">Объект с данными</param>
        /// <param name="reportFileName">Наименование файла-отчета</param>
        private static void CreateReport(IProductReport reportGenerator, ProductsCatalog catalog, string reportFileName)
        {
            reportGenerator.CatalogName = catalog.Name;
            reportGenerator.CatalogDescription = catalog.Description;
            reportGenerator.CreationDate = catalog.CreationDate;
            reportGenerator.Products = catalog.Products.Select(product => (product.Id, product.Name, product.Category, product.Price));

            var reportFileInfo = reportGenerator.Create(reportFileName);
            reportFileInfo.Execute();
        }


    }
    
}
