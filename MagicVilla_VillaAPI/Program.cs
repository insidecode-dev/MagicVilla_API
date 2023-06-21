using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel 
    .Debug() //anything above the debug will be logged
    .WriteTo
    .File("log/villalogs.txt", rollingInterval:RollingInterval.Day) // rollingInterval defines how often should the new file be created
    .CreateLogger();

//we're saying the application here that we need to use this Serilog logging configuration rather than the built in console logging 
builder.Host.UseSerilog();

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
