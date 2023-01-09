using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orders.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP.NET_MVC_Core_Lesson_6
{
    internal class Sample02
    {
        private static void Main(string[] args)
        {
            var serviceBuilder = new ServiceCollection();

            #region Configure EF DBContext Service

            serviceBuilder.AddDbContext<OrdersDbContext>(options =>
            {
                options.UseSqlServer("data source=DESKTOP-ISK20;initial catalog=OrdersDatabase;User Id=OrdersUser;Password=12345;App=EntityFramework;Trusted_Connection=True;Encrypt=false");
            });

            #endregion

            //serviceBuilder.AddSingleton<IService, ServiceImplementation>();

            var serviceProvider = serviceBuilder.BuildServiceProvider();

            var context = serviceProvider.GetRequiredService<OrdersDbContext>();

            foreach (var buyer in context.Buyers)
            {
                Console.WriteLine($"{buyer.LastName} {buyer.Name} {buyer.Patronymic} {buyer.Birthday.ToShortDateString()}");
            }

            Console.ReadKey(true);
        }
    }
}
