using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MunchifyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddAutoMapper(config => config.AddProfile<Munchify.BLL.Helpers.MappingProfile>());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Disabled for local HTTP development

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
