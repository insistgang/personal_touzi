import axios from 'axios'
import type { AxiosInstance, AxiosResponse } from 'axios'

const api: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器
api.interceptors.request.use(
  (config) => {
    // 可以在这里添加 token
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
api.interceptors.response.use(
  (response: AxiosResponse) => {
    return response.data
  },
  (error) => {
    console.error('API Error:', error)
    return Promise.reject(error)
  }
)

export default api

// API 接口定义
export interface Position {
  id: number
  symbol: string
  name: string
  quantity: number
  avgCost: number
  currentPrice: number
  marketValue: number
  gainLoss: number
  gainLossPercent: number
  accountId: number
}

export interface Transaction {
  id: number
  symbol: string
  type: 'buy' | 'sell'
  quantity: number
  price: number
  amount: number
  tradeDate: string
  accountId: number
}

export interface Account {
  id: number
  name: string
  broker: string
  type: string
  balance: number
}

export interface AssetSnapshot {
  id: number
  date: string
  totalAssets: number
  netValue: number
  cash: number
  positionsValue: number
  gainLoss: number
  gainLossPercent: number
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

// API 方法
export const apiPortfolio = {
  // 获取仪表盘数据
  getDashboard: (): Promise<DashboardData> => {
    return api.get('/portfolio/dashboard')
  },

  // 获取持仓列表
  getPositions: (accountId?: number): Promise<Position[]> => {
    return api.get('/portfolio/positions', { params: { accountId } })
  },

  // 获取交易记录
  getTransactions: (params?: { accountId?: number; startDate?: string; endDate?: string }): Promise<Transaction[]> => {
    return api.get('/portfolio/transactions', { params })
  },

  // 获取账户列表
  getAccounts: (): Promise<Account[]> => {
    return api.get('/portfolio/accounts')
  },

  // 获取资产快照
  getSnapshots: (startDate?: string, endDate?: string): Promise<AssetSnapshot[]> => {
    return api.get('/portfolio/snapshots', { params: { startDate, endDate } })
  },

  // 获取净值走势
  getNetValueTrend: (days: number = 30): Promise<Array<{ date: string; value: number }>> => {
    return api.get('/portfolio/net-value-trend', { params: { days } })
  }
}

// AI API 接口
export const apiAI = {
  // AI 聊天
  chat: (message: string) => api.post('/ai/chat', { message }),

  // 分析持仓
  analyzePortfolio: (accountId: number) => api.post(`/ai/analyze/${accountId}`),

  // 生成报告
  generateReport: (accountId: number, type: string) => api.post(`/ai/report/${accountId}`, { type }),

  // 市场情绪分析
  analyzeSentiment: (codes: string[]) => api.post('/ai/sentiment', { codes }),

  // 收益预测
  predictRevenue: (accountId: number, days: number) => api.post(`/ai/predict/${accountId}`, { days })
}
