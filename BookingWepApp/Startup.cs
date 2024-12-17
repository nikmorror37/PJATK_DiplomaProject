using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookingWepApp.Data;
using BookingWepApp.Models;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Security.Claims;

namespace BookingWepApp
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
            // Register MongoDbContext
            services.AddSingleton<MongoDbContext>();
            services.AddSingleton<IMongoClient, MongoClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetSection("MongoDbSettings:ConnectionString").Value;
                return new MongoClient(connectionString);
            });

            // Register MongoUserStore and MongoRoleStore
            services.AddScoped<IUserStore<ApplicationUser>, MongoUserStore>();
            services.AddScoped<IRoleStore<IdentityRole>, MongoRoleStore>();
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>(); //new


            //// Identity configuration for ApplicationUser with MongoDB
            //services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddDefaultTokenProviders();

            // Identity configuration for ApplicationUser with MongoDB
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role; // Укажите правильный тип для ролей
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnValidatePrincipal = async context =>
                {
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var user = await userManager.GetUserAsync(context.Principal);
                    if (user != null)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        var identity = new ClaimsIdentity();
                        foreach (var role in roles)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        }
                        context.Principal.AddIdentity(identity);
                    }
                };
            });



            // Session configuration
            services.AddDistributedMemoryCache();
            services.AddSession();

            // Add MVC, Razor Pages, and Swagger
            services.AddControllersWithViews();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            // Set default culture
            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            // Configure request pipeline
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallbackToController("Index", "Home");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            CreateUserRoles(serviceProvider).Wait();
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminUserName = Configuration["AdminLogin"];
            var adminPassword = Configuration["AdminPassword"];

            var admin = await userManager.FindByNameAsync(adminUserName);
            if (admin == null)
            {
                //var newAdmin = new ApplicationUser { UserName = adminUserName };
                var newAdmin = new ApplicationUser
                {
                    UserName = adminUserName,
                    FirstName = "Top",
                    LastName = "Admin",
                    Email = "admin@example.com"
                };

                var createAdminResult = await userManager.CreateAsync(newAdmin, adminPassword);
                if (createAdminResult.Succeeded)
                {   
                    await userManager.AddToRoleAsync(newAdmin, "Admin");

                    // Explicitly add role claims
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, adminUserName),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    foreach (var claim in claims)
                    {
                        await userManager.AddClaimAsync(newAdmin, claim);
                    }
                }
            }
        }
    }
}
