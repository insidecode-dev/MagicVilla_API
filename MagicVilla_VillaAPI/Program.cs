
using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//connection to database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

//registering automapper 
builder.Services.AddAutoMapper(typeof(MappingConfig));

builder.Services.AddControllers(/*options =>
{
    options.ReturnHttpNotAcceptable = true;  //returns error message if return type format of response is not acceptable, it means if we do not add this manually and set true it will be false by default and as a result it will return data as json format by default even if the return format is switched to text/plain in swagger documentation no error will be throwed
}*/)
    .AddNewtonsoftJson();  // we added AddNewtonsoftJson() extension method manually for HttpPatch reuqest
    //.AddXmlDataContractSerializerFormatters();  //if we want to get response in xml format we add this service

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();