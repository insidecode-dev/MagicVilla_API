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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        //BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        //Type = SecuritySchemeType.Http
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{Type = ReferenceType.SecurityScheme, Id="Bearer"},
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    // creating a swagger document for our specified version 
    // I created swagger documentation for v1
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Magic Villa V1",
        Description = "API to manage Villa",
        TermsOfService = new Uri("https://www.postman.com/"),
        Contact = new OpenApiContact
        {
            Name = "insidecode",
            Url = new Uri("https://github.com/insidecode-dev")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url= new Uri("https://www.google.com/search?q=license&oq=license&aqs=chrome..69i57.3695j0j7&sourceid=chrome&ie=UTF-8")
        }
    });

    // I created swagger documentation for v2
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        
        Version = "v2.0",
        Title = "Magic Villa V2",
        Description = "API to manage Villa",
        TermsOfService = new Uri("https://www.postman.com/"),
        Contact = new OpenApiContact
        {
            Name = "insidecode",
            Url = new Uri("https://github.com/insidecode-dev")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://www.google.com/search?q=license&oq=license&aqs=chrome..69i57.3695j0j7&sourceid=chrome&ie=UTF-8")
        }
    });
});





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

app.Run();