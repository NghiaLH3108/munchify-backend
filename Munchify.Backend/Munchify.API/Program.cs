using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Munchify.DAL.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MunchifyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

// Add Repositories and Services to DI Container
builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IUserRepository, Munchify.DAL.Repository.UserRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IUserService, Munchify.BLL.Service.UserService>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IAuthService, Munchify.BLL.Service.AuthService>();

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

builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IPromotionRepository, Munchify.DAL.Repository.PromotionRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IPromotionService, Munchify.BLL.Service.PromotionService>();


builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.ICustomerRepository, Munchify.DAL.Repository.CustomerRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.ICustomerService, Munchify.BLL.Service.CustomerService>();

builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.ISupplierRepository, Munchify.DAL.Repository.SupplierRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.ISupplierService, Munchify.BLL.Service.SupplierService>();
builder.Services.AddHttpClient<Munchify.BLL.Service.Interfaces.IInventoryAiService, Munchify.BLL.Service.InventoryAiService>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IEmailService, Munchify.BLL.Service.EmailService>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IDamagedExpiredService, Munchify.BLL.Service.DamagedExpiredService>();
builder.Services.AddHostedService<Munchify.API.BackgroundServices.ExpiryCheckBackgroundService>();

// Orders
builder.Services.AddScoped<Munchify.DAL.Repository.Interfaces.IOrderRepository, Munchify.DAL.Repository.OrderRepository>();
builder.Services.AddScoped<Munchify.BLL.Service.Interfaces.IOrderService, Munchify.BLL.Service.OrderService>();

builder.Services.AddAutoMapper(config => config.AddProfile<Munchify.BLL.Helpers.MappingProfile>());

// JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MunchifySuperSecretKeyForJwtAuthentication2026!LongKeyHere";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MunchifyAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MunchifyClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.Name
    };

    options.Events = new JwtBearerEvents
    {
        // SignalR (WebSocket) không gửi được header -> nhận token qua query string
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) &&
                context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Chưa đăng nhập hoặc phiên đăng nhập đã hết hạn."
            });
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Bạn không có quyền thực hiện chức năng này."
            });
        }
    };
});

// Mặc định mọi endpoint yêu cầu đăng nhập (dùng [AllowAnonymous] để mở ngoại lệ)
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Munchify API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT Bearer token theo định dạng: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Munchify.API.Hubs.InventoryHub>("/hubs/inventory");

app.Run();
