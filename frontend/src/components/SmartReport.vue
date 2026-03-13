<template>
  <div class="smart-report">
    <div class="report-header">
      <h3>智能分析报告</h3>
      <div class="controls">
        <select v-model="reportType" class="type-select">
          <option value="week">周报</option>
          <option value="month">月报</option>
          <option value="quarter">季报</option>
        </select>
        <button @click="generateReport" :disabled="loading" class="generate-btn">
          {{ loading ? '生成中...' : '生成报告' }}
        </button>
      </div>
    </div>

    <div v-if="loading && !reportData" class="loading">
      正在生成报告，请稍候...
    </div>

    <div v-else-if="error" class="error">
      {{ error }}
    </div>

    <div v-else-if="reportData" class="report-content">
      <div class="report-meta">
        <div class="meta-item">
          <span class="label">报告类型：</span>
          <span class="value">{{ reportTypeText }}</span>
        </div>
        <div class="meta-item">
          <span class="label">生成时间：</span>
          <span class="value">{{ reportData.generateTime }}</span>
        </div>
        <div class="meta-item">
          <span class="label">时间范围：</span>
          <span class="value">{{ reportData.period }}</span>
        </div>
      </div>

      <div class="report-sections">
        <div class="report-section">
          <h4>一、收益概况</h4>
          <div class="section-content">
            <p>报告期内总收益 <span :class="reportData.totalReturn >= 0 ? 'positive' : 'negative'">
              {{ formatPercent(reportData.totalReturn) }}
            </span>，盈亏金额 <span :class="reportData.totalAmount >= 0 ? 'positive' : 'negative'">
              ¥{{ formatAmount(reportData.totalAmount) }}
            </span>。</p>
            <p>同期基准指数收益 <span :class="reportData.benchmarkReturn >= 0 ? 'positive' : 'negative'">
              {{ formatPercent(reportData.benchmarkReturn) }}
            </span>，{{ reportData.totalReturn > reportData.benchmarkReturn ? '跑赢' : '跑输' }}基准
            <span :class="reportData.excessReturn >= 0 ? 'positive' : 'negative'">
              {{ formatPercent(reportData.excessReturn) }}
            </span>。</p>
          </div>
        </div>

        <div class="report-section">
          <h4>二、持仓分析</h4>
          <div class="section-content">
            <p>期末持仓市值 <span class="value">¥{{ formatAmount(reportData.positionValue) }}</span>，
            共持有 <span class="value">{{ reportData.positionCount }}</span> 只标的。</p>
            <p>持仓集中度 <span class="value">{{ formatPercent(reportData.concentration) }}</span>，
            {{ reportData.concentration > 0.5 ? '集中度较高，需注意风险分散' : '分散度合理' }}。</p>
          </div>
        </div>

        <div class="report-section">
          <h4>三、交易分析</h4>
          <div class="section-content">
            <p>报告期内共交易 <span class="value">{{ reportData.tradeCount }}</span> 次，
            交易金额 <span class="value">¥{{ formatAmount(reportData.tradeAmount) }}</span>。</p>
            <p>胜率 <span class="value">{{ formatPercent(reportData.winRate) }}</span>，
            盈亏比 <span class="value">{{ reportData.profitLossRatio }}</span>。</p>
          </div>
        </div>

        <div class="report-section">
          <h4>四、风险提示</h4>
          <div class="section-content">
            <ul class="risk-list">
              <li v-for="(risk, index) in reportData.risks" :key="index" class="risk-item">
                {{ risk }}
              </li>
            </ul>
          </div>
        </div>

        <div class="report-section">
          <h4>五、投资建议</h4>
          <div class="section-content">
            <ul class="suggestion-list">
              <li v-for="(suggestion, index) in reportData.suggestions" :key="index" class="suggestion-item">
                {{ suggestion }}
              </li>
            </ul>
          </div>
        </div>
      </div>

      <div class="report-actions">
        <button @click="exportReport" class="export-btn">
          导出报告
        </button>
      </div>
    </div>

    <div v-else class="empty-state">
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
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 20px;
}

.report-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.report-header h3 {
  margin: 0;
  font-size: 16px;
  color: #333;
}

.controls {
  display: flex;
  gap: 8px;
}

.type-select {
  padding: 6px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
}

.generate-btn {
  padding: 6px 16px;
  background: #409eff;
  color: #fff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s;
}

.generate-btn:hover:not(:disabled) {
  background: #66b1ff;
}

.generate-btn:disabled {
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

.report-meta {
  display: flex;
  gap: 24px;
  padding: 16px;
  background: #f9f9f9;
  border-radius: 8px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.meta-item {
  display: flex;
  gap: 4px;
}

.meta-item .label {
  color: #999;
  font-size: 14px;
}

.meta-item .value {
  color: #333;
  font-size: 14px;
}

.report-sections {
  margin-bottom: 20px;
}

.report-section {
  margin-bottom: 24px;
}

.report-section h4 {
  margin: 0 0 12px 0;
  font-size: 15px;
  color: #333;
  padding-bottom: 8px;
  border-bottom: 1px solid #eee;
}

.section-content {
  font-size: 14px;
  line-height: 1.8;
  color: #666;
}

.section-content p {
  margin: 8px 0;
}

.section-content .value {
  color: #409eff;
  font-weight: 500;
}

.section-content .positive {
  color: #67c23a;
}

.section-content .negative {
  color: #f56c6c;
}

.risk-list, .suggestion-list {
  margin: 8px 0;
  padding-left: 20px;
}

.risk-item, .suggestion-item {
  margin: 8px 0;
}

.risk-item {
  color: #e6a23c;
}

.suggestion-item {
  color: #409eff;
}

.report-actions {
  display: flex;
  justify-content: flex-end;
  padding-top: 16px;
  border-top: 1px solid #eee;
}

.export-btn {
  padding: 8px 20px;
  background: #67c23a;
  color: #fff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s;
}

.export-btn:hover {
  background: #85ce61;
}
</style>
