using Messenger.API.Data;
using Messenger.API.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Messenger.API.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddCors();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"])),
            ValidateIssuerSigningKey = true
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accesToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accesToken) && path.StartsWithSegments("/chat"))
                {
                    context.Token = accesToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddSignalR();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationDbContext")));

var app = builder.Build();

app.UseCors(options =>
{
    options.WithOrigins("http://localhost:3000", "http://localhost:8000");
    options.AllowCredentials();
    options.AllowAnyHeader();
    options.AllowAnyMethod();
}); 
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
app.MapHub<ChatHub>("/chat");
app.MapControllers();

app.Run();
