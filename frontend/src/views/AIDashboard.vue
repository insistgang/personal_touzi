<template>
  <div class="ai-dashboard">
    <div class="page-header">
      <div class="header-content">
        <div class="header-icon">
          <svg width="32" height="32" viewBox="0 0 32 32" fill="none">
            <rect width="32" height="32" rx="16" fill="url(#gradient)"/>
            <path d="M16 8L20 12H18V16H20L16 20L12 16H14V12H12L16 8Z" fill="white"/>
            <defs>
              <linearGradient id="gradient" x1="0" y1="0" x2="32" y2="32" gradientUnits="userSpaceOnUse">
                <stop stop-color="#667eea"/>
                <stop offset="1" stop-color="#764ba2"/>
              </linearGradient>
            </defs>
          </svg>
        </div>
        <div>
          <h1>AI 智能分析</h1>
          <p class="subtitle">利用人工智能辅助投资决策，洞察市场趋势</p>
        </div>
      </div>
      <div class="header-stats">
        <div class="stat-item">
          <span class="stat-label">AI 模型</span>
          <span class="stat-value">GLM / 内置</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">运行模式</span>
          <span class="stat-value">自动降级</span>
        </div>
      </div>
    </div>

    <div class="dashboard-grid">
      <!-- 左侧列 -->
      <div class="dashboard-column">
        <!-- 聊天组件 -->
        <div class="card card-chat">
          <div class="card-header">
            <div class="card-title">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
              </svg>
              AI 智能助手
            </div>
            <span class="card-badge">在线</span>
          </div>
          <div class="card-body">
            <AiChat ref="aiChatRef" />
          </div>
        </div>

        <!-- 市场情绪 -->
        <div class="card">
          <div class="card-header">
            <div class="card-title">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 20a8 8 0 1 0 0-16 8 8 0 0 0 0 16z"/>
                <path d="M8 14s1.5 2 4 2 4-2 4-2M9 9h.01M15 9h.01"/>
              </svg>
              市场情绪分析
            </div>
            <span class="card-badge neutral">实时</span>
          </div>
          <div class="card-body">
            <MarketSentiment ref="sentimentRef" />
          </div>
        </div>
      </div>

      <!-- 右侧列 -->
      <div class="dashboard-column">
        <!-- 收益预测 -->
        <div class="card">
          <div class="card-header">
            <div class="card-title">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
              </svg>
              AI 收益预测
            </div>
            <span class="card-badge positive">+预测</span>
          </div>
          <div class="card-body">
            <RevenuePrediction ref="predictionRef" :account-id="currentAccountId" />
          </div>
        </div>

        <!-- 智能报告 -->
        <div class="card">
          <div class="card-header">
            <div class="card-title">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                <polyline points="14 2 14 8 20 8"/>
                <line x1="16" y1="13" x2="8" y2="13"/>
                <line x1="16" y1="17" x2="8" y2="17"/>
              </svg>
              智能投资报告
            </div>
            <span class="card-badge">每日</span>
          </div>
          <div class="card-body">
            <SmartReport ref="reportRef" :account-id="currentAccountId" />
          </div>
        </div>

        <!-- 快捷操作 -->
        <div class="card quick-actions">
          <div class="card-header">
            <div class="card-title">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10"/>
                <line x1="12" y1="8" x2="12" y2="16"/>
                <line x1="8" y1="12" x2="16" y2="12"/>
              </svg>
              快捷操作
            </div>
          </div>
          <div class="card-body">
            <div class="action-buttons">
              <button class="action-btn" @click="askAi('分析我的当前持仓结构和风险')">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                  <circle cx="12" cy="12" r="3"/>
                </svg>
                分析持仓
              </button>
              <button class="action-btn" @click="runPrediction">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M12 20V10M18 20V4M6 20v-6"/>
                </svg>
                风险评估
              </button>
              <button class="action-btn" @click="runSentiment">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="12" cy="12" r="10"/>
                  <polyline points="12 6 12 12 16 14"/>
                </svg>
                市场回顾
              </button>
              <button class="action-btn" @click="runReport">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                </svg>
                收藏推荐
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { apiPortfolio } from '@/api/index.ts'
import AiChat from '@/components/AiChat.vue'
import RevenuePrediction from '@/components/RevenuePrediction.vue'
import SmartReport from '@/components/SmartReport.vue'
import MarketSentiment from '@/components/MarketSentiment.vue'

