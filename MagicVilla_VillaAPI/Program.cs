var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers(/*options =>
{
    options.ReturnHttpNotAcceptable = true;  //returns error message if return type format of response is not acceptable
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
