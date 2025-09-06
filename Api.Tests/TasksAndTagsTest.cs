using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

// Disable parallel so both tests don't step on the same app.db file.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

public class TasksAndTagsTests
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // DTOs that match your API responses
    public record TagDto(int Id, string? Name);
    public record TaskDto(int Id, string? Name, bool IsComplete, System.Collections.Generic.List<TagDto> Tags);

    [Fact]
    public async Task Task_CRUD_and_Tag_Assign_Unassign()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client  = factory.CreateClient();

        // 1) Create a tag
        var tagResp = await client.PostAsJsonAsync("/tags", new { name = "Urgent" });
        Assert.Equal(HttpStatusCode.Created, tagResp.StatusCode);
        var tag = await tagResp.Content.ReadFromJsonAsync<TagDto>(JsonOpts);
        Assert.NotNull(tag);

        // 2) Create a task
        var taskResp = await client.PostAsJsonAsync("/todoitems", new { name = "Buy milk", isComplete = false });
        Assert.Equal(HttpStatusCode.Created, taskResp.StatusCode);
        var task = await taskResp.Content.ReadFromJsonAsync<TaskDto>(JsonOpts);
        Assert.NotNull(task);
        Assert.Empty(task!.Tags);

        // 3) Assign tag (PATCH) — no helper, build request manually
        using (var req = new HttpRequestMessage(HttpMethod.Patch, $"/todoitems/{task.Id}/tags/add"))
        {
            req.Content = JsonContent.Create(new[] { tag!.Id });
            var add = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);
        }

        var afterAdd = await client.GetFromJsonAsync<TaskDto>($"/todoitems/{task.Id}", JsonOpts);
        Assert.NotNull(afterAdd);
        Assert.Contains(afterAdd!.Tags, tg => tg.Id == tag.Id && tg.Name == "Urgent");

        // 4) Update task
        var put = await client.PutAsJsonAsync($"/todoitems/{task.Id}", new { name = "Buy milk + eggs", isComplete = true });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var updated = await client.GetFromJsonAsync<TaskDto>($"/todoitems/{task.Id}", JsonOpts);
        Assert.Equal("Buy milk + eggs", updated!.Name);
        Assert.True(updated.IsComplete);

        // 5) Unassign tag
        using (var req = new HttpRequestMessage(HttpMethod.Patch, $"/todoitems/{task.Id}/tags/remove"))
        {
            req.Content = JsonContent.Create(new[] { tag.Id });
            var rem = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.NoContent, rem.StatusCode);
        }

        var afterRemove = await client.GetFromJsonAsync<TaskDto>($"/todoitems/{task.Id}", JsonOpts);
        Assert.Empty(afterRemove!.Tags);

        // 6) Delete task (cleanup)
        var del = await client.DeleteAsync($"/todoitems/{task.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        // Also delete the tag (cleanup)
        var delTag = await client.DeleteAsync($"/tags/{tag.Id}");
        Assert.True(delTag.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Tag_Name()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client  = factory.CreateClient();

        // create
        var create = await client.PostAsJsonAsync("/tags", new { name = "Home" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var tag = await create.Content.ReadFromJsonAsync<TagDto>(JsonOpts);
        Assert.NotNull(tag);

        // update
        var put = await client.PutAsJsonAsync($"/tags/{tag!.Id}", new { name = "House" });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        // verify via list
        var list = await client.GetFromJsonAsync<System.Collections.Generic.List<TagDto>>("/tags", JsonOpts);
        Assert.Contains(list!, t => t.Id == tag.Id && t.Name == "House");

        // cleanup
        var del = await client.DeleteAsync($"/tags/{tag.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
    }
}
