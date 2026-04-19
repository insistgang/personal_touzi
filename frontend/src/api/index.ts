import axios from 'axios'
import type { AxiosInstance, AxiosResponse } from 'axios'

const api: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

api.interceptors.request.use(
  (config) => config,
  (error) => Promise.reject(error)
)

api.interceptors.response.use(
  (response: AxiosResponse) => response.data,
  (error) => {
    console.error('API Error:', error)
    return Promise.reject(error)
  }
)

export default api

export interface Position {
  id: number
  symbol: string
  name: string
  type: 'stock' | 'fund' | 'bond'
  quantity: number
  avgCost: number
  costPrice: number
  currentPrice: number
  marketValue: number
  gainLoss: number
  gainLossPercent: number
  accountId: number
}

export interface PositionPayload {
  symbol: string
  name: string
  type: 'stock' | 'fund' | 'bond'
  quantity: number
  costPrice: number
  currentPrice: number
  accountId: number
}

export interface PositionUpdatePayload {
  name?: string
  type?: 'stock' | 'fund' | 'bond'
  currentPrice?: number
}

export interface Transaction {
  id: number
  symbol: string
  name: string
  type: 'buy' | 'sell'
  quantity: number
  price: number
  amount: number
  tradeDate: string
  accountId: number
  remark?: string
}

export interface TransactionPayload {
  symbol: string
  name: string
  assetType?: 'stock' | 'fund' | 'bond'
  type: 'buy' | 'sell'
  quantity: number
  price: number
  tradeDate: string
  accountId: number
  remark?: string
}

export interface Account {
  id: number
  name: string
  broker: string
  type: string
  balance: number
  positionsValue: number
  totalAssets: number
  gainLoss: number
  gainLossPercent: number
  positionCount: number
  createdAt: string
}

export interface AccountPayload {
  name: string
  broker: string
  balance: number
}

export interface AssetSnapshot {
  date: string
  totalAssets: number
  netValue: number
  cash: number
  positionsValue: number
  gainLoss: number
  gainLossPercent: number
}

export interface CsvImportPayload {
  accountId: number
  csvContent: string
  hasHeader: boolean
}

export interface InitialPositionImportResult {
  accountId: number
  importedCount: number
  totalCostBasis: number
  totalMarketValue: number
}

export interface TransactionImportResult {
  accountId: number
  importedCount: number
  buyCount: number
  sellCount: number
  totalAmount: number
}

export interface DashboardData {
  totalAssets: number
  totalGainLoss: number
  totalGainLossPercent: number
  cash: number
  positionsValue: number
  todayGainLoss: number
  netValueTrend: Array<{ date: string; value: number }>
  assetDistribution: Array<{ name: string; value: number }>
  topPositions: Position[]
}

export interface PortfolioAnalysis {
  riskLevel: number
  riskAssessment: string
  suggestions: string[]
  summary: string
}

export interface SentimentFactor {
  name: string
  impact: number
  description: string
}

export interface MarketSentimentData {
  overall: 'bullish' | 'neutral' | 'bearish'
  score: number
  bullish: number
  neutral: number
  bearish: number
  factors: SentimentFactor[]
  advices: string[]
  recommendation: string
}

export interface RevenuePredictionData {
  dates: string[]
  predictedValues: number[]
  lowerBounds: number[]
  upperBounds: number[]
  expectedReturn: number
  lowerBound: number
  upperBound: number
  confidenceLevel: number
  warnings: string[]
  analysis: string
}

export interface InvestmentReport {
  generateTime: string
  period: string
  totalReturn: number
  totalAmount: number
  benchmarkReturn: number
  excessReturn: number
  positionValue: number
  positionCount: number
  concentration: number
  tradeCount: number
  tradeAmount: number
  winRate: number
  profitLossRatio: number
  risks: string[]
  suggestions: string[]
  content: string
}

const toAccountRequest = (account: AccountPayload) => ({
  name: account.name,
  description: account.broker,
  initialCash: account.balance
})

const normalizeReportType = (type: string) => {
  switch (type) {
    case 'month':
    case 'monthly':
      return 'monthly'
    case 'quarter':
    case 'quarterly':
      return 'quarterly'
    case 'year':
    case 'yearly':
      return 'yearly'
    default:
      return 'weekly'
  }
}

export const apiPortfolio = {
  getDashboard: (): Promise<DashboardData> => api.get('/portfolio/dashboard'),

  getPositions: (accountId?: number): Promise<Position[]> => {
    if (accountId) {
      return api.get(`/portfolio/positions/by-account/${accountId}`)
    }
    return api.get('/portfolio/positions')
  },

  createPosition: (position: PositionPayload): Promise<Position> =>
    api.post('/portfolio/positions', position),

  updatePosition: (id: number, position: PositionUpdatePayload): Promise<Position> =>
    api.put(`/portfolio/positions/${id}`, position),

  deletePosition: (id: number): Promise<void> =>
    api.delete(`/portfolio/positions/${id}`),

  getTransactions: (params?: { accountId?: number }): Promise<Transaction[]> => {
    if (params?.accountId) {
      return api.get(`/portfolio/transactions/by-account/${params.accountId}`)
    }
    return api.get('/portfolio/transactions')
  },

  createTransaction: (transaction: TransactionPayload): Promise<Transaction> =>
    api.post('/portfolio/transactions', transaction),

  updateTransaction: (id: number, transaction: Partial<TransactionPayload>): Promise<Transaction> =>
    api.put(`/portfolio/transactions/${id}`, transaction),

  deleteTransaction: (id: number): Promise<void> =>
    api.delete(`/portfolio/transactions/${id}`),

  getAccounts: (): Promise<Account[]> =>
    api.get('/portfolio/accounts'),

  getAccount: (id: number): Promise<Account> =>
    api.get(`/portfolio/accounts/${id}`),

  createAccount: (account: AccountPayload): Promise<Account> =>
    api.post('/portfolio/accounts', toAccountRequest(account)),

  updateAccount: (id: number, account: AccountPayload): Promise<Account> =>
    api.put(`/portfolio/accounts/${id}`, toAccountRequest(account)),

  deleteAccount: (id: number): Promise<void> =>
    api.delete(`/portfolio/accounts/${id}`),

  getSnapshots: (startDate?: string, endDate?: string): Promise<AssetSnapshot[]> =>
    api.get('/portfolio/snapshots', { params: { startDate, endDate } }),

  getNetValueTrend: (days = 30): Promise<Array<{ date: string; value: number }>> =>
    api.get('/portfolio/net-value-trend', { params: { days } }),

  importInitialPositions: (payload: CsvImportPayload): Promise<InitialPositionImportResult> =>
    api.post('/portfolio/imports/initial-positions', payload),

  importTransactions: (payload: CsvImportPayload): Promise<TransactionImportResult> =>
    api.post('/portfolio/imports/transactions', payload)
}

export const apiAI = {
  chat: (message: string): Promise<string> =>
    api.post('/ai/chat', { message }),

  analyzePortfolio: (accountId: number): Promise<PortfolioAnalysis> =>
    api.post(`/ai/analyze/${accountId}`),

  generateReport: (accountId: number, type: string): Promise<InvestmentReport> =>
    api.post(`/ai/report/${accountId}`, { reportType: normalizeReportType(type) }),

  analyzeSentiment: (codes: string[]): Promise<MarketSentimentData> =>
    api.post('/ai/sentiment', { stockCodes: codes }),

  predictRevenue: (accountId: number, days: number): Promise<RevenuePredictionData> =>
    api.post(`/ai/predict/${accountId}`, { days })
}
