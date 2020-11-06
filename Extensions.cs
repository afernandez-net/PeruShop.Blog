using Blog.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog
{
    public static class Extensions
    {
        public static IApplicationBuilder UseDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var env = serviceScope.ServiceProvider.GetService<IHostingEnvironment>();

                var context = serviceScope.ServiceProvider.GetRequiredService<BlogContext>();

                context.Database.EnsureCreated();
            }

            return app;
        }
    }
}
