<template>
  <div class="smart-report">
    <div class="report-header">
      <div class="header-left">
        <div class="icon-wrapper">
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path d="M10 2L2 6V10C2 14.42 5.57 18.54 10 19.5C14.43 18.54 18 14.42 18 10V6L10 2Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M7 10L9 12L13 8" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <h3>AI 智能报告</h3>
      </div>
      <div class="controls">
        <div class="select-wrapper">
          <select v-model="reportType" class="type-select">
            <option value="week">周报</option>
            <option value="month">月报</option>
            <option value="quarter">季报</option>
          </select>
        </div>
        <button @click="generateReport" :disabled="loading" class="generate-btn">
          <svg v-if="!loading" width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M8 3V13M8 13L5 10M8 13L11 10" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          <span v-else class="spinner"></span>
          {{ loading ? '生成中...' : '生成报告' }}
        </button>
      </div>
    </div>

    <div v-if="loading && !reportData" class="loading">
      <div class="loading-spinner"></div>
      <p>AI 正在分析您的投资数据，生成智能报告...</p>
    </div>

    <div v-else-if="error" class="error">
      <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
        <circle cx="20" cy="20" r="16" stroke="#f56c6c" stroke-width="2"/>
        <path d="M20 14V22M20 26V27" stroke="#f56c6c" stroke-width="2" stroke-linecap="round"/>
      </svg>
      <p>{{ error }}</p>
    </div>

    <div v-else-if="reportData" class="report-content">
      <div class="report-meta">
        <div class="meta-item">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M8 2C4.69 2 2 4.69 2 8C2 11.31 4.69 14 8 14C11.31 14 14 11.31 14 8C14 4.69 11.31 2 8 2Z" stroke="currentColor" stroke-width="1.5"/>
            <path d="M8 5V8L10 10" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
          <span class="label">报告类型</span>
          <span class="value">{{ reportTypeText }}</span>
        </div>
        <div class="meta-item">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M8 2V14M8 2L4 6M8 2L12 6" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          <span class="label">生成时间</span>
          <span class="value">{{ reportData.generateTime }}</span>
        </div>
        <div class="meta-item">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M2 8H14M2 8L6 4M2 8L6 12" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M14 8L10 4M14 8L10 12" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          <span class="label">时间范围</span>
          <span class="value">{{ reportData.period }}</span>
        </div>
      </div>

      <div class="report-sections">
        <div class="report-section highlight-section">
          <div class="section-header">
            <span class="section-number">01</span>
            <h4>收益概况</h4>
          </div>
          <div class="section-content">
            <div class="highlight-cards">
              <div class="highlight-card" :class="reportData.totalReturn >= 0 ? 'positive' : 'negative'">
                <div class="card-label">总收益</div>
                <div class="card-value">{{ formatPercent(reportData.totalReturn) }}</div>
              </div>
              <div class="highlight-card" :class="reportData.totalAmount >= 0 ? 'positive' : 'negative'">
                <div class="card-label">盈亏金额</div>
                <div class="card-value">¥{{ formatAmount(reportData.totalAmount) }}</div>
              </div>
              <div class="highlight-card" :class="reportData.excessReturn >= 0 ? 'positive' : 'negative'">
                <div class="card-label">超额收益</div>
                <div class="card-value">{{ formatPercent(reportData.excessReturn) }}</div>
              </div>
            </div>
            <p>同期基准指数收益 <span :class="reportData.benchmarkReturn >= 0 ? 'positive' : 'negative'">
              {{ formatPercent(reportData.benchmarkReturn) }}
            </span>，{{ reportData.totalReturn > reportData.benchmarkReturn ? '跑赢' : '跑输' }}基准。</p>
          </div>
        </div>

        <div class="report-section">
          <div class="section-header">
            <span class="section-number">02</span>
            <h4>持仓分析</h4>
          </div>
          <div class="section-content">
            <div class="info-row">
              <div class="info-item">
                <span class="info-label">期末持仓市值</span>
                <span class="info-value">¥{{ formatAmount(reportData.positionValue) }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">持仓数量</span>
                <span class="info-value">{{ reportData.positionCount }} 只</span>
              </div>
            </div>
            <div class="concentration-bar">
              <div class="bar-label">持仓集中度</div>
              <div class="bar-wrapper">
                <div class="bar-fill" :style="{ width: (reportData.concentration * 100) + '%' }"></div>
              </div>
              <div class="bar-value">{{ formatPercent(reportData.concentration) }}</div>
            </div>
            <p class="conclusion" :class="reportData.concentration > 0.5 ? 'warning' : 'info'">
              {{ reportData.concentration > 0.5 ? '集中度较高，需注意风险分散' : '分散度合理' }}
            </p>
          </div>
        </div>

        <div class="report-section">
          <div class="section-header">
            <span class="section-number">03</span>
            <h4>交易分析</h4>
          </div>
          <div class="section-content">
            <div class="trade-stats">
              <div class="stat-item">
                <div class="stat-label">交易次数</div>
                <div class="stat-value">{{ reportData.tradeCount }}</div>
              </div>
              <div class="stat-item">
                <div class="stat-label">交易金额</div>
                <div class="stat-value">¥{{ formatAmount(reportData.tradeAmount) }}</div>
              </div>
              <div class="stat-item">
                <div class="stat-label">胜率</div>
                <div class="stat-value success">{{ formatPercent(reportData.winRate) }}</div>
              </div>
              <div class="stat-item">
                <div class="stat-label">盈亏比</div>
                <div class="stat-value">{{ reportData.profitLossRatio }}</div>
              </div>
            </div>
          </div>
        </div>

        <div class="report-section warning-section">
          <div class="section-header">
            <span class="section-number warning">04</span>
            <h4>风险提示</h4>
          </div>
          <div class="section-content">
            <ul class="risk-list">
              <li v-for="(risk, index) in reportData.risks" :key="index" class="risk-item">
                <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                  <path d="M8 2L1.5 13.5H14.5L8 2Z" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round"/>
                  <path d="M8 6V9M8 11V11.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
                </svg>
                {{ risk }}
              </li>
            </ul>
          </div>
        </div>

        <div class="report-section suggestion-section">
          <div class="section-header">
            <span class="section-number suggestion">05</span>
            <h4>投资建议</h4>
          </div>
          <div class="section-content">
            <ul class="suggestion-list">
              <li v-for="(suggestion, index) in reportData.suggestions" :key="index" class="suggestion-item">
                <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                  <path d="M8 2L10.5 6H5.5L8 2Z" fill="currentColor"/>
                  <path d="M8 6V14" stroke="currentColor" stroke-width="1.5"/>
                  <path d="M3 14H13" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
                </svg>
                {{ suggestion }}
              </li>
            </ul>
          </div>
        </div>
      </div>

      <div class="report-actions">
        <button @click="exportReport" class="export-btn">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M8 2V10M8 10L5 7M8 10L11 7" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M2 13H14" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
            <path d="M3 10V14H13V10" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          导出报告
        </button>
      </div>
    </div>

    <div v-else class="empty-state">
      <svg width="64" height="64" viewBox="0 0 64 64" fill="none">
        <circle cx="32" cy="32" r="24" stroke="#e0e0e0" stroke-width="2"/>
        <path d="M32 20V36M32 42V44" stroke="#e0e0e0" stroke-width="2" stroke-linecap="round"/>
      </svg>
      <p>请选择报告类型并点击"生成报告"</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { apiAI } from '@/api/index.ts'

interface ReportData {
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
}

interface Props {
  accountId?: number
}

const props = defineProps<Props>()

const reportType = ref('week')
const loading = ref(false)
const error = ref('')
const reportData = ref<ReportData | null>(null)

const reportTypeText = computed(() => {
  const map: Record<string, string> = {
    week: '周报',
    month: '月报',
    quarter: '季报'
  }
  return map[reportType.value] || '报告'
})

const formatPercent = (value: number) => {
  return (value * 100).toFixed(2) + '%'
}

const formatAmount = (value: number) => {
  return value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const generateReport = async () => {
  loading.value = true
  error.value = ''

  try {
    const response = await apiAI.generateReport(props.accountId || 1, reportType.value)
    reportData.value = response.data || response
  } catch (err: any) {
    console.error('Report error:', err)
    error.value = err.response?.data?.message || '生成报告失败'
  } finally {
    loading.value = false
  }
}

const exportReport = () => {
  if (!reportData.value) return

  const content = `
智能分析报告（${reportTypeText.value}）
====================================

生成时间：${reportData.value.generateTime}
时间范围：${reportData.value.period}

一、收益概况
-----------
报告期内总收益：${formatPercent(reportData.value.totalReturn)}
盈亏金额：¥${formatAmount(reportData.value.totalAmount)}
同期基准收益：${formatPercent(reportData.value.benchmarkReturn)}
超额收益：${formatPercent(reportData.value.excessReturn)}

二、持仓分析
-----------
持仓市值：¥${formatAmount(reportData.value.positionValue)}
持仓数量：${reportData.value.positionCount}
持仓集中度：${formatPercent(reportData.value.concentration)}

三、交易分析
-----------
交易次数：${reportData.value.tradeCount}
交易金额：¥${formatAmount(reportData.value.tradeAmount)}
胜率：${formatPercent(reportData.value.winRate)}
盈亏比：${reportData.value.profitLossRatio}

四、风险提示
-----------
${reportData.value.risks.map((r, i) => `${i + 1}. ${r}`).join('\n')}

五、投资建议
-----------
${reportData.value.suggestions.map((s, i) => `${i + 1}. ${s}`).join('\n')}
  `.trim()

  const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `投资报告_${reportType.value}_${new Date().toISOString().slice(0, 10)}.txt`
  a.click()
  URL.revokeObjectURL(url)
}
</script>

<style scoped>
.smart-report {
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.08);
  padding: 24px;
}

.report-header {
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

.report-header h3 {
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

.type-select {
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

.type-select:hover {
  border-color: #667eea;
}

.type-select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.generate-btn {
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

.generate-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.3);
}

.generate-btn:disabled {
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

.report-meta {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 12px;
  padding: 16px;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%);
  border-radius: 12px;
  margin-bottom: 24px;
  border: 1px solid rgba(102, 126, 234, 0.1);
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 10px;
}

.meta-item svg {
  color: #667eea;
  flex-shrink: 0;
}

.meta-item .label {
  color: #999;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.meta-item .value {
  color: #333;
  font-size: 14px;
  font-weight: 500;
}

.report-sections {
  margin-bottom: 24px;
}

.report-section {
  margin-bottom: 24px;
  padding: 20px;
  background: #fafafa;
  border-radius: 12px;
  transition: all 0.3s;
}

.report-section:hover {
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
}

.report-section.highlight-section {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%);
  border: 1px solid rgba(102, 126, 234, 0.1);
}

.report-section.warning-section {
  background: linear-gradient(135deg, rgba(230, 162, 60, 0.05) 0%, rgba(230, 162, 60, 0.02) 100%);
  border: 1px solid rgba(230, 162, 60, 0.15);
}

.report-section.suggestion-section {
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.05) 0%, rgba(64, 158, 255, 0.02) 100%);
  border: 1px solid rgba(64, 158, 255, 0.15);
}

