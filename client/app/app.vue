<template>
  <main class="min-h-screen bg-slate-50 text-slate-900">
    <div class="mx-auto max-w-6xl p-6 space-y-6">
      <!-- header -->
      <header class="text-center space-y-2">
        <span class="inline-block text-xs px-2 py-1 rounded-full bg-indigo-100 text-indigo-700">Nuxt 3 ▸ ASP.NET</span>
        <h1 class="text-3xl font-semibold">Tasks & Tags</h1>
        <p class="text-slate-500">Create, update, delete — and assign tags to tasks.</p>
      </header>

      <div v-if="status==='error'" class="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-red-700">
        <strong>Oops:</strong> {{ errorMsg }}
      </div>

      <!-- grid -->
      <section class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- TAGS -->
        <div class="rounded-xl border bg-white shadow-sm">
          <div class="p-4 border-b flex items-center justify-between gap-3">
            <h2 class="font-medium">Tags</h2>
            <form class="flex items-center gap-2" @submit.prevent="createTag">
              <input class="input" v-model="newTagName" placeholder="New tag name" required />
              <button class="btn">Add</button>
            </form>
          </div>

          <div v-if="tags.length===0" class="p-6 text-center text-slate-500">No tags yet. Add one above.</div>

          <ul v-else class="divide-y">
            <li v-for="tg in tags" :key="tg.id" class="p-4 flex items-center gap-3">
              <input class="input flex-1" v-model="tg.name" placeholder="Tag name" />
              <button class="btn" @click="updateTag(tg)" :disabled="busy.tagUpdate===tg.id">Save</button>
              <button class="btn btn-danger" @click="deleteTag(tg.id)" :disabled="busy.tagDelete===tg.id">Delete</button>
            </li>
          </ul>
        </div>

        <!-- TASKS -->
        <div class="rounded-xl border bg-white shadow-sm">
          <div class="p-4 border-b">
            <h2 class="font-medium mb-3">Tasks</h2>
            <form class="flex flex-wrap items-center gap-2" @submit.prevent="createTask">
              <input class="input flex-1 min-w-[220px]" v-model="newTaskName" placeholder="Task name" required />
              <label class="inline-flex items-center gap-2 text-sm text-slate-600">
                <input type="checkbox" v-model="newTaskComplete" class="h-4 w-4" />
                Complete?
              </label>
              <button class="btn" :disabled="busy.taskCreate">Add</button>
            </form>
          </div>

          <div v-if="status==='loading'" class="p-6 text-slate-500">Loading tasks…</div>
          <div v-else-if="tasks.length===0" class="p-6 text-center text-slate-500">No tasks yet. Add one above.</div>

          <ul v-else class="divide-y">
            <li v-for="t in tasks" :key="t.id" class="p-4 flex flex-col gap-3">
              <div class="flex items-center justify-between gap-3">
                <div class="flex items-center gap-3 flex-1">
                  <label class="inline-flex items-center gap-2 text-slate-600">
                    <input type="checkbox" class="h-4 w-4" v-model="t.isComplete" @change="updateTask(t)" />
                    <span class="text-xs text-slate-500">#{{ t.id }}</span>
                  </label>
                  <input
                    class="input flex-1"
                    :class="t.isComplete ? 'line-through text-slate-400' : ''"
                    v-model="t.name"
                    placeholder="Task title"
                  />
                </div>
                <div class="flex items-center gap-2">
                  <button class="btn" @click="updateTask(t)" :disabled="busy.taskUpdate===t.id">Save</button>
                  <button class="btn btn-ghost" @click="refreshOne(t.id)" title="Refresh">Refresh</button>
                  <button class="btn btn-danger" @click="deleteTask(t.id)" :disabled="busy.taskDelete===t.id">Delete</button>
                </div>
              </div>

              <!-- assigned tags -->
              <div class="flex flex-wrap gap-2">
                <span v-for="tg in t.tags" :key="tg.id" class="badge">
                  {{ tg.name }}
                  <button class="badge-x" title="Remove tag" @click="unassign(t.id, tg.id)">Remove</button>
                </span>
              </div>

              <!-- picker -->
              <details class="rounded-lg border bg-slate-50/50">
                <summary class="cursor-pointer px-3 py-2 text-sm text-slate-600">
                  Assign / Unassign Tags
                  <span class="text-slate-400">({{ t.tags.length }} selected)</span>
                </summary>
                <div class="px-3 pb-3 grid grid-cols-1 sm:grid-cols-2 gap-2">
                  <label v-for="tg in tags" :key="`P${t.id}-${tg.id}`" class="inline-flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      class="h-4 w-4"
                      :checked="t.tags.some(x => x.id === tg.id)"
                      @change="toggleAssign(t, tg.id, $event)"
                    />
                    <span>{{ tg.name }}</span>
                  </label>
                </div>
              </details>
            </li>
          </ul>
        </div>
      </section>

      <footer class="text-center text-xs text-slate-500">API: <code>{{ apiBase }}</code></footer>
    </div>
  </main>
</template>

<script setup lang="ts">
import { ofetch } from 'ofetch'
import { useRuntimeConfig } from 'nuxt/app'

type Tag  = { id: number; name: string | null }
type Task = { id: number; name: string | null; isComplete: boolean; tags: Tag[] }

