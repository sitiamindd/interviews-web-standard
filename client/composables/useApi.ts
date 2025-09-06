// composables/useApi.ts
import { useRuntimeConfig } from 'nuxt/app'
import { ofetch } from 'ofetch'

export const useApi = () => {
  // read from runtime config; fall back to localhost
  const cfg = useRuntimeConfig()
  const apiBase = (cfg.public?.apiBase as string) || 'http://localhost:5113'
  // optional: sanity log
  // console.log('API base:', apiBase)

  // create a preconfigured fetch
  const api = ofetch.create({
    baseURL: apiBase,                               // <-- the line that was erroring
    headers: { 'Content-Type': 'application/json' }
  })

  return {
    // Tasks
    listTasks: () => api('/todoitems'),
    getTask: (id: number) => api(`/todoitems/${id}`),
    createTask: (body: { name: string; isComplete: boolean }) =>
      api('/todoitems', { method: 'POST', body }),
    updateTask: (id: number, body: { name: string; isComplete: boolean }) =>
      api(`/todoitems/${id}`, { method: 'PUT', body }),
    deleteTask: (id: number) =>
      api(`/todoitems/${id}`, { method: 'DELETE' }),
    addTagsToTask: (id: number, tagIds: number[]) =>
      api(`/todoitems/${id}/tags/add`, { method: 'PATCH', body: tagIds }),
    removeTagsFromTask: (id: number, tagIds: number[]) =>
      api(`/todoitems/${id}/tags/remove`, { method: 'PATCH', body: tagIds }),

    // Tags
    listTags: () => api('/tags'),
    createTag: (body: { name: string }) =>
      api('/tags', { method: 'POST', body }),
    deleteTag: (id: number) =>
      api(`/tags/${id}`, { method: 'DELETE' }),
  }
}
