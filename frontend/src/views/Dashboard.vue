<template>
  <div class="dashboard">
    <h1>投资组合仪表盘</h1>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else-if="dashboardData" class="dashboard-content">
      <!-- 概览卡片 -->
      <div class="summary-cards">
        <div class="card">
          <h3>总资产</h3>
          <p class="value">¥{{ formatAmount(totalAssets) }}</p>
        </div>
        <div class="card">
          <h3>总盈亏</h3>
          <p class="value" :class="{ positive: totalGainLoss >= 0, negative: totalGainLoss < 0 }">
            ¥{{ formatAmount(totalGainLoss) }} ({{ totalGainLossPercent.toFixed(2) }}%)
          </p>
        </div>
        <div class="card">
          <h3>现金</h3>
          <p class="value">¥{{ formatAmount(dashboardData.cash) }}</p>
        </div>
        <div class="card">
          <h3>持仓市值</h3>
          <p class="value">¥{{ formatAmount(dashboardData.positionsValue) }}</p>
        </div>
      </div>

      <!-- 图表区域 -->
      <div class="charts-section">
        <div class="chart-container">
          <h3>净值走势</h3>
          <NetValueChart :data="dashboardData.netValueTrend" />
        </div>
        <div class="chart-container">
          <h3>资产分布</h3>
          <AssetPieChart :data="dashboardData.assetDistribution" />
        </div>
      </div>

      <!-- 持仓概览 -->
      <div class="positions-section">
        <h3>持仓概览</h3>
        <PositionTable :positions="dashboardData.topPositions" />
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

onMounted(() => {
  portfolioStore.fetchDashboard()
})
</script>

<style scoped>
.dashboard {
  padding: 20px;
}

.summary-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.card h3 {
  margin: 0 0 10px;
  font-size: 14px;
  color: #666;
}

.card .value {
  margin: 0;
  font-size: 24px;
  font-weight: bold;
  color: #333;
}

.card .value.positive {
  color: #f56c6c;
}

.card .value.negative {
  color: #67c23a;
}

.charts-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.chart-container {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.chart-container h3 {
  margin: 0 0 15px;
  font-size: 16px;
}

.positions-section {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.positions-section h3 {
  margin: 0 0 15px;
  font-size: 16px;
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
