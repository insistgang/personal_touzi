import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  apiPortfolio,
  type Account,
  type AccountPayload,
  type CsvImportPayload,
  type DashboardData,
  type Position,
  type PositionPayload,
  type PositionUpdatePayload,
  type Transaction,
  type TransactionPayload
} from '@/api'

export const usePortfolioStore = defineStore('portfolio', () => {
  const dashboardData = ref<DashboardData | null>(null)
  const positions = ref<Position[]>([])
  const transactions = ref<Transaction[]>([])
  const accounts = ref<Account[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const totalAssets = computed(() => dashboardData.value?.totalAssets ?? 0)
  const totalGainLoss = computed(() => dashboardData.value?.totalGainLoss ?? 0)
  const totalGainLossPercent = computed(() => dashboardData.value?.totalGainLossPercent ?? 0)

  const withLoading = async <T>(task: () => Promise<T>) => {
    loading.value = true
    error.value = null

    try {
      return await task()
    } catch (err: any) {
      error.value = err.response?.data?.error || err.response?.data?.message || err.message || '请求失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  const fetchDashboard = () => withLoading(async () => {
    dashboardData.value = await apiPortfolio.getDashboard()
    return dashboardData.value
  })

  const fetchPositions = (accountId?: number) => withLoading(async () => {
    positions.value = await apiPortfolio.getPositions(accountId)
    return positions.value
  })

  const fetchTransactions = (params?: { accountId?: number }) => withLoading(async () => {
    transactions.value = await apiPortfolio.getTransactions(params)
    return transactions.value
  })

  const fetchAccounts = () => withLoading(async () => {
    accounts.value = await apiPortfolio.getAccounts()
    return accounts.value
  })

  const refreshOverview = async () => {
    await fetchDashboard()
    await fetchAccounts()
  }

  const addPosition = (position: PositionPayload) => withLoading(async () => {
    const newPosition = await apiPortfolio.createPosition(position)
    positions.value = [newPosition, ...positions.value.filter(item => item.id !== newPosition.id)]
    await refreshOverview()
    return newPosition
  })

  const updatePosition = (id: number, position: PositionUpdatePayload) => withLoading(async () => {
    const updated = await apiPortfolio.updatePosition(id, position)
    positions.value = positions.value.map(item => item.id === id ? updated : item)
    await refreshOverview()
    return updated
  })

  const deletePosition = (id: number) => withLoading(async () => {
    await apiPortfolio.deletePosition(id)
    positions.value = positions.value.filter(item => item.id !== id)
    await refreshOverview()
  })

  const addTransaction = (transaction: TransactionPayload) => withLoading(async () => {
    const newTransaction = await apiPortfolio.createTransaction(transaction)
    transactions.value = [newTransaction, ...transactions.value.filter(item => item.id !== newTransaction.id)]
    dashboardData.value = await apiPortfolio.getDashboard()
    accounts.value = await apiPortfolio.getAccounts()
    positions.value = await apiPortfolio.getPositions()
    return newTransaction
  })

  const importInitialPositions = (payload: CsvImportPayload) => withLoading(async () => {
    const result = await apiPortfolio.importInitialPositions(payload)
    dashboardData.value = await apiPortfolio.getDashboard()
    accounts.value = await apiPortfolio.getAccounts()
    positions.value = await apiPortfolio.getPositions()
    return result
  })

  const importTransactions = (payload: CsvImportPayload) => withLoading(async () => {
    const result = await apiPortfolio.importTransactions(payload)
    dashboardData.value = await apiPortfolio.getDashboard()
    accounts.value = await apiPortfolio.getAccounts()
    positions.value = await apiPortfolio.getPositions()
    transactions.value = await apiPortfolio.getTransactions()
    return result
  })

  const updateTransaction = (id: number, transaction: Partial<TransactionPayload>) => withLoading(async () => {
    const updated = await apiPortfolio.updateTransaction(id, transaction)
    transactions.value = transactions.value.map(item => item.id === id ? updated : item)
    return updated
  })

  const deleteTransaction = (id: number) => withLoading(async () => {
    await apiPortfolio.deleteTransaction(id)
    transactions.value = transactions.value.filter(item => item.id !== id)
  })

  const addAccount = (account: AccountPayload) => withLoading(async () => {
    const newAccount = await apiPortfolio.createAccount(account)
    accounts.value = [newAccount, ...accounts.value.filter(item => item.id !== newAccount.id)]
    await fetchDashboard()
    return newAccount
  })

  const updateAccount = (id: number, account: AccountPayload) => withLoading(async () => {
    const updated = await apiPortfolio.updateAccount(id, account)
    accounts.value = accounts.value.map(item => item.id === id ? updated : item)
    await fetchDashboard()
    return updated
  })

  const deleteAccount = (id: number) => withLoading(async () => {
    await apiPortfolio.deleteAccount(id)
    accounts.value = accounts.value.filter(item => item.id !== id)
    await fetchDashboard()
  })

  return {
    dashboardData,
    positions,
    transactions,
    accounts,
    loading,
    error,
    totalAssets,
    totalGainLoss,
    totalGainLossPercent,
    fetchDashboard,
    fetchPositions,
    fetchTransactions,
    fetchAccounts,
    refreshOverview,
    addPosition,
    updatePosition,
    deletePosition,
    addTransaction,
    importInitialPositions,
    importTransactions,
    updateTransaction,
    deleteTransaction,
    addAccount,
    updateAccount,
    deleteAccount
  }
})
