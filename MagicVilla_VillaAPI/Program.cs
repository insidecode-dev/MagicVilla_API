using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using MagicVilla_VillaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//connection to database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

// configurong caching 
builder.Services.AddResponseCaching();

//registering repository
builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// adding .NET Identity to the project 
builder.Services.AddIdentity</*first time what I wrote : IdentityUser , I changed this because ApplicationUser is derived from IdentityUser class and also contains some more properties in it*/ ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// with the line below versioning support has been added to api 
builder.Services.AddApiVersioning(options =>
{
    // without the configuration below when we run our application we get error llike api version is not specified, because AddApiVersioning requires version from our endpoints and I've not still provided endpoints with API Version attribute 
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // with enabling the property below we'll be able to see supported versions in response header as api-supported-versions
    options.ReportApiVersions = true;
});


// configuring the versioned API explorer
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    // this property makes it possible for not to entering version of endpoint in each request, it will be automatically set
    options.SubstituteApiVersionInUrl = true;
});


var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

//registering automapper 
builder.Services.AddAutoMapper(typeof(MappingConfig));

builder.Services.AddControllers(options =>
{
    //options.ReturnHttpNotAcceptable = true;  //returns error message if return type format of response is not acceptable, it means if we do not add this manually and set true it will be false by default and as a result it will return data as json format by default even if the return format is switched to text/plain in swagger documentation no error will be throwed

    options.CacheProfiles.Add("Default30", new Microsoft.AspNetCore.Mvc.CacheProfile
    {
        Duration = 30
    });
}).AddNewtonsoftJson();  // we added AddNewtonsoftJson() extension method manually for HttpPatch reuqest
                           //.AddXmlDataContractSerializerFormatters();  //if we want to get response in xml format we add this service

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // first endpoint in this order will be default in swagger

        // here we add another version for ourself
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Magic_VillaV2");

        // this is the first version of swagger, we override it below 
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Magic_VillaV1");
    });
}

// for wwwroot folder   
app.UseStaticFiles();


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages();

app.MapControllers();
ApplyMigration();
app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}