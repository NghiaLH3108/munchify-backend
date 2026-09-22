using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MunchifyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IUnitOfWork, Munchify.DAL.Repository.UnitOfWork>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.ICategoryService, Munchify.BLL.Service.CategoryService>();

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

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
