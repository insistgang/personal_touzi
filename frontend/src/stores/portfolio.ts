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

  // Position Actions
  const addPosition = async (position: Omit<Position, 'id'>) => {
    loading.value = true
    error.value = null
    try {
      const newPosition = await apiPortfolio.createPosition(position)
      positions.value.push(newPosition)
    } catch (err: any) {
      error.value = err.message || 'Failed to add position'
      throw err
    } finally {
      loading.value = false
    }
  }

  const updatePosition = async (id: number, position: Partial<Position>) => {
    loading.value = true
    error.value = null
    try {
      const updated = await apiPortfolio.updatePosition(id, position)
      const index = positions.value.findIndex(p => p.id === id)
      if (index !== -1) {
        positions.value[index] = updated
      }
    } catch (err: any) {
      error.value = err.message || 'Failed to update position'
      throw err
    } finally {
      loading.value = false
    }
  }

  const deletePosition = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      await apiPortfolio.deletePosition(id)
      positions.value = positions.value.filter(p => p.id !== id)
    } catch (err: any) {
      error.value = err.message || 'Failed to delete position'
      throw err
    } finally {
      loading.value = false
    }
  }

  // Transaction Actions
  const addTransaction = async (transaction: Omit<Transaction, 'id'>) => {
    loading.value = true
    error.value = null
    try {
      const newTransaction = await apiPortfolio.createTransaction(transaction)
      transactions.value.unshift(newTransaction)
    } catch (err: any) {
      error.value = err.message || 'Failed to add transaction'
      throw err
    } finally {
      loading.value = false
    }
  }

  const updateTransaction = async (id: number, transaction: Partial<Transaction>) => {
    loading.value = true
    error.value = null
    try {
      const updated = await apiPortfolio.updateTransaction(id, transaction)
      const index = transactions.value.findIndex(t => t.id === id)
      if (index !== -1) {
        transactions.value[index] = updated
      }
    } catch (err: any) {
      error.value = err.message || 'Failed to update transaction'
      throw err
    } finally {
      loading.value = false
    }
  }

  const deleteTransaction = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      await apiPortfolio.deleteTransaction(id)
      transactions.value = transactions.value.filter(t => t.id !== id)
    } catch (err: any) {
      error.value = err.message || 'Failed to delete transaction'
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
    fetchAccounts,
    addPosition,
    updatePosition,
    deletePosition,
    addTransaction,
    updateTransaction,
    deleteTransaction
  }
})
