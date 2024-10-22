using API.MiddleWare;
using API.SignalR;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(opt => {
          opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSingleton<IConnectionMultiplexer>(config => {
          var conString = builder.Configuration.GetConnectionString("Redis") ?? throw new Exception("Cannot get redis connection string");
          var configuration = ConfigurationOptions.Parse(conString, true);
          return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddCors();
builder.Services.AddSingleton<ICartService, CartService>();
builder.Services.AddSingleton<IResponseCacheService, ResponseCacheService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
          .AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<StoreContext>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
          .WithOrigins("http://localhost:4200","https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>();
app.MapHub<NotificationHub>("/hub/notifications");

try
{
          using var scope = app.Services.CreateScope();
          var services = scope.ServiceProvider;
          var context = services.GetRequiredService<StoreContext>();
          var userManager = services.GetRequiredService<UserManager<AppUser>>();
          await context.Database.MigrateAsync();
          await StoreContextSeed.SeedAsync(context, userManager);
}
catch (Exception ex)
{
          Console.WriteLine(ex);
          throw;
}

app.Run();