.section-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.section-number {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
}

.section-number.warning {
  background: linear-gradient(135deg, #e6a23c 0%, #d4963a 100%);
}

.section-number.suggestion {
  background: linear-gradient(135deg, #409eff 0%, #3a8ee6 100%);
}

.section-header h4 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #333;
}

.section-content {
  font-size: 14px;
  line-height: 1.8;
  color: #666;
}

.section-content p {
  margin: 8px 0;
}

.highlight-cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
  margin-bottom: 16px;
}

.highlight-card {
  padding: 16px;
  border-radius: 10px;
  background: #fff;
  text-align: center;
  transition: all 0.3s;
}

.highlight-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.highlight-card .card-label {
  font-size: 12px;
  color: #999;
  margin-bottom: 8px;
}

.highlight-card .card-value {
  font-size: 20px;
  font-weight: 700;
}

.highlight-card.positive {
  border: 1px solid rgba(103, 194, 58, 0.2);
}

.highlight-card.positive .card-value {
  color: #67c23a;
}

.highlight-card.negative {
  border: 1px solid rgba(245, 108, 108, 0.2);
}

.highlight-card.negative .card-value {
  color: #f56c6c;
}

.info-row {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  margin-bottom: 16px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  padding: 12px 16px;
  background: #fff;
  border-radius: 8px;
}

