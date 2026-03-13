<template>
  <div class="revenue-prediction">
    <div class="prediction-header">
      <div class="header-left">
        <div class="icon-wrapper">
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path d="M10 2L3 7V17H17V7L10 2Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M10 11V17" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
            <path d="M6 14H14" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
        </div>
        <h3>AI 收益预测</h3>
      </div>
      <div class="controls">
        <div class="select-wrapper">
          <select v-model="selectedDays" @change="fetchPrediction" class="days-select">
            <option :value="7">未来 7 天</option>
            <option :value="30">未来 30 天</option>
            <option :value="90">未来 90 天</option>
          </select>
        </div>
        <button @click="fetchPrediction" :disabled="loading" class="refresh-btn">
          <svg v-if="!loading" width="14" height="14" viewBox="0 0 14 14" fill="none">
            <path d="M7 2.33325V11.6666M7 11.6666L10.5 8.16659M7 11.6666L3.5 8.16659" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M12.8333 7C12.8333 4.23858 10.5947 2 7.83325 2C5.07183 2 2.83325 4.23858 2.83325 7" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
          <span v-else class="spinner"></span>
          刷新
        </button>
      </div>
    </div>

    <div v-if="loading && !predictionData" class="loading">
      <div class="loading-spinner"></div>
      <p>AI 正在分析历史数据，预测未来收益...</p>
    </div>

    <div v-else-if="error" class="error">
      <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
        <circle cx="20" cy="20" r="16" stroke="#f56c6c" stroke-width="2"/>
        <path d="M20 14V22M20 26V27" stroke="#f56c6c" stroke-width="2" stroke-linecap="round"/>
      </svg>
      <p>{{ error }}</p>
    </div>

    <template v-else-if="predictionData">
      <div class="prediction-summary">
        <div class="summary-card main-card" :class="predictionData.expectedReturn >= 0 ? 'positive' : 'negative'">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
              <path d="M12 2V6M12 18V22M6 12H2M22 12H18M19.07 4.93L16.24 7.76M7.76 16.24L4.93 19.07M19.07 19.07L16.24 16.24M7.76 7.76L4.93 4.93" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            </svg>
          </div>
          <div class="card-content">
            <div class="label">预期收益</div>
            <div class="value">{{ formatPercent(predictionData.expectedReturn) }}</div>
          </div>
        </div>
        <div class="summary-card range-card">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
              <path d="M4 12H20M4 12L8 8M4 12L8 16" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
              <path d="M20 12L16 8M20 12L16 16" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
          <div class="card-content">
            <div class="label">置信区间</div>
            <div class="value range">{{ formatPercent(predictionData.lowerBound) }} ~ {{ formatPercent(predictionData.upperBound) }}</div>
          </div>
        </div>
        <div class="summary-card risk-card" :class="riskLevelClass">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
              <path d="M12 9V14M12 18H12.01M5.25 19.5L12 4.5L18.75 19.5H5.25Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
          <div class="card-content">
            <div class="label">风险等级</div>
            <div class="value risk">{{ riskLevel }}</div>
          </div>
        </div>
      </div>

      <div class="chart-container">
        <v-chart class="chart" :option="chartOption" autoresize />
      </div>

      <div class="risk-warning">
        <div class="warning-header">
          <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
            <path d="M9 2.25L1.5 15.75H16.5L9 2.25Z" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round"/>
            <path d="M9 7.5V10.5M9 12.75V13.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
          <span>风险提示</span>
        </div>
        <ul class="warning-list">
          <li v-for="(warning, index) in predictionData.warnings" :key="index">
            <span class="bullet">•</span> {{ warning }}
          </li>
          <li><span class="bullet">•</span> 预测基于历史数据和 AI 模型，仅供参考，不构成投资建议</li>
          <li><span class="bullet">•</span> 市场存在不确定性，实际收益可能偏离预测值</li>
        </ul>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent
} from 'echarts/components'
import { apiAI } from '@/api/index.ts'

use([
  CanvasRenderer,
  LineChart,
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent
])

interface PredictionData {
  dates: string[]
  predictedValues: number[]
  lowerBounds: number[]
  upperBounds: number[]
  expectedReturn: number
  lowerBound: number
  upperBound: number
  warnings: string[]
}

interface Props {
  accountId?: number
}

const props = defineProps<Props>()

const selectedDays = ref(30)
const loading = ref(false)
const error = ref('')
const predictionData = ref<PredictionData | null>(null)

