using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SecondApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Регистрация HttpClient
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Ваш текущий код настройки приложения
            // ...

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Определение маршрута только для нужного контроллера (замените "Fibonacci" на ваш контроллер)
                endpoints.MapControllerRoute(
                    name: "Fibonacci",
                    pattern: "Fibonacci/{action}/{id?}", // Укажите нужный шаблон маршрута
                    defaults: new { controller = "Fibonacci", action = "Index" }); // Укажите нужный метод

                // Добавьте другие маршруты для других контроллеров, если нужно
                // ...
            });
        }
    }
}

