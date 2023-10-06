using MagicVilla_Web;
using MagicVilla_Web.Extensions;
using MagicVilla_Web.Services;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(/*this is our exception filter*/u=>u.Filters.Add(new AuthExceptionRedirection()));

// automapper
builder.Services.AddAutoMapper(typeof(MappingConfig));

// registering services for VillaAPI api
builder.Services.AddHttpClient<IVillaService, VillaService>();
builder.Services.AddScoped<IVillaService, VillaService>();

// registering services for VillaNumber api 
builder.Services.AddHttpClient<IVillaNumberService, VillaNumberService>();
builder.Services.AddScoped<IVillaNumberService, VillaNumberService>();

// registering services for TokenProvider 
builder.Services.AddScoped<ITokenProvider, TokenProvider>();

// registering services for BaseService 
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// registering services for BaseService api 
builder.Services.AddScoped<IBaseService, BaseService>();

// registering 
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// after separating request message to another file we inject it   
builder.Services.AddSingleton<IApiMessageRequestBuilder, ApiMessageRequestBuilder>();

//
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(100);
        options.LoginPath = "/Auth/LogIn";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.SlidingExpiration = true;
    });

// adding session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// this is for authentication 
app.UseAuthentication();

app.UseAuthorization();

// adding session to request pipeline
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