const riskLevel = computed(() => {
  if (!predictionData.value) return '-'
  const volatility = Math.abs(predictionData.value.upperBound - predictionData.value.lowerBound)
  if (volatility < 0.05) return '低风险'
  if (volatility < 0.15) return '中等风险'
  return '高风险'
})

const riskLevelClass = computed(() => {
  const level = riskLevel.value
  if (level === '低风险') return 'risk-low'
  if (level === '中等风险') return 'risk-medium'
  return 'risk-high'
})

const chartOption = computed(() => {
  if (!predictionData.value) return {}

  return {
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#e0e0e0',
      borderWidth: 1,
      textStyle: { color: '#333' },
      formatter: (params: any) => {
        let result = `<div style="margin-bottom: 4px;">${params[0].name}</div>`
        params.forEach((p: any) => {
          result += `<div style="display: flex; align-items: center; gap: 8px;">
            <span style="display: inline-block; width: 10px; height: 10px; background: ${p.color}; border-radius: 50%;"></span>
            <span>${p.seriesName}: ¥${p.value.toFixed(2)}</span>
          </div>`
        })
        return result
      }
    },
    legend: {
      data: ['预测值', '上限', '下限'],
      bottom: 0,
      textStyle: { color: '#666' }
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '15%',
      top: '10%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: predictionData.value.dates,
      boundaryGap: false,
      axisLine: { lineStyle: { color: '#e0e0e0' } },
      axisLabel: { color: '#999' }
    },
    yAxis: {
      type: 'value',
      axisLabel: {
        formatter: '¥{value}',
        color: '#999'
      },
      splitLine: { lineStyle: { color: '#f0f0f0' } }
    },
    series: [
      {
        name: '置信区间',
        type: 'line',
        data: predictionData.value.upperBounds.map((up, i) => [(predictionData.value?.lowerBounds[i] || 0), up]),
        lineStyle: { opacity: 0 },
        itemStyle: { opacity: 0 },
        areaStyle: {
          color: {
            type: 'linear',
            x: 0,
            y: 0,
            x2: 0,
            y2: 1,
            colorStops: [
              { offset: 0, color: 'rgba(102, 126, 234, 0.2)' },
              { offset: 1, color: 'rgba(118, 75, 162, 0.05)' }
            ]
          }
        },
        stack: 'confidence',
        z: 1
      },
      {
        name: '上限',
        type: 'line',
        data: predictionData.value.upperBounds,
        lineStyle: { type: 'dashed', color: '#67c23a', width: 1.5 },
        itemStyle: { color: '#67c23a' },
        showSymbol: false,
        z: 2
      },
      {
        name: '下限',
        type: 'line',
        data: predictionData.value.lowerBounds,
        lineStyle: { type: 'dashed', color: '#f56c6c', width: 1.5 },
        itemStyle: { color: '#f56c6c' },
        showSymbol: false,
        z: 2
      },
      {
        name: '预测值',
        type: 'line',
        data: predictionData.value.predictedValues,
        smooth: true,
        lineStyle: { color: '#667eea', width: 3 },
        itemStyle: { color: '#667eea' },
        areaStyle: {
          color: {
            type: 'linear',
            x: 0,
            y: 0,
            x2: 0,
            y2: 1,
            colorStops: [
              { offset: 0, color: 'rgba(102, 126, 234, 0.3)' },
              { offset: 1, color: 'rgba(102, 126, 234, 0.02)' }
            ]
          }
        },
        z: 3
      }
    ]
  }
})

const formatPercent = (value: number) => {
  return (value * 100).toFixed(2) + '%'
}

const fetchPrediction = async () => {
  loading.value = true
  error.value = ''

  try {
    const response = await apiAI.predictRevenue(props.accountId || 1, selectedDays.value)
    predictionData.value = response.data || response
  } catch (err: any) {
    console.error('Prediction error:', err)
    error.value = err.response?.data?.message || '获取预测数据失败'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchPrediction()
})
</script>

<style scoped>
.revenue-prediction {
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.08);
  padding: 24px;
}

.prediction-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  flex-wrap: wrap;
  gap: 16px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.icon-wrapper {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
}

.prediction-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.controls {
  display: flex;
  gap: 12px;
  align-items: center;
}

.select-wrapper {
  position: relative;
}

