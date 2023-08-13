using ASP_Components;
using ASP_Components.Middleware;
using ASP_Components.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IRedirectService, RedirectService>();

//Add Settings
builder.Services.Configure<ComponentSettings>(builder.Configuration.GetSection("ComponentSettings"));

var app = builder.Build();
var loggerFactory = app.Services.GetService<ILoggerFactory>();
loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"].ToString());

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
app.UseMiddleware<RedirectMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.Run();