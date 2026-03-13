<template>
  <div class="ai-dashboard">
    <div class="dashboard-header">
      <h2>AI 智能分析</h2>
      <p class="subtitle">利用人工智能辅助投资决策</p>
    </div>

    <div class="dashboard-grid">
      <!-- 左侧列 -->
      <div class="dashboard-column">
        <!-- 聊天组件 -->
        <div class="card card-chat">
          <AiChat />
        </div>

        <!-- 市场情绪 -->
        <div class="card">
          <MarketSentiment />
        </div>
      </div>

      <!-- 右侧列 -->
      <div class="dashboard-column">
        <!-- 收益预测 -->
        <div class="card">
          <RevenuePrediction :account-id="currentAccountId" />
        </div>

        <!-- 智能报告 -->
        <div class="card">
          <SmartReport :account-id="currentAccountId" />
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

onMounted(async () => {
  // 获取第一个账户ID
  try {
    const accounts = await apiPortfolio.getAccounts()
    if (accounts && accounts.length > 0) {
      currentAccountId.value = accounts[0].id
    }
  } catch (error) {
    console.error('Failed to load accounts:', error)
  }
})
</script>

<style scoped>
.ai-dashboard {
  padding: 20px;
  background: #f5f5f5;
  min-height: calc(100vh - 60px);
}

.dashboard-header {
  margin-bottom: 24px;
}

.dashboard-header h2 {
  margin: 0 0 8px 0;
  font-size: 24px;
  color: #333;
}

.subtitle {
  margin: 0;
  color: #999;
  font-size: 14px;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.dashboard-column {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.card-chat {
  height: 500px;
}

@media (max-width: 1200px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
}
</style>
