<template>
  <div class="accounts">
    <h1>账户管理</h1>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else class="accounts-list">
      <div v-for="account in accounts" :key="account.id" class="account-card">
        <h3>{{ account.name }}</h3>
        <p class="broker">{{ account.broker }} - {{ account.type }}</p>
        <p class="balance">余额: ¥{{ formatAmount(account.balance) }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { usePortfolioStore } from '@/stores/portfolio'

const portfolioStore = usePortfolioStore()

const loading = computed(() => portfolioStore.loading)
const error = computed(() => portfolioStore.error)
const accounts = computed(() => portfolioStore.accounts)

const formatAmount = (value: number): string => {
  return value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

onMounted(() => {
  portfolioStore.fetchAccounts()
})
</script>

<style scoped>
.accounts {
  padding: 20px;
}

.accounts-list {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
}

.account-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.account-card h3 {
  margin: 0 0 10px;
  font-size: 18px;
}

.account-card .broker {
  margin: 5px 0;
  color: #666;
  font-size: 14px;
}

.account-card .balance {
  margin: 10px 0 0;
  font-size: 20px;
  font-weight: bold;
  color: #333;
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
