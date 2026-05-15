<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-gray-100">
    <AppHeader />
    <main class="container mx-auto px-4 py-6 max-w-7xl">
      <slot />
    </main>
    <AppFooter />
  </div>
</template>