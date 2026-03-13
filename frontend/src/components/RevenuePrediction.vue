<template>
  <div class="revenue-prediction">
    <div class="prediction-header">
      <h3>收益预测</h3>
      <div class="controls">
        <select v-model="selectedDays" @change="fetchPrediction" class="days-select">
          <option :value="7">未来 7 天</option>
          <option :value="30">未来 30 天</option>
          <option :value="90">未来 90 天</option>
        </select>
        <button @click="fetchPrediction" :disabled="loading" class="refresh-btn">
          刷新
        </button>
      </div>
    </div>

    <div v-if="loading && !predictionData" class="loading">
      加载中...
    </div>

    <div v-else-if="error" class="error">
      {{ error }}
    </div>

    <template v-else-if="predictionData">
      <div class="prediction-summary">
        <div class="summary-card">
          <div class="label">预期收益</div>
          <div class="value" :class="predictionData.expectedReturn >= 0 ? 'positive' : 'negative'">
            {{ formatPercent(predictionData.expectedReturn) }}
          </div>
        </div>
        <div class="summary-card">
          <div class="label">置信区间</div>
          <div class="value range">
            {{ formatPercent(predictionData.lowerBound) }} ~ {{ formatPercent(predictionData.upperBound) }}
          </div>
        </div>
        <div class="summary-card">
          <div class="label">风险等级</div>
          <div class="value risk" :class="riskLevelClass">
            {{ riskLevel }}
          </div>
        </div>
      </div>

      <div class="chart-container">
        <v-chart class="chart" :option="chartOption" autoresize />
      </div>

      <div class="risk-warning">
        <div class="warning-title">
          <span class="icon">⚠️</span> 风险提示
        </div>
        <ul class="warning-list">
          <li v-for="(warning, index) in predictionData.warnings" :key="index">
            {{ warning }}
          </li>
          <li>预测基于历史数据和 AI 模型，仅供参考，不构成投资建议</li>
          <li>市场存在不确定性，实际收益可能偏离预测值</li>
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
  lowerBound: number[]
  upperBound: number[]
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
  const volatility = predictionData.value.upperBound - predictionData.value.lowerBound
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
      formatter: (params: any) => {
        let result = `${params[0].name}<br/>`
        params.forEach((p: any) => {
          result += `${p.seriesName}: ¥${p.value.toFixed(2)}<br/>`
        })
        return result
      }
    },
    legend: {
      data: ['预测值', '上限', '下限'],
      bottom: 0
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
      boundaryGap: false
    },
    yAxis: {
      type: 'value',
      axisLabel: {
        formatter: '¥{value}'
      }
    },
    series: [
      {
        name: '上限',
        type: 'line',
        data: predictionData.value.upperBound,
        lineStyle: { type: 'dashed', color: '#91cc75' },
        itemStyle: { color: '#91cc75' },
        showSymbol: false
      },
      {
        name: '下限',
        type: 'line',
        data: predictionData.value.lowerBound,
        lineStyle: { type: 'dashed', color: '#ee6666' },
        itemStyle: { color: '#ee6666' },
        showSymbol: false
      },
      {
        name: '预测值',
        type: 'line',
        data: predictionData.value.predictedValues,
        smooth: true,
        lineStyle: { color: '#5470c6', width: 3 },
        itemStyle: { color: '#5470c6' },
        areaStyle: {
          color: {
            type: 'linear',
            x: 0,
            y: 0,
            x2: 0,
            y2: 1,
            colorStops: [
              { offset: 0, color: 'rgba(84, 112, 198, 0.3)' },
              { offset: 1, color: 'rgba(84, 112, 198, 0.05)' }
            ]
          }
        }
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
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 20px;
}

.prediction-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.prediction-header h3 {
  margin: 0;
  font-size: 16px;
  color: #333;
}

.controls {
  display: flex;
  gap: 8px;
}

.days-select {
  padding: 6px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
}

.refresh-btn {
  padding: 6px 16px;
  background: #409eff;
  color: #fff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s;
}

.refresh-btn:hover:not(:disabled) {
  background: #66b1ff;
}

.refresh-btn:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.loading, .error {
  text-align: center;
  padding: 40px;
  color: #999;
}

.error {
  color: #f56c6c;
}

.prediction-summary {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 20px;
}

.summary-card {
  background: #f9f9f9;
  padding: 16px;
  border-radius: 8px;
  text-align: center;
}

.summary-card .label {
  font-size: 12px;
  color: #999;
  margin-bottom: 8px;
}

.summary-card .value {
  font-size: 20px;
  font-weight: bold;
}

.summary-card .value.positive {
  color: #67c23a;
}

.summary-card .value.negative {
  color: #f56c6c;
}

.summary-card .value.range {
  font-size: 16px;
  color: #409eff;
}

.summary-card .value.risk {
  font-size: 16px;
}

.summary-card .value.risk-low {
  color: #67c23a;
}

.summary-card .value.risk-medium {
  color: #e6a23c;
}

.summary-card .value.risk-high {
  color: #f56c6c;
}

.chart-container {
  margin-bottom: 20px;
}

.chart {
  width: 100%;
  height: 300px;
}

.risk-warning {
  background: #fef0f0;
  border-left: 4px solid #f56c6c;
  padding: 16px;
  border-radius: 4px;
}

.warning-title {
  font-weight: bold;
  color: #f56c6c;
  margin-bottom: 8px;
  display: flex;
  align-items: center;
  gap: 4px;
}

.warning-list {
  margin: 0;
  padding-left: 20px;
  color: #666;
  font-size: 13px;
}

.warning-list li {
  margin-bottom: 4px;
}
</style>
