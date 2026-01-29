using BookingOrchestrationApi.Services;
using BookingOrchestrationApi.Models.ApiClients;
using BookingOrchestrationApi.Services.Hotel;
using BookingOrchestrationApi.Services.Restaurant;
using BookingOrchestrationApi.Models.ApiClients.Restaurant;
using BookingOrchestrationApi.Models.ApiClients.Hotel;
using System.Text.Json.Serialization;
using BookingOrchestrationApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HttpClient for external APIs
builder.Services.AddHttpClient<IHotelApiClient, HotelApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:HotelApi:BaseUrl"] 
        ?? "https://webapi-lmar-prd-hotel01-gmhbb0abafggdgfc.westeurope-01.azurewebsites.net/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IGiteApiClient, GiteApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:GiteApi:BaseUrl"] 
        ?? "https://webapi-lmar-prd-gite01-bgerc3b8gxhcd8fs.westeurope-01.azurewebsites.net/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add this for Camping API
builder.Services.AddHttpClient<ICampingApiClient, CampingApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:CampingApi:BaseUrl"] 
        ?? "https://webapp-lgpteam-camping-marconnes.azurewebsites.net/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IRestaurantApiClient, RestaurantApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:RestaurantApi:BaseUrl"] 
        ?? "https://your-restaurant-api-url.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register restaurant services
builder.Services.AddScoped<RestaurantBookingService>();
builder.Services.AddScoped<CampingBookingService>();
builder.Services.AddScoped<HotelBookingService>();
builder.Services.AddScoped<GiteBookingService>();

// Register concrete implementations against their interfaces
builder.Services.AddScoped<IHotelBookingService>(sp => sp.GetRequiredService<HotelBookingService>());

// Note: GiteBookingController expects IBookingService for GiteBookingService
// and CampingBookingController expects CampingBookingService directly

// Register controllers (they will be auto-discovered)
// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();