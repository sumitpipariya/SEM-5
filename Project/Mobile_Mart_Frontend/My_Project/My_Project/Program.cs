using My_Project.Services;
using Microsoft.AspNetCore.Http;

namespace Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<AuthService>();

            

            // ✅ Enable Session for JWT storage
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expiry
            });

            var app = builder.Build();

            // ✅ Enable Session Middleware

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession();


            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

                     
            app.UseAuthentication();    
            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
