using BackendTest.Conway.Data;
using BackendTest.Conway.Interfaces;
using BackendTest.Conway.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BackendTestConwayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BackendTestConwayContext") ?? throw new InvalidOperationException("Connection string 'BackendTestConwayContext' not found.")));

// Add services to the container.
builder.Services.AddScoped<IGameLogicService, GameLogicService>();
builder.Services.AddScoped<IGameOfLifeService, GameOfLifeService>();

builder.Services.AddControllers().AddNewtonsoftJson();
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



