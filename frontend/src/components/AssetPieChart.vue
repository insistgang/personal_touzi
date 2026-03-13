<template>
  <div class="asset-pie-chart">
    <div class="chart-header">
      <h3>资产分布</h3>
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
import { PieChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  LegendComponent
} from 'echarts/components'

use([
  CanvasRenderer,
  PieChart,
  TitleComponent,
  TooltipComponent,
  LegendComponent
])

interface DataPoint {
  name: string
  value: number
}

interface Props {
  data: DataPoint[]
}

const props = defineProps<Props>()

const colors = [
  '#667eea', '#764ba2', '#67c23a', '#e6a23c',
  '#f56c6c', '#909399', '#c71585', '#00ced1',
  '#9370db', '#3cb371', '#ff69b4', '#cd5c5c'
]

const chartOption = computed(() => ({
  tooltip: {
    trigger: 'item',
    backgroundColor: 'rgba(255, 255, 255, 0.95)',
    borderColor: '#e0e0e0',
    borderWidth: 1,
    textStyle: { color: '#333' },
    formatter: '{a} <br/>{b}: ¥{c} ({d}%)'
  },
  legend: {
    orient: 'vertical',
    right: '10%',
    top: 'center',
    textStyle: {
      fontSize: 12,
      color: '#666'
    }
  },
  series: [
    {
      name: '资产分布',
      type: 'pie',
      radius: ['40%', '70%'],
      center: ['35%', '50%'],
      avoidLabelOverlap: false,
      itemStyle: {
        borderRadius: 10,
        borderColor: '#fff',
        borderWidth: 2
      },
      label: {
        show: false,
        position: 'center'
      },
      emphasis: {
        label: {
          show: true,
          fontSize: 18,
          fontWeight: 'bold',
          color: '#333'
        },
        itemStyle: {
          shadowBlur: 10,
          shadowOffsetX: 0,
          shadowColor: 'rgba(0, 0, 0, 0.1)'
        }
      },
      labelLine: {
        show: false
      },
      data: props.data.map((d, i) => ({
        ...d,
        itemStyle: {
          color: colors[i % colors.length]
        }
      }))
    }
  ]
}))
</script>

<style scoped>
.asset-pie-chart {
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