const cfg = useRuntimeConfig()
const apiBase = (cfg.public?.apiBase as string) || 'http://localhost:5000'

const status = ref<'loading'|'ok'|'error'>('loading')
const errorMsg = ref('')
const tasks = ref<Task[]>([])
const tags  = ref<Tag[]>([])

const newTaskName = ref('')
const newTaskComplete = ref(false)
const newTagName = ref('')

const busy = reactive({
  taskCreate: false as boolean,
  taskUpdate: 0 as number|false,
  taskDelete: 0 as number|false,
  tagCreate: false as boolean,
  tagUpdate: 0 as number|false,
  tagDelete: 0 as number|false
})

async function loadAll() {
  status.value = 'loading'
  try {
    const [tks, tgs] = await Promise.all([
      ofetch<Task[]>(`${apiBase}/todoitems`),
      ofetch<Tag[]>(`${apiBase}/tags`)
    ])
    tasks.value = tks
    tags.value  = tgs
    status.value = 'ok'
  } catch (e: any) {
    errorMsg.value = e?.message || String(e)
    status.value = 'error'
  }
}
onMounted(loadAll)

/* ===== Tasks ===== */
async function createTask() {
  if (!newTaskName.value.trim()) return
  busy.taskCreate = true
  try {
    const created = await ofetch<Task>(`${apiBase}/todoitems`, {
      method: 'POST',
      body: { name: newTaskName.value.trim(), isComplete: newTaskComplete.value }
    })
    tasks.value.unshift({ ...created, tags: created.tags ?? [] })
    newTaskName.value = ''; newTaskComplete.value = false
  } finally { busy.taskCreate = false }
}
async function updateTask(t: Task) {
  busy.taskUpdate = t.id
  try {
    await ofetch(`${apiBase}/todoitems/${t.id}`, {
      method: 'PUT',
      body: { name: t.name ?? '', isComplete: t.isComplete }
    })
  } finally { busy.taskUpdate = false }
}
async function deleteTask(id: number) {
  if (!confirm('Delete this task?')) return
  busy.taskDelete = id
  try {
    await ofetch(`${apiBase}/todoitems/${id}`, { method: 'DELETE' })
    tasks.value = tasks.value.filter(t => t.id !== id)
  } finally { busy.taskDelete = false }
}
async function refreshOne(id: number) {
  try {
    const t = await ofetch<Task>(`${apiBase}/todoitems/${id}`)
    const idx = tasks.value.findIndex(x => x.id === id)
    if (idx >= 0) tasks.value[idx] = t
  } catch {}
}

/* ===== Tags ===== */
async function createTag() {
  if (!newTagName.value.trim()) return
  busy.tagCreate = true
  try {
    const created = await ofetch<Tag>(`${apiBase}/tags`, {
      method: 'POST',
      body: { name: newTagName.value.trim() }
    })
    tags.value.push(created); newTagName.value = ''
  } finally { busy.tagCreate = false }
}
async function updateTag(tg: Tag) {
  if (!tg.name?.trim()) return
  busy.tagUpdate = tg.id
  try {
    await ofetch(`${apiBase}/tags/${tg.id}`, { method: 'PUT', body: { name: tg.name.trim() } })
  } finally { busy.tagUpdate = false }
}
async function deleteTag(id: number) {
  if (!confirm('Delete this tag?')) return
  busy.tagDelete = id
  try {
    await ofetch(`${apiBase}/tags/${id}`, { method: 'DELETE' })
    tags.value = tags.value.filter(t => t.id !== id)
    tasks.value = tasks.value.map(task => ({ ...task, tags: task.tags.filter(t => t.id !== id) }))
  } finally { busy.tagDelete = false }
}

/* ===== Assign / Unassign ===== */
async function toggleAssign(task: Task, tagId: number, ev: Event) {
  const checked = (ev.target as HTMLInputElement).checked
  if (checked) {
    await ofetch(`${apiBase}/todoitems/${task.id}/tags/add`, { method: 'PATCH', body: [tagId] })
    const tag = tags.value.find(t => t.id === tagId)
    if (tag && !task.tags.some(t => t.id === tagId)) task.tags.push(tag)
  } else {
    await ofetch(`${apiBase}/todoitems/${task.id}/tags/remove`, { method: 'PATCH', body: [tagId] })
    task.tags = task.tags.filter(t => t.id !== tagId)
  }
}
async function unassign(taskId: number, tagId: number) {
  await ofetch(`${apiBase}/todoitems/${taskId}/tags/remove`, { method: 'PATCH', body: [tagId] })
  const task = tasks.value.find(t => t.id === taskId)
  if (task) task.tags = task.tags.filter(t => t.id !== tagId)
}
</script>

<style scoped>
.input { @apply rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm outline-none focus:border-indigo-400 focus:ring-2 focus:ring-indigo-100; }
.btn { @apply inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm hover:border-indigo-400 disabled:opacity-60; }
.btn-ghost { @apply border-transparent hover:border-slate-200; }
.btn-danger { @apply border-rose-300 bg-rose-500 text-white hover:bg-rose-600; }
.badge { @apply inline-flex items-center gap-2 rounded-full border border-indigo-200 bg-indigo-50 px-2 py-0.5 text-xs text-indigo-700; }
.badge-x { @apply rounded-full px-1 text-indigo-700 hover:bg-indigo-100; }
</style>
