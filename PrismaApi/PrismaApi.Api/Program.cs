using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using PrismaApi.Api.Configuration.Extensions;
using PrismaApi.Api.Configuration.JsonResponseOptions;
using PrismaApi.Application.Interfaces;
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
    .AddDownstreamApi("FastApi", builder.Configuration.GetSection("FastApiService"))
    .AddMicrosoftGraph(builder.Configuration.GetSection("GraphApi"))
    .AddInMemoryTokenCaches();

builder.Services.AddMemoryCache();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IDiscreteTableRuleEventHandler, DiscreteTableRuleEventHandler>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<INodeStyleRepository, NodeStyleRepository>();
builder.Services.AddScoped<IEdgeRepository, EdgeRepository>();
builder.Services.AddScoped<IDecisionRepository, DecisionRepository>();
builder.Services.AddScoped<IOptionRepository, OptionRepository>();
builder.Services.AddScoped<IOutcomeRepository, OutcomeRepository>();
builder.Services.AddScoped<IUncertaintyRepository, UncertaintyRepository>();
builder.Services.AddScoped<IUtilityRepository, UtilityRepository>();
builder.Services.AddScoped<IDiscreteProbabilityRepository, DiscreteProbabilityRepository>();
builder.Services.AddScoped<IDiscreteUtilityRepository, DiscreteUtilityRepository>();
builder.Services.AddScoped<IValueMetricRepository, ValueMetricRepository>();
builder.Services.AddScoped<IStrategyRepository, StrategyRepository>();
builder.Services.AddScoped<IObjectiveRepository, ObjectiveRepository>();
builder.Services.AddScoped<IProjectRoleRepository, ProjectRoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ITableRebuildingService, TableRebuildingService>();
builder.Services.AddScoped<IStrategyTableService, StrategyTableService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<INodeService, NodeService>();
builder.Services.AddScoped<INodeStyleService, NodeStyleService>();
builder.Services.AddScoped<IEdgeService, EdgeService>();
builder.Services.AddScoped<IDecisionService, DecisionService>();
builder.Services.AddScoped<IOptionService, OptionService>();
builder.Services.AddScoped<IOutcomeService, OutcomeService>();
builder.Services.AddScoped<IUncertaintyService, UncertaintyService>();
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<IDiscreteProbabilityService, DiscreteProbabilityService>();
builder.Services.AddScoped<IDiscreteUtilityService, DiscreteUtilityService>();
builder.Services.AddScoped<IValueMetricService, ValueMetricService>();
builder.Services.AddScoped<IStrategyService, StrategyService>();
builder.Services.AddScoped<IObjectiveService, ObjectiveService>();
builder.Services.AddScoped<IProjectRoleService, ProjectRoleService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new DateTimeOffsetJsonConverter());
    });

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
