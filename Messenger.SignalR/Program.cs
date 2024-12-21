using Messenger.SignalR.Data;
using Messenger.SignalR.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationDbContext")));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHub<ChatHub>("/chat");

app.Run();
