<script setup lang="ts">
const colorMode = useColorMode()
const route = useRoute()

const isDark = computed(() => colorMode.value === 'dark')

const toggleDark = () => {
  colorMode.preference = isDark.value ? 'light' : 'dark'
}

const navItems = [
  { to: '/', label: 'Accueil', icon: 'heroicons:home' },
  { to: '/search', label: 'Recherche', icon: 'heroicons:magnifying-glass' },
  { to: '/library', label: 'Bibliothèque', icon: 'heroicons:bookmark' },
  { to: '/calendar', label: 'Calendrier', icon: 'heroicons:calendar-days' }
]
</script>

<template>
  <header class="border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 sticky top-0 z-50">
    <div class="container mx-auto px-4 max-w-7xl">
      <div class="flex items-center justify-between h-16">
        <NuxtLink to="/" class="flex items-center gap-2 font-bold text-xl">
          <Icon name="heroicons:film" class="text-primary-500" />
          <span>MediaHub</span>
        </NuxtLink>

        <nav class="hidden md:flex items-center gap-1">
          <NuxtLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition"
            :class="route.path === item.to
              ? 'bg-primary-100 dark:bg-primary-700/20 text-primary-700 dark:text-primary-400'
              : 'hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-700 dark:text-gray-300'"
          >
            <Icon :name="item.icon" />
            {{ item.label }}
          </NuxtLink>
        </nav>

        <div class="flex items-center gap-2">
          <ClientOnly>
            <button
              class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800"
              :aria-label="isDark ? 'Mode clair' : 'Mode sombre'"
              @click="toggleDark"
            >
              <Icon :name="isDark ? 'heroicons:sun' : 'heroicons:moon'" />
            </button>
            <template #fallback>
              <div class="w-9 h-9"></div>
            </template>
          </ClientOnly>
          <NuxtLink
            to="/login"
            class="px-4 py-2 rounded-lg bg-primary-600 hover:bg-primary-700 text-white text-sm font-medium"
          >
            Connexion
          </NuxtLink>
        </div>
      </div>
    </div>
  </header>
</template>