.info-label {
  color: #999;
  font-size: 13px;
}

.info-value {
  color: #333;
  font-weight: 600;
}

.concentration-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.bar-label {
  font-size: 13px;
  color: #666;
  width: 80px;
}

.bar-wrapper {
  flex: 1;
  height: 10px;
  background: #e0e0e0;
  border-radius: 5px;
  overflow: hidden;
}

.bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
  border-radius: 5px;
  transition: width 0.5s ease;
}

.bar-value {
  width: 50px;
  text-align: right;
  font-weight: 600;
  color: #667eea;
}

.conclusion {
  padding: 10px 14px;
  border-radius: 6px;
  font-size: 13px;
  margin: 0;
}

.conclusion.warning {
  background: rgba(230, 162, 60, 0.1);
  color: #e6a23c;
}

.conclusion.info {
  background: rgba(103, 194, 58, 0.1);
  color: #67c23a;
}

.trade-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}

.stat-item {
  padding: 16px;
  background: #fff;
  border-radius: 10px;
  text-align: center;
}

.stat-label {
  font-size: 12px;
  color: #999;
  margin-bottom: 8px;
}

.stat-value {
  font-size: 18px;
  font-weight: 700;
  color: #333;
}

.stat-value.success {
  color: #67c23a;
}

.risk-list, .suggestion-list {
  margin: 0;
  padding: 0;
  list-style: none;
}

.risk-item, .suggestion-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  margin-bottom: 12px;
  padding: 12px;
  background: #fff;
  border-radius: 8px;
}

.risk-item svg {
  color: #e6a23c;
  flex-shrink: 0;
  margin-top: 2px;
}

.suggestion-item svg {
  color: #409eff;
  flex-shrink: 0;
  margin-top: 2px;
}

.risk-item {
  color: #666;
}

.suggestion-item {
  color: #666;
}

.report-actions {
  display: flex;
  justify-content: flex-end;
  padding-top: 16px;
  border-top: 1px solid #eee;
}

.export-btn {
  padding: 10px 24px;
  background: linear-gradient(135deg, #67c23a 0%, #5daf34 100%);
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

.export-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(103, 194, 58, 0.3);
}

@media (max-width: 768px) {
  .highlight-cards,
  .trade-stats {
    grid-template-columns: 1fr;
  }

  .info-row {
    grid-template-columns: 1fr;
  }
}
</style>
