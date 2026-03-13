<template>
  <div class="market-sentiment">
    <div class="sentiment-header">
      <div class="header-left">
        <div class="icon-wrapper">
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path d="M10 18C14.42 18 18 14.42 18 10C18 5.58 14.42 2 10 2C5.58 2 2 5.58 2 10C2 14.42 5.58 18 10 18Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
            <path d="M10 6V10L13 13" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <h3>市场情绪分析</h3>
      </div>
      <button @click="analyzeSentiment" :disabled="loading" class="analyze-btn">
        <svg v-if="!loading" width="16" height="16" viewBox="0 0 16 16" fill="none">
          <path d="M8 2L14.5 6.5L13 9.5L9 8.5L7 12L4.5 11L3.5 14L2 13.5L4 8L8 9L10.5 5.5L8 2Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          <path d="M14 14C14.55 14 15 13.55 15 13V9C15 8.45 14.55 8 14 8C13.45 8 13 8.45 13 9V13C13 13.55 13.45 14 14 14Z" fill="currentColor"/>
        </svg>
        <span v-else class="spinner"></span>
        {{ loading ? '分析中...' : 'AI 分析' }}
      </button>
    </div>

    <div v-if="loading && !sentimentData" class="loading">
      <div class="loading-spinner"></div>
      <p>AI 正在分析市场数据...</p>
    </div>

    <div v-else-if="error" class="error">
      <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
        <circle cx="20" cy="20" r="16" stroke="#f56c6c" stroke-width="2"/>
        <path d="M20 14V22M20 26V27" stroke="#f56c6c" stroke-width="2" stroke-linecap="round"/>
      </svg>
      <p>{{ error }}</p>
    </div>

    <template v-else-if="sentimentData">
      <div class="sentiment-overview">
        <div class="sentiment-gauge" :class="sentimentClass">
          <div class="gauge-icon">{{ gaugeIcon }}</div>
          <div class="gauge-value">{{ sentimentText }}</div>
          <div class="gauge-label">当前市场情绪</div>
          <div class="gauge-score" v-if="sentimentData.score">
            综合评分: {{ sentimentData.score.toFixed(1) }}
          </div>
        </div>
        <div class="sentiment-meters">
          <div class="meter">
            <div class="meter-header">
              <span class="meter-label">看涨</span>
              <span class="meter-value">{{ bullishPercent }}%</span>
            </div>
            <div class="meter-bar">
              <div class="meter-fill bullish" :style="{ width: bullishPercent + '%' }"></div>
            </div>
          </div>
          <div class="meter">
            <div class="meter-header">
              <span class="meter-label">中性</span>
              <span class="meter-value">{{ neutralPercent }}%</span>
            </div>
            <div class="meter-bar">
              <div class="meter-fill neutral" :style="{ width: neutralPercent + '%' }"></div>
            </div>
          </div>
          <div class="meter">
            <div class="meter-header">
              <span class="meter-label">看跌</span>
              <span class="meter-value">{{ bearishPercent }}%</span>
            </div>
            <div class="meter-bar">
              <div class="meter-fill bearish" :style="{ width: bearishPercent + '%' }"></div>
            </div>
          </div>
        </div>
      </div>

      <div class="sentiment-factors">
        <div class="section-header">
          <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
            <path d="M9 2L2.5 8.5H7.5V16H10.5V8.5H15.5L9 2Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          <h4>关键因素</h4>
        </div>
        <div class="factors-list">
          <div
            v-for="factor in sentimentData.factors"
            :key="factor.name"
            class="factor-item"
            :class="factor.impact > 0 ? 'positive' : factor.impact < 0 ? 'negative' : 'neutral'"
          >
            <div class="factor-icon">
              <svg v-if="factor.impact > 0" width="20" height="20" viewBox="0 0 20 20" fill="none">
                <path d="M10 4L16 10L14.5 11.5L11 8V16H9V8L5.5 11.5L4 10L10 4Z" fill="currentColor"/>
              </svg>
              <svg v-else-if="factor.impact < 0" width="20" height="20" viewBox="0 0 20 20" fill="none">
                <path d="M10 16L4 10L5.5 8.5L9 12V4H11V12L14.5 8.5L16 10L10 16Z" fill="currentColor"/>
              </svg>
              <svg v-else width="20" height="20" viewBox="0 0 20 20" fill="none">
                <circle cx="10" cy="10" r="6" stroke="currentColor" stroke-width="1.5"/>
                <path d="M10 7V13" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
              </svg>
            </div>
            <div class="factor-content">
              <div class="factor-header">
                <span class="factor-name">{{ factor.name }}</span>
                <span class="factor-impact">
                  {{ factor.impact > 0 ? '+' : '' }}{{ factor.impact }}
                </span>
              </div>
              <div class="factor-desc">{{ factor.description }}</div>
            </div>
          </div>
        </div>
      </div>

      <div class="sentiment-advice">
        <div class="section-header">
          <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
            <path d="M9 2L3 7H7V15H11V7H15L9 2Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          <h4>操作建议</h4>
        </div>
        <div class="advice-content">
          <div class="advice-main" :class="adviceClass">
            <div class="advice-visual">
              <div class="advice-icon">{{ adviceIcon }}</div>
              <div class="advice-badge">{{ adviceBadge }}</div>
            </div>
            <div class="advice-text">
              <div class="advice-title">{{ adviceTitle }}</div>
              <div class="advice-desc">{{ adviceDesc }}</div>
            </div>
          </div>
          <div class="advice-list-container">
            <div class="advice-list-title">具体建议：</div>
            <ul class="advice-list">
              <li v-for="(item, index) in sentimentData.advices" :key="index" class="advice-list-item">
                <span class="bullet">•</span>
                {{ item }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </template>

    <div v-else class="empty-state">
      <svg width="64" height="64" viewBox="0 0 64 64" fill="none">
        <circle cx="32" cy="32" r="24" stroke="#e0e0e0" stroke-width="2"/>
        <path d="M32 20V36M32 42V44" stroke="#e0e0e0" stroke-width="2" stroke-linecap="round"/>
      </svg>
      <p>点击"AI 分析"按钮获取市场情绪分析</p>
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

const gaugeIcon = computed(() => {
  if (!sentimentData.value) return ''
  const map: Record<string, string> = {
    bullish: '🚀',
    neutral: '😐',
    bearish: '🐻'
  }
  return map[sentimentData.value.overall] || ''
})

const adviceBadge = computed(() => {
  if (!sentimentData.value) return ''
  const map: Record<string, string> = {
    bullish: '积极',
    neutral: '观望',
    bearish: '保守'
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
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.08);
  padding: 24px;
}

.sentiment-header {
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

.sentiment-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.analyze-btn {
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

.analyze-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.3);
}

.analyze-btn:disabled {
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

.loading, .error, .empty-state {
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

.loading p, .error p, .empty-state p {
  margin: 8px 0 0 0;
  font-size: 14px;
}

.error {
  color: #f56c6c;
}

.empty-state svg {
  margin-bottom: 16px;
}

.sentiment-overview {
  display: flex;
  gap: 24px;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.sentiment-gauge {
  flex-shrink: 0;
  width: 140px;
  height: 140px;
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 3px solid #e0e0e0;
  background: #fafafa;
  transition: all 0.3s;
}

.sentiment-gauge.sentiment-bullish {
  border-color: #67c23a;
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.15) 0%, rgba(103, 194, 58, 0.05) 100%);
  box-shadow: 0 4px 20px rgba(103, 194, 58, 0.2);
}

.sentiment-gauge.sentiment-neutral {
  border-color: #909399;
  background: linear-gradient(135deg, rgba(144, 147, 153, 0.15) 0%, rgba(144, 147, 153, 0.05) 100%);
}

.sentiment-gauge.sentiment-bearish {
  border-color: #f56c6c;
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.15) 0%, rgba(245, 108, 108, 0.05) 100%);
  box-shadow: 0 4px 20px rgba(245, 108, 108, 0.2);
}

.gauge-icon {
  font-size: 36px;
  margin-bottom: 4px;
}

.gauge-value {
  font-size: 22px;
  font-weight: 700;
  color: #333;
}

.sentiment-gauge.sentiment-bullish .gauge-value {
  color: #67c23a;
}

.sentiment-gauge.sentiment-bearish .gauge-value {
  color: #f56c6c;
}

.gauge-label {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}

.gauge-score {
  font-size: 11px;
  color: #999;
  margin-top: 4px;
  padding: 2px 8px;
  background: rgba(255, 255, 255, 0.6);
  border-radius: 10px;
}

.sentiment-meters {
  flex: 1;
  min-width: 200px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  justify-content: center;
}

.meter {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.meter-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.meter-label {
  font-size: 14px;
  color: #666;
  font-weight: 500;
}

.meter-value {
  font-size: 14px;
  font-weight: 700;
  color: #333;
}

.meter-bar {
  height: 12px;
  background: #f0f0f0;
  border-radius: 6px;
  overflow: hidden;
}

.meter-fill {
  height: 100%;
  border-radius: 6px;
  transition: width 0.6s ease;
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

.sentiment-factors {
  margin-bottom: 24px;
}

.sentiment-advice {
  margin-bottom: 8px;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
}

.section-header svg {
  color: #667eea;
}

.section-header h4 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #333;
}

.factors-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 12px;
}

.factor-item {
  display: flex;
  gap: 12px;
  padding: 14px;
  border-radius: 10px;
  background: #fafafa;
  border: 1px solid #f0f0f0;
  transition: all 0.3s;
}

.factor-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
}

