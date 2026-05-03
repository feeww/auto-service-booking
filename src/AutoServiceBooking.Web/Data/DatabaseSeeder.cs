using AutoServiceBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            await SeedAutoServicesAsync(dbContext);
        }

        private static async Task SeedAutoServicesAsync(ApplicationDbContext dbContext)
        {
            List<AutoService> services = new List<AutoService>
            {
                new AutoService(
                    "Комп'ютерна діагностика",
                    "Перевірка електронних систем автомобіля та зчитування помилок.",
                    900,
                    40),

                new AutoService(
                    "Заміна масла",
                    "Заміна моторного масла та базова перевірка фільтрів.",
                    700,
                    30),

                new AutoService(
                    "Шиномонтаж",
                    "Заміна, балансування та сезонне обслуговування коліс.",
                    1200,
                    60),

                new AutoService(
                    "Ремонт гальм",
                    "Огляд і ремонт гальмівних колодок, дисків та супортів.",
                    1500,
                    90),

                new AutoService(
                    "Перевірка акумулятора",
                    "Перевірка заряду, клем, генератора та стану акумулятора.",
                    500,
                    20),

                new AutoService(
                    "Обслуговування кондиціонера",
                    "Діагностика, заправка та перевірка герметичності системи кондиціонування.",
                    1100,
                    50),

                new AutoService(
                    "Інше",
                    "Для нестандартних проблем, інцидентів або ситуацій, коли клієнт не знає точну послугу.",
                    0,
                    50)
            };

            foreach (AutoService service in services)
            {
                bool exists = await dbContext.AutoServices.AnyAsync(autoService => autoService.Name == service.Name);

                if (!exists)
                {
                    dbContext.AutoServices.Add(service);
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
