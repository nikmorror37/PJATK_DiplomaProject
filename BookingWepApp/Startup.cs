using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookingWepApp.Data;
using BookingWepApp.Models;
using System.Threading.Tasks;
using System;
using System.Globalization;
using Microsoft.OpenApi.Models;
using System.Linq;

namespace BookingWepApp
{
    public class Startup
    {
        //Этот код представляет конструктор для класса Startup.
        //Конструктор принимает объект IConfiguration в качестве параметра и сохраняет его в свойстве Configuration класса Startup.
        //IConfiguration является интерфейсом, который предоставляет доступ к конфигурационным данным приложения,
        //таким как строки подключения к базе данных, настройки приложения и другие параметры.
        //Он используется для централизованного управления конфигурацией приложения.
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        //собственно, это само свойство Configuration
        public IConfiguration Configuration { get; }

        //Этот код представляет метод ConfigureServices
        //В этом методе происходит настройка сервисов приложения.
        public void ConfigureServices(IServiceCollection services)
        {
            //регистрирует контекст базы данных ApplicationDbContext в контейнере внедрения зависимостей (DI container).
            //Это позволяет другим компонентам приложения получать доступ к этому контексту базы данных через механизм внедрения зависимостей.
            services.AddDbContext<ApplicationDbContext>();
            //добавляет сервисы для аутентификации и авторизации в приложении.
            //Он регистрирует стандартные службы идентификации, используя класс ApplicationUser в качестве модели пользователя.
            //Опция RequireConfirmedAccount установлена в false, что означает, что не требуется подтверждение учетной записи пользователя.
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                // добавляет роли (ролевую систему) к сервисам идентификации
                .AddRoles<IdentityRole>()
                //указывает, что службы идентификации должны использовать ApplicationDbContext для хранения данных пользователей и ролей
                .AddEntityFrameworkStores<ApplicationDbContext>();
            //добавляет распределенный кэш в памяти в качестве сервиса. Он используется для кэширования данных в приложении.
            services.AddDistributedMemoryCache();
            //добавляет поддержку сессий в приложении. Он регистрирует сервисы, необходимые для работы сессий
            services.AddSession();
            //добавляет службы MVC (Model-View-Controller) в приложение.
            //Он регистрирует контроллеры и представления в контейнере внедрения зависимостей.
            services.AddControllersWithViews();
            //указывает, что экземпляр ApplicationDbContext должен быть создан один раз на запрос (scoped lifetime).
            //Это означает, что каждый HTTP-запрос будет использовать один и тот же экземпляр ApplicationDbContext.
            services.AddScoped<ApplicationDbContext>();
            //добавляет поддержку Swagger для генерации документации API.
            //Он настраивает версию документации и другие параметры Swagger.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
            //добавляет поддержку Razor Pages в приложение и включает компиляцию Razor страниц во время выполнения
            services.AddRazorPages().AddRazorRuntimeCompilation();
        }

        //В этом методе происходит настройка конвейера обработки запросов и маршрутизации запросов в контроллеры и Razor Pages.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            //позволяет установить язык по умолчанию для обработки строк,
            //форматирования даты и времени и других культурозависимых операций.
            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            //Если текущее окружение приложения (env) находится в разработке
            if (env.IsDevelopment())
            {
                //Добавляет страницу с подробной информацией об ошибке при разработке и отображает исключения на странице.
                app.UseDeveloperExceptionPage();
                //Добавляет страницу с подробной информацией об ошибках базы данных при разработке.
                app.UseDatabaseErrorPage();
                //Включает поддержку Swagger для генерации и отображения документации API во время разработки.
                //Определяется конечная точка Swagger и ее маршрут.
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("./v1/swagger.json", "My API V1");
                });
            }
            //иначе
            else
            {
                //Устанавливает обработчик исключений для необработанных ошибок и перенаправляет на заданный маршрут обработчика ошибок.
                app.UseExceptionHandler("/Home/Error");
                //Включает использование HTTP Strict Transport Security (HSTS), чтобы защитить приложение от атак через протокол HTTP.
                app.UseHsts();
            }
            //Добавляет промежуточное ПО для перенаправления запросов с HTTP на HTTPS для обеспечения безопасного соединения.
            app.UseHttpsRedirection();
            //Добавляет промежуточное ПО для обслуживания статических файлов, таких как HTML, CSS, JavaScript и изображения.
            app.UseStaticFiles();
            //Добавляет промежуточное ПО для поддержки сеансов в приложении.
            app.UseSession();
            //Добавляет промежуточное ПО для маршрутизации запросов в соответствующие обработчики.
            app.UseRouting();
            //Добавляет промежуточное ПО для аутентификации пользователей в приложении.
            app.UseAuthentication();
            //Добавляет промежуточное ПО для авторизации пользователей и проверки доступа к ресурсам в приложении.
            app.UseAuthorization();
            //Настраивает конечные точки маршрутизации запросов
            app.UseEndpoints(endpoints =>
            {
                //Задает обработчик для всех запросов, которые не соответствуют другим маршрутам.
                //Он перенаправляет такие запросы на действие "Index" контроллера "Home".
                endpoints.MapFallbackToController(
                    action: "Index",
                    controller: "Home"
                    );
                //Задает маршрут по умолчанию для контроллеров, используя шаблон "{controller=Home}/{action=Index}/{id?}".
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                //Добавляет маршруты для Razor Pages.
                endpoints.MapRazorPages();
                //Добавляет маршруты для контроллеров API.
                endpoints.MapControllers();
            });
            //Вызывает асинхронный метод CreateUserRoles с передачей serviceProvider в качестве параметра.
            //выполняет логику связанную с созданием ролей пользователей.
            CreateUserRoles(serviceProvider).Wait();
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            //Получение необходимых сервисов из serviceProvider (поставщика сервисов)
            //получаются необходимые сервисы RoleManager<IdentityRole> и UserManager<ApplicationUser>
            //Эти сервисы используются для управления ролями и пользователями в приложении.
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //Создается массив строк roles, содержащий имена ролей, которые нужно создать (например, "Admin" и "User").
            string[] roles = { "Admin", "User" };
            IdentityResult result;

            //Проверка существования ролей
            foreach (var role in roles)
            {
                //для каждой роли в массиве roles проверяется, существует ли уже такая роль
                var roleAlreadyExists = await roleManager.RoleExistsAsync(role);
                //Если роль не существует
                if (!roleAlreadyExists)
                {
                    //то вызывается метод roleManager.CreateAsync, чтобы создать роль.
                    result = await roleManager.CreateAsync(new IdentityRole(role));
                }
                //Создание и добавление администратора
                //Создается объект ApplicationUser с именем пользователя, полученным из конфигурации 
                var admin = new ApplicationUser
                {
                    UserName = Configuration["AdminLogin"]
                };
                var pw = Configuration["AdminPassword"];
                //Поиск пользователя
                //Проверяется, существует ли пользователь с заданным именем (admin.UserName). 
                var user = await userManager.FindByNameAsync(admin.UserName);
                //Если пользователь не найден
                if (user == null)
                {
                    //Вызывается метод userManager.CreateAsync, чтобы создать пользователя с указанным именем и паролем
                    var addAdmin = await userManager.CreateAsync(admin, pw);
                    //Если создание пользователя прошло успешно(addAdmin.Succeeded)
                    if (addAdmin.Succeeded)
                    {
                        //то вызывается метод userManager.AddToRoleAsync, чтобы добавить созданного пользователя в роль "Admin".
                        await userManager.AddToRoleAsync(admin, "Admin");
                    }
                }
            }
        }
    }
}