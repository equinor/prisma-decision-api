using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Repositories;
using PrismaApi.Application.Services;
using PrismaApi.Infrastructure;
using PrismaApi.Infrastructure.DiscreteTables;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<DiscreteTableSaveChangesInterceptor>();
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(serviceProvider.GetRequiredService<DiscreteTableSaveChangesInterceptor>());
});

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication/authorization hook - intentionally permissive for local testing.
app.UseAuthorization();

app.MapControllers();

app.Run();
