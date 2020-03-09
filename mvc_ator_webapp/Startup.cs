using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using mvc_ator_webapp.Filters;

namespace mvc_ator_webapp
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

            #region snippet_AddRazorPages
            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                    {
                        options.Conventions
                            .AddPageApplicationModelConvention("/AWSLambdaConvert",
                                model =>
                                {
                                    //model.Filters.Add(
                                        //new GenerateAntiforgeryTokenCookieAttribute());
                                    model.Filters.Add(
                                        new DisableFormValueModelBindingAttribute());
                                });
                    });
            #endregion

            // To list physical files from a path provided by configuration:
            var physicalProvider = new PhysicalFileProvider(Configuration.GetValue<string>("StoredFilesPath"));

            services.AddSingleton<IFileProvider>(physicalProvider);

/// https://stackoverflow.com/questions/23402210/the-anti-forgery-token-could-not-be-decrypted/53870092#53870092
/// https://stackoverflow.com/questions/42103004/using-antiforgery-in-asp-net-core-and-got-error-the-antiforgery-token-could-no/47143941#47143941
            services.AddDataProtection()
                .SetApplicationName("ator-mvc-framework")
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(@"/var/secret/"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