const currentAccountId = ref<number>(1)
const aiChatRef = ref<{ askQuestion: (question: string) => void } | null>(null)
const predictionRef = ref<{ fetchPrediction: () => void } | null>(null)
const reportRef = ref<{ generateReport: () => void } | null>(null)
const sentimentRef = ref<{ analyzeSentiment: () => void } | null>(null)

const askAi = (question: string) => {
  aiChatRef.value?.askQuestion(question)
}

const runPrediction = () => {
  predictionRef.value?.fetchPrediction()
}

const runReport = () => {
  reportRef.value?.generateReport()
}

const runSentiment = () => {
  sentimentRef.value?.analyzeSentiment()
}

onMounted(async () => {
  // 获取第一个账户ID
  try {
    const accounts = await apiPortfolio.getAccounts()
    if (accounts && accounts.length > 0 && accounts[0]?.id) {
      currentAccountId.value = accounts[0].id
    }
  } catch (error) {
    console.error('Failed to load accounts:', error)
  }
})
</script>

<style scoped>
.ai-dashboard {
  padding: 30px;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
  min-height: calc(100vh - 70px);
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

.header-content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-icon {
  width: 56px;
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 100%);
  border-radius: 16px;
}

.header-content h1 {
  margin: 0 0 4px 0;
  font-size: 26px;
  font-weight: 700;
  color: #1a1a2e;
}

.subtitle {
  margin: 0;
  color: #6b7280;
  font-size: 14px;
}

.header-stats {
  display: flex;
  gap: 20px;
}

.stat-item {
  text-align: center;
  padding: 12px 20px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.stat-label {
  display: block;
  font-size: 12px;
  color: #6b7280;
  margin-bottom: 4px;
}

.stat-value {
  display: block;
  font-size: 16px;
  font-weight: 700;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}

.dashboard-column {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.card {
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.card:hover {
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 18px 24px;
  border-bottom: 1px solid #f0f0f0;
  background: linear-gradient(to bottom, #fafbfc, #fff);
}

.card-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 15px;
  font-weight: 600;
  color: #1a1a2e;
}

.card-title svg {
  color: #667eea;
}

.card-badge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
  background: #f0f0f0;
  color: #6b7280;
}

.card-badge.positive {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 100%);
  color: #667eea;
}

.card-badge.neutral {
  background: linear-gradient(135deg, rgba(79, 172, 254, 0.15) 0%, rgba(0, 242, 254, 0.15) 100%);
  color: #4facfe;
}

.card-body {
  padding: 20px 24px;
}

.card-chat {
  height: 520px;
}

.card-chat .card-body {
  padding: 0;
  height: calc(100% - 61px);
}

.quick-actions .card-body {
  padding: 20px;
}

.action-buttons {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

.action-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 20px 16px;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.08) 100%);
  border: 2px solid transparent;
  border-radius: 14px;
  font-size: 13px;
  font-weight: 500;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.3s;
}

.action-btn:hover {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 100%);
  border-color: rgba(102, 126, 234, 0.3);
  color: #667eea;
  transform: translateY(-2px);
}

.action-btn svg {
  color: #667eea;
}

@media (max-width: 1200px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .ai-dashboard {
    padding: 20px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .header-content h1 {
    font-size: 22px;
  }

  .action-buttons {
    grid-template-columns: 1fr;
  }

  .card-chat {
    height: 450px;
  }
}
</style>
