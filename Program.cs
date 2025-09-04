using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Chat.Data;
using Chat.Repository;
using Chat.Repository.Interfaces;
using Chat.Service;
using Chat.Service.Interfaces;
using AutoMapper;
using Chat.AutoMapper;
using Microsoft.AspNetCore.Identity;
using Chat.Models;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using BotChat.Service;
using Chat.HttpClients.Interface;
using Chat.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Xóa mapping mặc định của JWT claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BotChat API", Version = "v1" });

    // Cấu hình JWT Bearer cho Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Nhập JWT token vào ?ây (Bearer {token})",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Cho phép tất cả domain (dev)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// đăng ký Identity
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(AppMapperProfile));

// đăng ký Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IConversationsRepository, ConversationsRepository>();
builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// đăng ký Service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IConversationsService, ConversationsService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Đăng ký HttpClient cho các dịch vụ bên ngoài
builder.Services.AddScoped<IExternalApi, ExternalApi>();

// Cấu hình xác thực JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    })
    .AddCookie()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        googleOptions.CallbackPath = "/signin-google";
    });

builder.Services.AddHttpClient();

// đăng ký Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// đăng ký Swagger
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Sử dụng Swagger môi trường phát triển
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();