.factor-item.positive {
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.08) 0%, rgba(103, 194, 58, 0.02) 100%);
  border-color: rgba(103, 194, 58, 0.2);
}

.factor-item.negative {
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.08) 0%, rgba(245, 108, 108, 0.02) 100%);
  border-color: rgba(245, 108, 108, 0.2);
}

.factor-item.neutral {
  background: #fafafa;
  border-color: #e0e0e0;
}

.factor-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.factor-item.positive .factor-icon {
  background: rgba(103, 194, 58, 0.15);
  color: #67c23a;
}

.factor-item.negative .factor-icon {
  background: rgba(245, 108, 108, 0.15);
  color: #f56c6c;
}

.factor-item.neutral .factor-icon {
  background: rgba(144, 147, 153, 0.15);
  color: #909399;
}

.factor-content {
  flex: 1;
}

.factor-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.factor-name {
  font-weight: 600;
  font-size: 14px;
  color: #333;
}

.factor-impact {
  font-size: 12px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
}

.factor-item.positive .factor-impact {
  color: #67c23a;
  background: rgba(103, 194, 58, 0.1);
}

.factor-item.negative .factor-impact {
  color: #f56c6c;
  background: rgba(245, 108, 108, 0.1);
}

.factor-item.neutral .factor-impact {
  color: #909399;
  background: rgba(144, 147, 153, 0.1);
}

