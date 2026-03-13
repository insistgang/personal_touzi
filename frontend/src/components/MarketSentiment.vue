<template>
  <div class="market-sentiment">
    <div class="sentiment-header">
      <h3>市场情绪分析</h3>
      <button @click="analyzeSentiment" :disabled="loading" class="analyze-btn">
        {{ loading ? '分析中...' : '分析' }}
      </button>
    </div>

    <div v-if="loading && !sentimentData" class="loading">
      正在分析市场情绪...
    </div>

    <div v-else-if="error" class="error">
      {{ error }}
    </div>

    <template v-else-if="sentimentData">
      <div class="sentiment-overview">
        <div class="sentiment-gauge" :class="sentimentClass">
          <div class="gauge-value">{{ sentimentText }}</div>
          <div class="gauge-label">当前市场情绪</div>
        </div>
        <div class="sentiment-meters">
          <div class="meter">
            <div class="meter-label">看涨</div>
            <div class="meter-bar">
              <div class="meter-fill bullish" :style="{ width: bullishPercent + '%' }"></div>
            </div>
            <div class="meter-value">{{ bullishPercent }}%</div>
          </div>
          <div class="meter">
            <div class="meter-label">中性</div>
            <div class="meter-bar">
              <div class="meter-fill neutral" :style="{ width: neutralPercent + '%' }"></div>
            </div>
            <div class="meter-value">{{ neutralPercent }}%</div>
          </div>
          <div class="meter">
            <div class="meter-label">看跌</div>
            <div class="meter-bar">
              <div class="meter-fill bearish" :style="{ width: bearishPercent + '%' }"></div>
            </div>
            <div class="meter-value">{{ bearishPercent }}%</div>
          </div>
        </div>
      </div>

      <div class="sentiment-factors">
        <h4>关键因素</h4>
        <div class="factors-list">
          <div
            v-for="factor in sentimentData.factors"
            :key="factor.name"
            class="factor-item"
            :class="factor.impact > 0 ? 'positive' : factor.impact < 0 ? 'negative' : 'neutral'"
          >
            <div class="factor-name">{{ factor.name }}</div>
            <div class="factor-impact">
              {{ factor.impact > 0 ? '+' : '' }}{{ factor.impact }}
            </div>
            <div class="factor-desc">{{ factor.description }}</div>
          </div>
        </div>
      </div>

      <div class="sentiment-advice">
        <h4>操作建议</h4>
        <div class="advice-content">
          <div class="advice-item" :class="adviceClass">
            <div class="advice-icon">{{ adviceIcon }}</div>
            <div class="advice-text">
              <div class="advice-title">{{ adviceTitle }}</div>
              <div class="advice-desc">{{ adviceDesc }}</div>
            </div>
          </div>
          <ul class="advice-list">
            <li v-for="(item, index) in sentimentData.advices" :key="index">
              {{ item }}
            </li>
          </ul>
        </div>
      </div>
    </template>

    <div v-else class="empty-state">
      <p>点击"分析"按钮获取市场情绪分析</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { apiAI } from '@/api/index.ts'

interface Factor {
  name: string
  impact: number
  description: string
}

interface SentimentData {
  overall: 'bullish' | 'neutral' | 'bearish'
  score: number
  bullish: number
  neutral: number
  bearish: number
  factors: Factor[]
  advices: string[]
}

interface Props {
  codes?: string[]
}

const props = defineProps<Props>()

const loading = ref(false)
const error = ref('')
const sentimentData = ref<SentimentData | null>(null)

const sentimentText = computed(() => {
  if (!sentimentData.value) return '-'
  const map: Record<string, string> = {
    bullish: '看涨',
    neutral: '中性',
    bearish: '看跌'
  }
  return map[sentimentData.value.overall] || '中性'
})

const sentimentClass = computed(() => {
  if (!sentimentData.value) return ''
  return `sentiment-${sentimentData.value.overall}`
})

const bullishPercent = computed(() => {
  return sentimentData.value ? Math.round(sentimentData.value.bullish * 100) : 0
})

const neutralPercent = computed(() => {
  return sentimentData.value ? Math.round(sentimentData.value.neutral * 100) : 0
})

const bearishPercent = computed(() => {
  return sentimentData.value ? Math.round(sentimentData.value.bearish * 100) : 0
})

const adviceTitle = computed(() => {
  if (!sentimentData.value) return ''
  const map: Record<string, string> = {
    bullish: '积极布局',
    neutral: '谨慎观望',
    bearish: '控制仓位'
  }
  return map[sentimentData.value.overall] || ''
})

const adviceDesc = computed(() => {
  if (!sentimentData.value) return ''
  const map: Record<string, string> = {
    bullish: '市场情绪偏暖，可适当增加仓位，关注优质标的',
    neutral: '市场方向不明，建议保持现有仓位，等待更明确信号',
    bearish: '市场情绪偏弱，建议降低仓位，注意风险控制'
  }
  return map[sentimentData.value.overall] || ''
})

const adviceClass = computed(() => {
  return sentimentClass.value
})

