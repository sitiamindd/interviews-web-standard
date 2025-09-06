// https://nuxt.com/docs/api/configuration/nuxt-config
/*export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true }
})
*/

/*
export default defineNuxtConfig({
  runtimeConfig: {
    public: {
      // default; will be overridden by .env NUXT_PUBLIC_API_BASE
      apiBase: 'http://localhost:5000'
    }
  }
})
*/

export default defineNuxtConfig({
  devtools: { enabled: true },
  modules: ['@nuxtjs/tailwindcss'],
  runtimeConfig: {
    public: {
      apiBase:  'http://localhost:5113'
    }
  }
})
