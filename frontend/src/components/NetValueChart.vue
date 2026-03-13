<template>
  <div class="net-value-chart">
    <div class="chart-header">
      <h3>净值走势</h3>
    </div>
    <div class="chart-container">
      <v-chart class="chart" :option="chartOption" autoresize />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent
} from 'echarts/components'

use([
  CanvasRenderer,
  LineChart,
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent
])

interface DataPoint {
  date: string
  value: number
}

interface Props {
  data: DataPoint[]
}

const props = defineProps<Props>()

const chartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    backgroundColor: 'rgba(255, 255, 255, 0.95)',
    borderColor: '#e0e0e0',
    borderWidth: 1,
    textStyle: { color: '#333' },
    formatter: (params: any) => {
      const param = params[0]
      return `<div style="margin-bottom: 4px;">${param.name}</div>
              <div style="display: flex; align-items: center; gap: 8px;">
                <span style="display: inline-block; width: 10px; height: 10px; background: #667eea; border-radius: 50%;"></span>
                <span>净值: ¥${param.value.toFixed(2)}</span>
              </div>`
    }
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '3%',
    top: '3%',
    containLabel: true
  },
  xAxis: {
    type: 'category',
    data: props.data.map(d => d.date),
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
      name: '净值',
      type: 'line',
      data: props.data.map(d => d.value),
      smooth: true,
      areaStyle: {
        color: {
          type: 'linear',
          x: 0,
          y: 0,
          x2: 0,
          y2: 1,
          colorStops: [
            { offset: 0, color: 'rgba(102, 126, 234, 0.3)' },
            { offset: 1, color: 'rgba(118, 75, 162, 0.05)' }
          ]
        }
      },
      lineStyle: {
        color: '#667eea',
        width: 3
      },
      itemStyle: {
        color: '#667eea'
      }
    }
  ]
}))
</script>

<style scoped>
.net-value-chart {
  width: 100%;
  height: 100%;
}

.chart-header {
  padding: 16px 20px;
  border-bottom: 1px solid #f0f0f0;
}

.chart-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.chart-container {
  padding: 16px;
  height: calc(100% - 55px);
}

.chart {
  width: 100%;
  height: 100%;
}
</style>
