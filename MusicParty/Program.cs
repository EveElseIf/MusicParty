using AspNetCore.Proxy;
using MusicParty;
using MusicParty.Hub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton(new NeteaseApi(
    builder.Configuration["NeteaseApiUrl"],
    builder.Configuration["NeteaseId"],
    builder.Configuration["NeteasePassword"]
));
builder.Services.AddCors(op =>
{
    op.AddPolicy("cors", p =>
        p.WithOrigins(builder.Configuration["FrontEndUrl"]).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    );
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<UserManager>();
builder.Services.AddAuthentication("Cookies").AddCookie("Cookies");
builder.Services.AddProxies();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors("cors");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<MusicHub>("/music");
});


app.RunHttpProxy(builder.Configuration["FrontEndUrl"]);

app.Run();