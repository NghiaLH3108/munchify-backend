using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MunchifyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

// Add Repositories and Services to DI Container
builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IUserRepository, Munchify.DAL.Repository.UserRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IUserService, Munchify.BLL.Service.UserService>();

builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IUnitOfWork, Munchify.DAL.Repository.UnitOfWork>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.ICategoryService, Munchify.BLL.Service.CategoryService>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IUnitService, Munchify.BLL.Service.UnitService>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IInventoryService, Munchify.BLL.Service.InventoryService>();

builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IProductRepository, Munchify.DAL.Repository.ProductRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IProductService, Munchify.BLL.Service.ProductService>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IComboService, Munchify.BLL.Service.ComboService>();

builder.Services.AddScoped(typeof(Munchify.DAL.Repository.Interfaces.IGenericRepository<>), typeof(Munchify.DAL.Repository.GenericRepository<>));
builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IStockImportRepository, Munchify.DAL.Repository.StockImportRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IStockImportService, Munchify.BLL.Service.StockImportService>();

builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.ICustomerRepository, Munchify.DAL.Repository.CustomerRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.ICustomerService, Munchify.BLL.Service.CustomerService>();

builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.ISupplierRepository, Munchify.DAL.Repository.SupplierRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.ISupplierService, Munchify.BLL.Service.SupplierService>();
builder.Services.AddHttpClient<Munchify.BLL.Service.Interfaces.IInventoryAiService, Munchify.BLL.Service.InventoryAiService>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IDamagedExpiredService, Munchify.BLL.Service.DamagedExpiredService>();
builder.Services.AddHostedService<Munchify.API.BackgroundServices.ExpiryCheckBackgroundService>();

// Orders & Promotions
builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IOrderRepository, Munchify.DAL.Repository.OrderRepository>();
builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IPromotionRepository, Munchify.DAL.Repository.PromotionRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IOrderService, Munchify.BLL.Service.OrderService>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IPromotionService, Munchify.BLL.Service.PromotionService>();

builder.Services.AddAutoMapper(config => config.AddProfile<Munchify.BLL.Helpers.MappingProfile>());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.SetIsOriginAllowed(_ => true) // Allow any origin for SignalR
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // SignalR requires credentials
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Bỏ HttpsRedirection để dùng cổng HTTP cục bộ không bị lỗi CORS/SSL Timeout
// app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<Munchify.API.Hubs.InventoryHub>("/hubs/inventory");

app.Run();
