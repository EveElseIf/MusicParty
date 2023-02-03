using AspNetCore.Proxy;
using MusicParty;
using MusicParty.Hub;
using MusicParty.MusicApi;
using MusicParty.MusicApi.NeteaseCloudMusic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Add music api
var musicApiList = new List<IMusicApi>();
if (bool.Parse(builder.Configuration["MusicApi:NeteaseCloudMusic:Enabled"]))
{
    var api = new NeteaseCloudMusicApi(
        builder.Configuration["MusicApi:NeteaseCloudMusic:ApiServerUrl"],
        builder.Configuration["MusicApi:NeteaseCloudMusic:PhoneNo"],
        builder.Configuration["MusicApi:NeteaseCloudMusic:Password"],
        bool.Parse(builder.Configuration["MusicApi:NeteaseCloudMusic:SMSLogin"])
    );
    api.Login();
    musicApiList.Add(api);
}

// Add more music api provider in the future.
if (musicApiList.Count == 0)
    throw new Exception("Cannot start without any music api service.");

builder.Services.AddSingleton<IEnumerable<IMusicApi>>(musicApiList);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<UserManager>();
builder.Services.AddAuthentication("Cookies").AddCookie("Cookies");
builder.Services.AddProxies();
builder.Services.AddSingleton<MusicBroadcaster>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<MusicHub>("/music");
});

// Proxy the front end server.
app.RunHttpProxy(builder.Configuration["FrontEndUrl"]);

app.Run();