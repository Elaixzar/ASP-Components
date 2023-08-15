using ASP_Components.Middleware;
using ASP_Components.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//Add Redirect Service <-- TALKING POINT
builder.Services.AddSingleton<IRedirectService, RedirectService>();

var app = builder.Build();

//Add Logger <-- TALKING POINT
var loggerFactory = app.Services.GetService<ILoggerFactory>();
loggerFactory.AddFile(builder.Configuration.GetSection("Logging:LogFilePath:Default").Value);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapRazorPages();

//Add Redirect Middleware <-- TALKING POINT
app.UseMiddleware<RedirectMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.Run();