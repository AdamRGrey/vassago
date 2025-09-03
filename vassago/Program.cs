using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json;
using vassago;
using vassago.Models;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHostedService, vassago.ConsoleService>();
builder.Services.AddDbContext<ChattingContext>();
builder.Services.AddControllers().AddNewtonsoftJson(options => {
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.MissingMemberHandling =  MissingMemberHandling.Error;
        options.SerializerSettings.Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
            {
                Console.Error.WriteLine($"serialization error - {args.ErrorContext.Error}");
            };
    });
builder.Services.AddProblemDetails();
builder.Services.Configure<RazorViewEngineOptions>(o => {
    o.ViewLocationFormats.Clear();
    o.ViewLocationFormats.Add("/WebInterface/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
    o.ViewLocationFormats.Add("/WebInterface/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "api");
});

//app.UseExceptionHandler();
//app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

Shared.App = app;

app.Run();
