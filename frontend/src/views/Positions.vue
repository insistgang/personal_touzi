<template>
  <div class="positions">
    <h1>持仓管理</h1>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <PositionTable v-else :positions="positions" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { usePortfolioStore } from '@/stores/portfolio'
import PositionTable from '@/components/PositionTable.vue'

const portfolioStore = usePortfolioStore()

const loading = computed(() => portfolioStore.loading)
const error = computed(() => portfolioStore.error)
const positions = computed(() => portfolioStore.positions)

onMounted(() => {
  portfolioStore.fetchPositions()
})
</script>

<style scoped>
.positions {
  padding: 20px;
}

.loading, .error {
  text-align: center;
  padding: 40px;
  font-size: 16px;
}

.error {
  color: #f56c6c;
}
</style>
