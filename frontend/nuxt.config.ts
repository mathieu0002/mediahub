import { defineNuxtConfig } from 'nuxt/config'

export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },

  modules: [
    '@nuxtjs/tailwindcss',
    ['@nuxtjs/color-mode', {
      classSuffix: '',
      preference: 'system',
      fallback: 'light'
    }],
    '@pinia/nuxt',
    '@vueuse/nuxt',
    '@nuxt/icon'
  ],

  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5087'
    }
  },

  app: {
    head: {
      title: 'MediaHub - Multi-media tracker',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Suivez vos animes, mangas, films et séries en un seul endroit' }
      ]
    }
  },

  typescript: {
    strict: true,
    typeCheck: false
  }
})