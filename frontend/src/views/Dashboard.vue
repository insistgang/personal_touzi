<template>
  <div class="dashboard">
    <div class="page-header">
      <div class="header-content">
        <h1>投资组合仪表盘</h1>
        <p class="subtitle">实时监控您的投资组合表现</p>
      </div>
      <div class="header-actions">
        <button class="btn-primary" @click="refreshData">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M23 4v6h-6M1 20v-6h6"/>
            <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"/>
          </svg>
          刷新数据
        </button>
      </div>
    </div>

    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>加载中...</p>
    </div>
    <div v-else-if="error" class="error-container">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <p>{{ error }}</p>
    </div>
    <div v-else-if="dashboardData" class="dashboard-content">
      <!-- 概览卡片 -->
      <div class="summary-cards">
        <div class="card gradient-card-1">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M12 2v20M17 7l-5-5-5 5M17 17l-5 5-5-5"/>
            </svg>
          </div>
          <div class="card-content">
            <h3>总资产</h3>
            <p class="value">¥{{ formatAmount(totalAssets) }}</p>
            <span class="trend positive">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="currentColor">
                <path d="M7 14l5-5 5 5H7z"/>
              </svg>
              +2.5% 本月
            </span>
          </div>
        </div>

        <div class="card gradient-card-2">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
            </svg>
          </div>
          <div class="card-content">
            <h3>总盈亏</h3>
            <p class="value" :class="{ positive: totalGainLoss >= 0, negative: totalGainLoss < 0 }">
              ¥{{ formatAmount(totalGainLoss) }}
            </p>
            <span class="trend" :class="totalGainLoss >= 0 ? 'positive' : 'negative'">
              <svg :width="12" :height="12" viewBox="0 0 24 24" fill="currentColor">
                <path :d="totalGainLoss >= 0 ? 'M7 14l5-5 5 5H7z' : 'M17 10l-5 5-5-5H17z'"/>
              </svg>
              {{ totalGainLossPercent.toFixed(2) }}%
            </span>
          </div>
        </div>

        <div class="card gradient-card-3">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="2" y="6" width="20" height="12" rx="2"/>
              <circle cx="12" cy="12" r="2"/>
              <path d="M6 12h.01M18 12h.01"/>
            </svg>
          </div>
          <div class="card-content">
            <h3>现金余额</h3>
            <p class="value">¥{{ formatAmount(dashboardData.cash) }}</p>
            <span class="trend neutral">可用资金</span>
          </div>
        </div>

        <div class="card gradient-card-4">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M3 3v18h18"/>
              <path d="M18 9l-5 5-4-4-3 3"/>
            </svg>
          </div>
          <div class="card-content">
            <h3>持仓市值</h3>
            <p class="value">¥{{ formatAmount(dashboardData.positionsValue) }}</p>
            <span class="trend neutral">{{ Object.keys(dashboardData.topPositions || {}).length }} 个持仓</span>
          </div>
        </div>
      </div>

      <!-- 图表区域 -->
      <div class="charts-section">
        <div class="chart-card">
          <div class="chart-header">
            <h3>
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M3 3v18h18"/>
                <path d="M18 9l-5 5-4-4-3 3"/>
              </svg>
              净值走势
            </h3>
            <span class="chart-badge">近30天</span>
          </div>
          <div class="chart-body">
            <NetValueChart :data="dashboardData.netValueTrend" />
          </div>
        </div>

        <div class="chart-card">
          <div class="chart-header">
            <h3>
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21.21 15.89A10 10 0 1 1 8 2.83"/>
                <path d="M22 12A10 10 0 0 0 12 2v10z"/>
              </svg>
              资产分布
            </h3>
            <span class="chart-badge">按类别</span>
          </div>
          <div class="chart-body">
            <AssetPieChart :data="dashboardData.assetDistribution" />
          </div>
        </div>
      </div>

      <!-- 持仓概览 -->
      <div class="positions-card">
        <div class="positions-header">
          <h3>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
              <circle cx="9" cy="7" r="4"/>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/>
            </svg>
            持仓概览
          </h3>
          <router-link to="/positions" class="view-all-link">
            查看全部
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M5 12h14M12 5l7 7-7 7"/>
            </svg>
          </router-link>
        </div>
        <div class="positions-body">
          <PositionTable :positions="dashboardData.topPositions" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { usePortfolioStore } from '@/stores/portfolio'