.days-select {
  padding: 10px 36px 10px 16px;
  border: 1px solid #e0e0e0;
  border-radius: 10px;
  font-size: 14px;
  cursor: pointer;
  appearance: none;
  background: #f9f9f9;
  color: #333;
  transition: all 0.3s;
}

.select-wrapper::after {
  content: '';
  position: absolute;
  right: 14px;
  top: 50%;
  transform: translateY(-50%);
  width: 0;
  height: 0;
  border-left: 4px solid transparent;
  border-right: 4px solid transparent;
  border-top: 5px solid #667eea;
}

.days-select:hover {
  border-color: #667eea;
}

.days-select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.refresh-btn {
  padding: 10px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 6px;
}

.refresh-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.3);
}

.refresh-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.spinner {
  width: 14px;
  height: 14px;
  border: 2px solid #fff;
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.loading, .error {
  text-align: center;
  padding: 60px 20px;
  color: #999;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  margin: 0 auto 16px;
  border: 3px solid #f0f0f0;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.loading p {
  margin: 0;
  font-size: 14px;
}

.error {
  color: #f56c6c;
}

.error svg {
  margin: 0 auto 12px;
}

.error p {
  margin: 0;
}

.prediction-summary {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}

.summary-card {
  background: #f9f9f9;
  padding: 20px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  gap: 16px;
  transition: all 0.3s;
}

.summary-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.card-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.main-card .card-icon {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 100%);
  color: #667eea;
}

.main-card.positive {
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.08) 0%, rgba(103, 194, 58, 0.03) 100%);
  border: 1px solid rgba(103, 194, 58, 0.2);
}

.main-card.positive .card-icon {
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.15) 0%, rgba(103, 194, 58, 0.05) 100%);
  color: #67c23a;
}

.main-card.negative {
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.08) 0%, rgba(245, 108, 108, 0.03) 100%);
  border: 1px solid rgba(245, 108, 108, 0.2);
}

.main-card.negative .card-icon {
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.15) 0%, rgba(245, 108, 108, 0.05) 100%);
  color: #f56c6c;
}

.range-card .card-icon {
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.15) 0%, rgba(64, 158, 255, 0.05) 100%);
  color: #409eff;
}

.risk-card .card-icon {
  background: linear-gradient(135deg, rgba(230, 162, 60, 0.15) 0%, rgba(230, 162, 60, 0.05) 100%);
  color: #e6a23c;
}

.risk-card.risk-low .card-icon {
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.15) 0%, rgba(103, 194, 58, 0.05) 100%);
  color: #67c23a;
}

.risk-card.risk-high .card-icon {
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.15) 0%, rgba(245, 108, 108, 0.05) 100%);
  color: #f56c6c;
}

.card-content {
  flex: 1;
}

.card-content .label {
  font-size: 12px;
  color: #999;
  margin-bottom: 4px;
}

.card-content .value {
  font-size: 22px;
  font-weight: 700;
  color: #333;
}

.main-card.positive .value {
  color: #67c23a;
}

.main-card.negative .value {
  color: #f56c6c;
}

.card-content .value.range {
  font-size: 16px;
  color: #409eff;
}

.card-content .value.risk {
  font-size: 16px;
  color: #e6a23c;
}

.risk-card.risk-low .value {
  color: #67c23a;
}

.risk-card.risk-high .value {
  color: #f56c6c;
}

.chart-container {
  margin-bottom: 24px;
  background: #fafafa;
  border-radius: 12px;
  padding: 16px;
}

.chart {
  width: 100%;
  height: 320px;
}

.risk-warning {
  background: linear-gradient(135deg, rgba(230, 162, 60, 0.08) 0%, rgba(230, 162, 60, 0.03) 100%);
  border: 1px solid rgba(230, 162, 60, 0.2);
  border-radius: 12px;
  padding: 16px 20px;
}

.warning-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #e6a23c;
  margin-bottom: 12px;
  font-size: 14px;
}

.warning-header svg {
  flex-shrink: 0;
}

.warning-list {
  margin: 0;
  padding: 0;
  list-style: none;
  color: #666;
  font-size: 13px;
  line-height: 1.8;
}

.warning-list li {
  display: flex;
  gap: 8px;
  margin-bottom: 4px;
}

.warning-list .bullet {
  color: #e6a23c;
  font-weight: bold;
}

@media (max-width: 768px) {
  .prediction-summary {
    grid-template-columns: 1fr;
  }

  .controls {
    flex-wrap: wrap;
  }
}
</style>
