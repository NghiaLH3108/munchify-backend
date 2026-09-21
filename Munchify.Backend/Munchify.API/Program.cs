using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MunchifyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories and Services to DI Container
builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IUserRepository, Munchify.DAL.Repository.UserRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IUserService, Munchify.BLL.Service.UserService>();

// Add services to the container.
builder.Services.AddControllers();
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
