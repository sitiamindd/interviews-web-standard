using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Api.Models;
using Microsoft.AspNetCore.Http.Json;   
using System.Text.Json.Serialization;

// Alias your entity type so it never conflicts with System.Threading.Tasks.Task
using TaskEntity = Api.Models.Task;
using TagEntity = Api.Models.Tag;
var builder = WebApplication.CreateBuilder(args);

// Allow Nuxt dev server to call the API
builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
        .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// EF Core + SQLite
builder.Services.AddDbContext<TaskDb>(options =>
    options.UseSqlite("Data Source=app.db"));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("frontend");  

// Ensure DB/tables exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDb>();
    db.Database.Migrate();
}

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger"));

// ----------------- Task Endpoints -----------------

// GET /todoitems  (list all, with tags)
app.MapGet("/todoitems", (TaskDb db) =>
    db.Tasks.Include(t => t.Tags)
            .AsNoTracking()
            .OrderByDescending(t => t.Id)
            .ToListAsync());

// GET /todoitems/complete  (only completed)
app.MapGet("/todoitems/complete", (TaskDb db) =>
    db.Tasks.Include(t => t.Tags)
            .AsNoTracking()
            .Where(t => t.IsComplete)
            .ToListAsync());

// GET /todoitems/{id}
app.MapGet("/todoitems/{id:int}", async (int id, TaskDb db) =>
{
    var task = await db.Tasks.Include(t => t.Tags).FirstOrDefaultAsync(t => t.Id == id);
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
    var task = await db.Tasks.Include(t => t.Tags).FirstOrDefaultAsync(t => t.Id == id);
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

// PATCH /todoitems/{id}/tags/add
// Body: [1,2,3]  (list of tag IDs to add)
app.MapPatch("/todoitems/{id:int}/tags/add", async (int id, [FromBody] List<int> tagIds, TaskDb db) =>
{
    var task = await db.Tasks.Include(t => t.Tags).FirstOrDefaultAsync(t => t.Id == id);
    if (task is null) return Results.NotFound();

    var tags = await db.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
    foreach (var tg in tags)
        if (!task.Tags.Any(x => x.Id == tg.Id))
            task.Tags.Add(tg);

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// PATCH /todoitems/{id}/tags/remove
// Body: [1,2,3]  (list of tag IDs to remove)
app.MapPatch("/todoitems/{id:int}/tags/remove", async (int id, [FromBody] List<int> tagIds, TaskDb db) =>
{
    var task = await db.Tasks.Include(t => t.Tags).FirstOrDefaultAsync(t => t.Id == id);
    if (task is null) return Results.NotFound();

    task.Tags.RemoveAll(t => tagIds.Contains(t.Id));
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ----------------- Tag Endpoints -----------------

// GET /tags (with their tasks)
app.MapGet("/tags", (TaskDb db) =>
    db.Tags.Include(t => t.Tasks).AsNoTracking().ToListAsync());

// GET /tags/{id}
app.MapGet("/tags/{id:int}", async (int id, TaskDb db) =>
{
    var tag = await db.Tags.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.Id == id);
    return tag is null ? Results.NotFound() : Results.Ok(tag);
});

// POST /tags
app.MapPost("/tags", async ([FromBody] TagEntity tag, TaskDb db) =>
{
    db.Tags.Add(tag);
    await db.SaveChangesAsync();
    return Results.Created($"/tags/{tag.Id}", tag);
});

// DELETE /tags/{id}
app.MapDelete("/tags/{id:int}", async (int id, TaskDb db) =>
{
    var tag = await db.Tags.FindAsync(id);
    if (tag is null) return Results.NotFound();

    db.Tags.Remove(tag);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// PUT /tags/{id}
app.MapPut("/tags/{id:int}", async (int id, [FromBody] TagEntity input, TaskDb db) =>
{
    var tag = await db.Tags.FindAsync(id);
    if (tag is null) return Results.NotFound();
    tag.Name = input.Name;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
