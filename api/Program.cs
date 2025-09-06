using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

// Alias your entity type so it never conflicts with System.Threading.Tasks.Task
using TaskEntity = Api.Models.Task;

var builder = WebApplication.CreateBuilder(args);

// EF Core + SQLite
builder.Services.AddDbContext<TaskDb>(options =>
    options.UseSqlite("Data Source=app.db"));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure DB/tables exist for quick testing (for production prefer migrations)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDb>();
    db.Database.EnsureCreated();
}

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger"));

// ----------------- Endpoints (same as tutorial) -----------------

// GET /todoitems  (list all)
app.MapGet("/todoitems", (TaskDb db) =>
    db.Tasks.AsNoTracking().OrderByDescending(t => t.Id).ToListAsync());

// GET /todoitems/complete  (only completed)
app.MapGet("/todoitems/complete", (TaskDb db) =>
    db.Tasks.AsNoTracking().Where(t => t.IsComplete).ToListAsync());

// GET /todoitems/{id}
app.MapGet("/todoitems/{id:int}", async (int id, TaskDb db) =>
{
    var task = await db.Tasks.FindAsync(id);
    return task is null ? Results.NotFound() : Results.Ok(task);
});

// POST /todoitems
app.MapPost("/todoitems", async ([FromBody] TaskEntity task, TaskDb db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{task.Id}", task);
});

// PUT /todoitems/{id}
app.MapPut("/todoitems/{id:int}", async (int id, [FromBody] TaskEntity input, TaskDb db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.Name = input.Name;
    task.IsComplete = input.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE /todoitems/{id}
app.MapDelete("/todoitems/{id:int}", async (int id, TaskDb db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