.factor-desc {
  font-size: 12px;
  color: #999;
  line-height: 1.5;
}

.advice-content {
  background: #fafafa;
  border-radius: 12px;
  padding: 20px;
}

.advice-main {
  display: flex;
  gap: 20px;
  padding: 20px;
  border-radius: 12px;
  margin-bottom: 16px;
}

.advice-main.sentiment-bullish {
  background: linear-gradient(135deg, rgba(103, 194, 58, 0.12) 0%, rgba(103, 194, 58, 0.04) 100%);
  border: 1px solid rgba(103, 194, 58, 0.2);
}

.advice-main.sentiment-neutral {
  background: linear-gradient(135deg, rgba(144, 147, 153, 0.12) 0%, rgba(144, 147, 153, 0.04) 100%);
  border: 1px solid rgba(144, 147, 153, 0.2);
}

.advice-main.sentiment-bearish {
  background: linear-gradient(135deg, rgba(245, 108, 108, 0.12) 0%, rgba(245, 108, 108, 0.04) 100%);
  border: 1px solid rgba(245, 108, 108, 0.2);
}

.advice-visual {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.advice-icon {
  font-size: 48px;
}

.advice-badge {
  position: absolute;
  bottom: -4px;
  right: -4px;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 700;
  color: #fff;
}

.advice-main.sentiment-bullish .advice-badge {
  background: #67c23a;
}

.advice-main.sentiment-neutral .advice-badge {
  background: #909399;
}

.advice-main.sentiment-bearish .advice-badge {
  background: #f56c6c;
}

.advice-text {
  flex: 1;
}

.advice-title {
  font-size: 18px;
  font-weight: 700;
  margin-bottom: 8px;
  color: #333;
}

.advice-desc {
  font-size: 14px;
  color: #666;
  line-height: 1.6;
}

.advice-list-container {
  background: #fff;
  border-radius: 10px;
  padding: 16px;
}

.advice-list-title {
  font-size: 13px;
  font-weight: 600;
  color: #666;
  margin-bottom: 12px;
}

.advice-list {
  margin: 0;
  padding: 0;
  list-style: none;
}

.advice-list-item {
  display: flex;
  gap: 8px;
  margin-bottom: 10px;
  color: #666;
  font-size: 14px;
  line-height: 1.6;
}

.advice-list-item:last-child {
  margin-bottom: 0;
}

.advice-list-item .bullet {
  color: #667eea;
  font-weight: bold;
  font-size: 18px;
  line-height: 1.4;
}

@media (max-width: 768px) {
  .sentiment-overview {
    flex-direction: column;
  }

  .sentiment-gauge {
    width: 120px;
    height: 120px;
  }

  .factors-list {
    grid-template-columns: 1fr;
  }

  .advice-main {
    flex-direction: column;
    text-align: center;
  }
}
</style>
