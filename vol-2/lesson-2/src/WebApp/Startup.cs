﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServiceComposer.AspNetCore;

namespace WebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddControllersWithViews();
            services.AddViewModelComposition(options =>
            {
                options.EnableCompositionOverControllers(true);
                options.EnableWriteSupport();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseBrowserLink();

            app.UseStaticFiles();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCompositionHandlers();
            });
        }
    }
}
