<template>
  <div class="transactions">
    <h1>交易记录</h1>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <table v-else class="transactions-table">
      <thead>
        <tr>
          <th>日期</th>
          <th>代码</th>
          <th>类型</th>
          <th>数量</th>
          <th>价格</th>
          <th>金额</th>
          <th>账户</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="txn in transactions" :key="txn.id">
          <td>{{ formatDate(txn.tradeDate) }}</td>
          <td>{{ txn.symbol }}</td>
          <td :class="txn.type">{{ txn.type === 'buy' ? '买入' : '卖出' }}</td>
          <td>{{ txn.quantity }}</td>
          <td>¥{{ txn.price.toFixed(2) }}</td>
          <td>¥{{ txn.amount.toFixed(2) }}</td>
          <td>{{ getAccountName(txn.accountId) }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { usePortfolioStore } from '@/stores/portfolio'
import type { Account } from '@/api'

const portfolioStore = usePortfolioStore()

const loading = computed(() => portfolioStore.loading)
const error = computed(() => portfolioStore.error)
const transactions = computed(() => portfolioStore.transactions)
const accounts = computed(() => portfolioStore.accounts as Account[])

const formatDate = (date: string): string => {
  return new Date(date).toLocaleDateString('zh-CN')
}

const getAccountName = (accountId: number): string => {
  const account = accounts.value.find((a: Account) => a.id === accountId)
  return account?.name || `账户 ${accountId}`
}

onMounted(async () => {
  await portfolioStore.fetchAccounts()
  portfolioStore.fetchTransactions()
})
</script>

<style scoped>
.transactions {
  padding: 20px;
}

.transactions-table {
  width: 100%;
  border-collapse: collapse;
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.transactions-table th,
.transactions-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #eee;
}

.transactions-table th {
  background: #f5f5f5;
  font-weight: 600;
}

.transactions-table tr:hover {
  background: #f9f9f9;
}

.transactions-table td.buy {
  color: #f56c6c;
}

.transactions-table td.sell {
  color: #67c23a;
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
