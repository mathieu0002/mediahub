<script setup lang="ts">
useHead({ title: 'MediaHub - Accueil' })

const features = [
  {
    icon: 'heroicons:film',
    title: 'Multi-média',
    description: 'Anime, manga, films et séries dans une seule app'
  },
  {
    icon: 'heroicons:tv',
    title: 'Multi-plateformes',
    description: 'Sache où regarder chaque œuvre (Netflix, Crunchyroll, Disney+...)'
  },
  {
    icon: 'heroicons:calendar-days',
    title: 'Calendrier',
    description: 'Les sorties à venir pour les œuvres en cours'
  }
]
</script>

<template>
  <div class="space-y-12 py-8">
    <section class="text-center space-y-4">
      <h1 class="text-5xl font-bold tracking-tight">
        Suivez tous vos
        <span class="text-primary-500">médias</span>
        en un seul endroit
      </h1>
      <p class="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
        Animes, mangas, films, séries. Centralisez votre suivi avec MediaHub
        et n'ayez plus jamais à vous demander où en êtes-vous.
      </p>
      <div class="flex gap-4 justify-center pt-4">
        <NuxtLink
          to="/register"
          class="px-6 py-3 bg-primary-600 hover:bg-primary-700 text-white rounded-lg font-medium"
        >
          Commencer
        </NuxtLink>
        <NuxtLink
          to="/search"
          class="px-6 py-3 border border-gray-300 dark:border-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg font-medium"
        >
          Explorer sans compte
        </NuxtLink>
      </div>
    </section>

    <section class="grid md:grid-cols-3 gap-6">
      <div
        v-for="feature in features"
        :key="feature.title"
        class="p-6 bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-800"
      >
        <div class="w-12 h-12 rounded-lg bg-primary-100 dark:bg-primary-700/20 flex items-center justify-center text-primary-500 mb-4">
          <Icon :name="feature.icon" class="text-2xl" />
        </div>
        <h3 class="text-lg font-semibold mb-2">{{ feature.title }}</h3>
        <p class="text-gray-600 dark:text-gray-400">{{ feature.description }}</p>
      </div>
    </section>
  </div>
</template>