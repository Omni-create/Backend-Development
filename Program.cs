using HotelApi.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<DBConnect>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HotelDb")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection(); <-- dit zorgde voor een probleem binnen mijn browser.

app.UseAuthorization();

app.MapControllers();

app.Run();
