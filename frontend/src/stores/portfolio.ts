import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiPortfolio, type Position, type Transaction, type Account, type DashboardData } from '@/api'

export const usePortfolioStore = defineStore('portfolio', () => {
  // State
  const dashboardData = ref<DashboardData | null>(null)
  const positions = ref<Position[]>([])
  const transactions = ref<Transaction[]>([])
  const accounts = ref<Account[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Computed
  const totalAssets = computed(() => dashboardData.value?.totalAssets ?? 0)
  const totalGainLoss = computed(() => dashboardData.value?.totalGainLoss ?? 0)
  const totalGainLossPercent = computed(() => dashboardData.value?.totalGainLossPercent ?? 0)

  // Actions
  const fetchDashboard = async () => {
    loading.value = true
    error.value = null
    try {
      dashboardData.value = await apiPortfolio.getDashboard()
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch dashboard data'
      throw err
    } finally {
      loading.value = false
    }
  }

  const fetchPositions = async (accountId?: number) => {
    loading.value = true
    error.value = null
    try {
      positions.value = await apiPortfolio.getPositions(accountId)
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch positions'
      throw err
    } finally {
      loading.value = false
    }
  }

  const fetchTransactions = async (params?: { accountId?: number; startDate?: string; endDate?: string }) => {
    loading.value = true
    error.value = null
    try {
      transactions.value = await apiPortfolio.getTransactions(params)
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch transactions'
      throw err
    } finally {
      loading.value = false
    }
  }

  const fetchAccounts = async () => {
    loading.value = true
    error.value = null
    try {
      accounts.value = await apiPortfolio.getAccounts()
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch accounts'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    dashboardData,
    positions,
    transactions,
    accounts,
    loading,
    error,
    // Computed
    totalAssets,
    totalGainLoss,
    totalGainLossPercent,
    // Actions
    fetchDashboard,
    fetchPositions,
    fetchTransactions,
    fetchAccounts
  }
})
