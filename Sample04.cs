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

namespace Lesson6
{
    internal class Sample04
    {
        private static IHost? _host;

        public static IHost Hosting => _host ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(container => // Autofac
                {

                    //container.RegisterType<OrderService>().As<IOrderService>().InstancePerLifetimeScope();
                    //container.RegisterType<OrderService>().InstancePerLifetimeScope();
                    //container.RegisterModule<ServicesModule>();
                    //container.RegisterAssemblyModules(Assembly.GetCallingAssembly());
                    
                    
                    //TODO: Почему не работает регистрация через конфиг?
                    var config = new ConfigurationBuilder()
                    .AddJsonFile("autofac.config.json", false, false);
                    //.AddXmlFile("autofac.config.xml", false, false);




                    var module = new ConfigurationModule(config.Build());
                    var builder = new ContainerBuilder();
                    builder.RegisterModule(module);

                })
                .ConfigureHostConfiguration(options =>
                    options.AddJsonFile("appsettings.json"))
                .ConfigureAppConfiguration(options =>
                    options
                        .AddJsonFile("appsettings.json")
                        .AddXmlFile("appsettings.xml", true)
                        .AddIniFile("appsettings.ini", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args))
                .ConfigureLogging(options =>
                options.ClearProviders() // Microsoft.Extensions.Logging
                    .AddConsole()
                    .AddDebug())
                .ConfigureServices(ConfigureServices);
        }

        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
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

        private static Random random = new Random();

        private static async Task PrintBuyersAsync()
        {
            await using var servicesScope = Services.CreateAsyncScope();
            var services = servicesScope.ServiceProvider;

            var context = services.GetRequiredService<OrdersDbContext>();
            var logger = services.GetRequiredService<ILogger<Sample03>>();

            foreach (var buyer in context.Buyers)
            {
                logger.LogInformation($"Покупатель >>> {buyer.LastName} {buyer.Name} {buyer.Patronymic} {buyer.Birthday.ToShortDateString()}");
            }

            var orderService = services.GetRequiredService<IOrderService>();

            await orderService.CreateAsync(random.Next(1, 6), "123, Russia, Address", "+79001112233", new (int, int)[] {
                    new ValueTuple<int, int>(1, 1)
            });
        }
    }
}
