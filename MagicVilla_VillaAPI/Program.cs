using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Code to use Serilog to Log to a file.
/*Log.Logger =  new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File("log/VillaLogs.txt",
    rollingInterval:RollingInterval.Day).CreateLogger();
builder.Host.UseSerilog();*/

builder.Services.AddDbContext<ApplicationDbContext>(option => {
        option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
        });
builder.Services.AddScoped<IVillaRepository,VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddControllers(
    //option => {option.ReturnHttpNotAcceptable = true; }
    )
.AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAutoMapper(typeof(MappingConfig));
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
