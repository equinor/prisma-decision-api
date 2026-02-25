using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using PrismaApi.Api.Configuration.Extensions;
using PrismaApi.Application.Repositories;
using PrismaApi.Application.Services;
using PrismaApi.Infrastructure;
using PrismaApi.Infrastructure.DiscreteTables;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5004", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("GraphApi"))
    .AddInMemoryTokenCaches();

builder.Services.AddMemoryCache();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IDiscreteTableRuleTrigger, DiscreteTableRuleTrigger>();
builder.Services.AddScoped<TableRebuildingService>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<IssueRepository>();
builder.Services.AddScoped<NodeRepository>();
builder.Services.AddScoped<NodeStyleRepository>();
builder.Services.AddScoped<EdgeRepository>();
builder.Services.AddScoped<DecisionRepository>();
builder.Services.AddScoped<OptionRepository>();
builder.Services.AddScoped<OutcomeRepository>();
builder.Services.AddScoped<UncertaintyRepository>();
builder.Services.AddScoped<UtilityRepository>();
builder.Services.AddScoped<ValueMetricRepository>();
builder.Services.AddScoped<StrategyRepository>();
builder.Services.AddScoped<ObjectiveRepository>();
builder.Services.AddScoped<ProjectRoleRepository>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<IssueService>();
builder.Services.AddScoped<NodeService>();
builder.Services.AddScoped<NodeStyleService>();
builder.Services.AddScoped<EdgeService>();
builder.Services.AddScoped<DecisionService>();
builder.Services.AddScoped<OptionService>();
builder.Services.AddScoped<OutcomeService>();
builder.Services.AddScoped<UncertaintyService>();
builder.Services.AddScoped<UtilityService>();
builder.Services.AddScoped<ValueMetricService>();
builder.Services.AddScoped<StrategyService>();
builder.Services.AddScoped<ObjectiveService>();
builder.Services.AddScoped<ProjectRoleService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddSwagger(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerWithAuth(builder.Configuration);
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS - must be before UseAuthorization
app.UseCors("AllowFrontend");

// Authentication/authorization hook - intentionally permissive for local testing.
app.UseAuthorization();

app.MapControllers();

app.Run();