const adviceIcon = computed(() => {
  if (!sentimentData.value) return ''
  const map: Record<string, string> = {
    bullish: '📈',
    neutral: '⚖️',
    bearish: '📉'
  }
  return map[sentimentData.value.overall] || ''
})

const analyzeSentiment = async () => {
  loading.value = true
  error.value = ''

  try {
    const codes = props.codes || ['000001', '399001', '000300']
    const response = await apiAI.analyzeSentiment(codes)
    sentimentData.value = response.data || response
  } catch (err: any) {
    console.error('Sentiment error:', err)
    error.value = err.response?.data?.message || '分析失败，请稍后重试'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.market-sentiment {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 20px;
}

.sentiment-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.sentiment-header h3 {
  margin: 0;
  font-size: 16px;
  color: #333;
}

.analyze-btn {
  padding: 6px 16px;
  background: #409eff;
  color: #fff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s;
}

.analyze-btn:hover:not(:disabled) {
  background: #66b1ff;
}

.analyze-btn:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.loading, .error, .empty-state {
  text-align: center;
  padding: 40px;
  color: #999;
}

.error {
  color: #f56c6c;
}

.sentiment-overview {
  display: flex;
  gap: 20px;
  margin-bottom: 24px;
}

.sentiment-gauge {
  flex-shrink: 0;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 4px solid #ddd;
}

.sentiment-gauge.sentiment-bullish {
  border-color: #67c23a;
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.1) 0%, rgba(103, 194, 58, 0.05) 100%);
}

.sentiment-gauge.sentiment-neutral {
  border-color: #909399;
  background: linear-gradient(135deg, rgba(144, 147, 153, 0.1) 0%, rgba(144, 147, 153, 0.05) 100%);
}

.sentiment-gauge.sentiment-bearish {
  border-color: #f56c6c;
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.1) 0%, rgba(245, 108, 108, 0.05) 100%);
}

.gauge-value {
  font-size: 24px;
  font-weight: bold;
}

.gauge-label {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}

.sentiment-meters {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  justify-content: center;
}

.meter {
  display: flex;
  align-items: center;
  gap: 12px;
}

.meter-label {
  width: 40px;
  font-size: 14px;
  color: #666;
}

.meter-bar {
  flex: 1;
  height: 12px;
  background: #f0f0f0;
  border-radius: 6px;
  overflow: hidden;
}

.meter-fill {
  height: 100%;
  border-radius: 6px;
  transition: width 0.5s ease;
}

.meter-fill.bullish {
  background: linear-gradient(90deg, #67c23a, #85ce61);
}

.meter-fill.neutral {
  background: linear-gradient(90deg, #909399, #a6a9ad);
}

.meter-fill.bearish {
  background: linear-gradient(90deg, #f56c6c, #f78989);
}

.meter-value {
  width: 40px;
  text-align: right;
  font-size: 14px;
  font-weight: bold;
  color: #333;
}

.sentiment-factors {
  margin-bottom: 24px;
}

.sentiment-factors h4, .sentiment-advice h4 {
  margin: 0 0 16px 0;
  font-size: 15px;
  color: #333;
}

.factors-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 12px;
}

.factor-item {
  padding: 12px;
  border-radius: 8px;
  border-left: 3px solid #ddd;
  background: #f9f9f9;
}

.factor-item.positive {
  border-left-color: #67c23a;
  background: #f0f9ff;
}

.factor-item.negative {
  border-left-color: #f56c6c;
  background: #fef0f0;
}

.factor-item.neutral {
  border-left-color: #909399;
}

.factor-name {
  font-weight: 500;
  margin-bottom: 4px;
}

.factor-impact {
  font-size: 12px;
  font-weight: bold;
  margin-bottom: 4px;
}

.factor-item.positive .factor-impact {
  color: #67c23a;
}

.factor-item.negative .factor-impact {
  color: #f56c6c;
}

.factor-item.neutral .factor-impact {
  color: #909399;
}

.factor-desc {
  font-size: 12px;
  color: #999;
}

.sentiment-advice {
  margin-bottom: 16px;
}

.advice-content {
  background: #f9f9f9;
  border-radius: 8px;
  padding: 16px;
}

.advice-item {
  display: flex;
  gap: 16px;
  margin-bottom: 16px;
  padding: 16px;
  border-radius: 8px;
}

.advice-item.sentiment-bullish {
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.1) 0%, rgba(103, 194, 58, 0.05) 100%);
}

.advice-item.sentiment-neutral {
  background: linear-gradient(135deg, rgba(144, 147, 153, 0.1) 0%, rgba(144, 147, 153, 0.05) 100%);
}

.advice-item.sentiment-bearish {
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.1) 0%, rgba(245, 108, 108, 0.05) 100%);
}

.advice-icon {
  font-size: 32px;
}

.advice-text {
  flex: 1;
}

.advice-title {
  font-size: 16px;
  font-weight: bold;
  margin-bottom: 4px;
}

.advice-desc {
  font-size: 14px;
  color: #666;
}

.advice-list {
  margin: 16px 0 0 0;
  padding-left: 20px;
}

.advice-list li {
  margin: 8px 0;
  color: #666;
}
</style>
