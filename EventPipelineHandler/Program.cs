using EventPipelineHandler.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using EventPipelineHandler.EventManager;
using EventPipelineHandler.Services;
using Microsoft.AspNetCore.ResponseCompression;
using EventPipelineHandler.Hubs.BlazorServerSignalRApp.Server.Hubs;
using EventPipelineHandler.Events.CustomEvents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddSingleton<EventActionHub>();
builder.Services.AddScoped<CreateCustomerInDbEvent>();
builder.Services.AddScoped<TransferToSharepointEventAction>();
builder.Services.AddHostedService<EventActionBackgroundService>();
builder.Services.AddScoped<IEventRunner, EventRunner>();
builder.Services.AddSingleton<BackgroundTaskQueue>();
builder.Services.AddScoped<EventService>();

// Add entity framework dbcontext with sqlite, using the nuget packag
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=EventPipelineHandler.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseResponseCompression();
app.MapBlazorHub();
app.MapHub<EventActionHub>("/eventActionHub");
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
}

app.Run();