import NetValueChart from '@/components/NetValueChart.vue'
import AssetPieChart from '@/components/AssetPieChart.vue'
import PositionTable from '@/components/PositionTable.vue'

const portfolioStore = usePortfolioStore()

const dashboardData = computed(() => portfolioStore.dashboardData)
const loading = computed(() => portfolioStore.loading)
const error = computed(() => portfolioStore.error)
const totalAssets = computed(() => portfolioStore.totalAssets)
const totalGainLoss = computed(() => portfolioStore.totalGainLoss)
const totalGainLossPercent = computed(() => portfolioStore.totalGainLossPercent)

const formatAmount = (value: number): string => {
  return value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const refreshData = () => {
  portfolioStore.fetchDashboard()
}

onMounted(() => {
  portfolioStore.fetchDashboard()
})
</script>

<style scoped>
.dashboard {
  padding: 30px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  flex-wrap: wrap;
  gap: 20px;
}

.header-content h1 {
  font-size: 28px;
  font-weight: 700;
  color: #1a1a2e;
  margin: 0 0 6px 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.subtitle {
  margin: 0;
  color: #6b7280;
  font-size: 14px;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  padding: 12px 24px;
  border-radius: 12px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 14px rgba(102, 126, 234, 0.4);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
}

.btn-primary:active {
  transform: translateY(0);
}

.summary-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 24px;
  margin-bottom: 30px;
}

.card {
  background: #fff;
  border-radius: 20px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  display: flex;
  align-items: flex-start;
  gap: 16px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
}

.card::before {
  content: '';
  position: absolute;
  top: 0;
  right: 0;
  width: 120px;
  height: 120px;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.2) 0%, rgba(255, 255, 255, 0) 100%);
  border-radius: 50%;
  transform: translate(30%, -30%);
}

.card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
}

.gradient-card-1 {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.gradient-card-2 {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}

.gradient-card-3 {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
}

.gradient-card-4 {
  background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
}

.card-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  flex-shrink: 0;
  backdrop-filter: blur(10px);
}

.card-content {
  flex: 1;
}

.card-content h3 {
  margin: 0 0 8px;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.85);
  font-weight: 500;
}

.card-content .value {
  margin: 0 0 8px;
  font-size: 26px;
  font-weight: 700;
  color: #fff;
  line-height: 1.2;
}

.card-content .value.positive {
  color: #a7f3d0;
}

.card-content .value.negative {
  color: #fca5a5;
}

.trend {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.85);
}

.trend svg {
  flex-shrink: 0;
}

.charts-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(450px, 1fr));
  gap: 24px;
  margin-bottom: 30px;
}

.chart-card {
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #f0f0f0;
}

.chart-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #1a1a2e;
  display: flex;
  align-items: center;
  gap: 8px;
}

.chart-badge {
  padding: 6px 12px;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
  color: #667eea;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

.chart-body {
  padding: 24px;
  min-height: 280px;
}

.positions-card {
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.positions-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #f0f0f0;
}

.positions-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #1a1a2e;
  display: flex;
  align-items: center;
  gap: 8px;
}

.view-all-link {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #667eea;
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s;
}

.view-all-link:hover {
  gap: 10px;
}

.positions-body {
  padding: 24px;
}

.loading-container,
.error-container {
  text-align: center;
  padding: 60px 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
}

.error-container {
  color: #ef4444;
}

.error-container svg {
  color: #ef4444;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 3px solid #f3f3f3;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 1200px) {
  .charts-section {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .dashboard {
    padding: 20px;
  }

  .summary-cards {
    grid-template-columns: 1fr;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .header-content h1 {
    font-size: 24px;
  }
}
</style>
