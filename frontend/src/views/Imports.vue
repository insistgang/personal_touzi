<template>
  <div class="imports">
    <div class="page-header">
      <div class="header-content">
        <h1>数据导入</h1>
        <p class="subtitle">把初始持仓导入和批量交易入账拆成独立流程，避免和日常维护入口混用。</p>
      </div>
    </div>

    <div class="notice-banner">
      初始持仓导入只适用于空账户，用来建立基线；批量交易导入会逐笔走结算规则，并以整批事务方式提交，任何一行失败都会整批回滚。
    </div>

    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>处理中...</p>
    </div>
    <div v-else-if="error" class="error-container">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <p>{{ error }}</p>
    </div>
    <div v-else class="imports-grid">
      <section class="import-card">
        <div class="card-header">
          <div>
            <h2>初始持仓导入</h2>
            <p>适合第一次建账时，把当前仓位一次性导入到空账户。</p>
          </div>
          <span class="card-badge baseline">基线导入</span>
        </div>

        <div class="form-group">
          <label>目标账户</label>
          <select v-model.number="initialPositionsForm.accountId">
            <option v-for="account in accounts" :key="account.id" :value="account.id">
              {{ account.name }}
            </option>
          </select>
        </div>

        <div class="card-actions">
          <label class="file-picker">
            <input type="file" accept=".csv,text/csv" @change="readCsvFile($event, 'positions')">
            <span>选择 CSV 文件</span>
          </label>
          <label class="check-option">
            <input v-model="initialPositionsForm.hasHeader" type="checkbox">
            <span>首行包含表头</span>
          </label>
        </div>

        <div v-if="initialPositionsForm.fileName" class="file-name">
          已载入：{{ initialPositionsForm.fileName }}
        </div>

        <div class="form-group">
          <label>CSV 内容</label>
          <textarea
            v-model="initialPositionsForm.csvContent"
            rows="10"
            placeholder="symbol,name,type,quantity,costPrice,currentPrice"
          ></textarea>
        </div>

        <div class="template-box">
          <span class="template-title">模板</span>
          <pre>{{ initialPositionsTemplate }}</pre>
        </div>

        <div v-if="initialPositionsSummary" class="result-box success">
          {{ initialPositionsSummary }}
        </div>

        <div class="card-footer">
          <span class="helper-text">导入前请先确认账户现金已经调整成“剩余现金”。</span>
          <button class="btn-primary" :disabled="!canImportInitialPositions" @click="submitInitialPositionsImport">
            导入初始持仓
          </button>
        </div>
      </section>

      <section class="import-card">
        <div class="card-header">
          <div>
            <h2>批量交易导入</h2>
            <p>适合把历史成交记录批量入账，统一驱动现金和持仓变化。</p>
          </div>
          <span class="card-badge ledger">记账导入</span>
        </div>

        <div class="form-group">
          <label>目标账户</label>
          <select v-model.number="transactionsForm.accountId">
            <option v-for="account in accounts" :key="account.id" :value="account.id">
              {{ account.name }}
            </option>
          </select>
        </div>

        <div class="card-actions">
          <label class="file-picker">
            <input type="file" accept=".csv,text/csv" @change="readCsvFile($event, 'transactions')">
            <span>选择 CSV 文件</span>
          </label>
          <label class="check-option">
            <input v-model="transactionsForm.hasHeader" type="checkbox">
            <span>首行包含表头</span>
          </label>
        </div>

        <div v-if="transactionsForm.fileName" class="file-name">
          已载入：{{ transactionsForm.fileName }}
        </div>

        <div class="form-group">
          <label>CSV 内容</label>
          <textarea
            v-model="transactionsForm.csvContent"
            rows="10"
            placeholder="tradeDate,symbol,name,assetType,type,quantity,price,remark"
          ></textarea>
        </div>

        <div class="template-box">
          <span class="template-title">模板</span>
          <pre>{{ transactionsTemplate }}</pre>
        </div>

        <div v-if="transactionsSummary" class="result-box success">
          {{ transactionsSummary }}
        </div>

        <div class="card-footer">
          <span class="helper-text">每一行都会按时间顺序调用现有结算逻辑；失败时整批回滚。</span>
          <button class="btn-primary" :disabled="!canImportTransactions" @click="submitTransactionsImport">
            导入交易记录
          </button>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { usePortfolioStore } from '@/stores/portfolio'

type ImportTarget = 'positions' | 'transactions'

interface ImportFormState {
  accountId: number
  csvContent: string
  hasHeader: boolean
  fileName: string
}

const portfolioStore = usePortfolioStore()

const loading = computed(() => portfolioStore.loading)
const error = computed(() => portfolioStore.error)
const accounts = computed(() => portfolioStore.accounts)

const createImportForm = (): ImportFormState => ({
  accountId: 0,
  csvContent: '',
  hasHeader: true,
  fileName: ''
})

const initialPositionsForm = ref<ImportFormState>(createImportForm())
const transactionsForm = ref<ImportFormState>(createImportForm())
const initialPositionsSummary = ref('')
const transactionsSummary = ref('')

const initialPositionsTemplate = `symbol,name,type,quantity,costPrice,currentPrice
510300,沪深300ETF,fund,1000,3.95,4.08
600036,招商银行,stock,200,34.50,36.10`

const transactionsTemplate = `tradeDate,symbol,name,assetType,type,quantity,price,remark
2026-04-01,510300,沪深300ETF,fund,buy,1000,3.95,首次建仓
2026-04-08,510300,沪深300ETF,fund,sell,300,4.05,阶段性止盈`

const canImportInitialPositions = computed(() => {
  return initialPositionsForm.value.accountId > 0 && initialPositionsForm.value.csvContent.trim().length > 0
})

const canImportTransactions = computed(() => {
  return transactionsForm.value.accountId > 0 && transactionsForm.value.csvContent.trim().length > 0
})

const syncDefaultAccount = () => {
  const defaultAccountId = accounts.value[0]?.id ?? 0

  if (defaultAccountId > 0 && initialPositionsForm.value.accountId === 0) {
    initialPositionsForm.value.accountId = defaultAccountId
  }

  if (defaultAccountId > 0 && transactionsForm.value.accountId === 0) {
    transactionsForm.value.accountId = defaultAccountId
  }
}

const readCsvFile = async (event: Event, target: ImportTarget) => {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) {
    return
  }

  const content = await file.text()
  const form = target === 'positions' ? initialPositionsForm.value : transactionsForm.value
  form.csvContent = content
  form.fileName = file.name
  input.value = ''
}

const submitInitialPositionsImport = async () => {
  const result = await portfolioStore.importInitialPositions({
    accountId: initialPositionsForm.value.accountId,
    csvContent: initialPositionsForm.value.csvContent,
    hasHeader: initialPositionsForm.value.hasHeader
  })

  initialPositionsSummary.value =
    `已导入 ${result.importedCount} 条初始持仓，当前持仓总成本 ¥${result.totalCostBasis.toFixed(2)}，最新市值 ¥${result.totalMarketValue.toFixed(2)}。`
}

const submitTransactionsImport = async () => {
  const result = await portfolioStore.importTransactions({
    accountId: transactionsForm.value.accountId,
    csvContent: transactionsForm.value.csvContent,
    hasHeader: transactionsForm.value.hasHeader
  })

  transactionsSummary.value =
    `已导入 ${result.importedCount} 条交易，其中买入 ${result.buyCount} 条、卖出 ${result.sellCount} 条，累计交易额 ¥${result.totalAmount.toFixed(2)}。`
}

onMounted(async () => {
  await portfolioStore.fetchAccounts()
  syncDefaultAccount()
})
</script>

<style scoped>
.imports {
  padding: 30px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  gap: 20px;
  flex-wrap: wrap;
}

.header-content h1 {
  margin: 0 0 6px 0;
  font-size: 26px;
  font-weight: 700;
  color: #1a1a2e;
}

.subtitle {
  margin: 0;
  color: #6b7280;
  font-size: 14px;
}

.notice-banner {
  margin-bottom: 24px;
  padding: 14px 18px;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.12) 0%, rgba(16, 185, 129, 0.08) 100%);
  color: #4b5563;
  font-size: 14px;
  line-height: 1.7;
}

.imports-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 24px;
}

.import-card {
  background: #fff;
  border-radius: 22px;
  padding: 24px;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
}

.card-header h2 {
  margin: 0 0 8px 0;
  font-size: 20px;
  color: #111827;
}

.card-header p {
  margin: 0;
  color: #6b7280;
  font-size: 14px;
  line-height: 1.6;
}

.card-badge {
  display: inline-flex;
  align-items: center;
  padding: 8px 12px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
  white-space: nowrap;
}

.card-badge.baseline {
  background: rgba(59, 130, 246, 0.12);
  color: #2563eb;
}

.card-badge.ledger {
  background: rgba(16, 185, 129, 0.12);
  color: #059669;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 14px;
  font-weight: 600;
  color: #374151;
}

.form-group select,
.form-group textarea {
  width: 100%;
  border: 1px solid #dbe3ef;
  border-radius: 14px;
  padding: 14px 16px;
  font-size: 14px;
  color: #111827;
  transition: all 0.25s ease;
}

.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.12);
}

.form-group textarea {
  resize: vertical;
  min-height: 220px;
  font-family: 'Consolas', 'SFMono-Regular', monospace;
  line-height: 1.55;
}

.card-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.file-picker {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 12px 16px;
  border-radius: 12px;
  background: #f3f6fb;
  color: #334155;
  cursor: pointer;
  font-weight: 600;
  overflow: hidden;
}

.file-picker input {
  position: absolute;
  inset: 0;
  opacity: 0;
  cursor: pointer;
}

.check-option {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #4b5563;
  font-size: 14px;
}

.file-name {
  padding: 10px 12px;
  border-radius: 12px;
  background: #f8fafc;
  color: #475569;
  font-size: 13px;
}

.template-box {
  padding: 16px;
  border-radius: 16px;
  background: linear-gradient(135deg, rgba(15, 23, 42, 0.96) 0%, rgba(30, 41, 59, 0.96) 100%);
  color: #e2e8f0;
}

.template-title {
  display: inline-block;
  margin-bottom: 10px;
  font-size: 12px;
  font-weight: 700;
  color: #93c5fd;
  letter-spacing: 0.04em;
}

.template-box pre {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
  font-size: 13px;
  line-height: 1.6;
}

.result-box {
  padding: 14px 16px;
  border-radius: 14px;
  font-size: 14px;
  line-height: 1.6;
}

.result-box.success {
  background: rgba(16, 185, 129, 0.12);
  color: #047857;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.helper-text {
  color: #6b7280;
  font-size: 13px;
  line-height: 1.6;
  flex: 1;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  padding: 12px 24px;
  border-radius: 12px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: 0 4px 14px rgba(102, 126, 234, 0.35);
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 22px rgba(102, 126, 234, 0.4);
}

.btn-primary:disabled {
  opacity: 0.55;
  cursor: not-allowed;
  box-shadow: none;
}

.loading-container,
.error-container {
  text-align: center;
  padding: 60px 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
}

.error-container {
  color: #ef4444;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 3px solid #f3f3f3;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 1024px) {
  .imports-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .imports {
    padding: 20px;
  }

  .card-header,
  .card-footer,
  .card-actions {
    flex-direction: column;
    align-items: stretch;
  }

  .btn-primary {
    width: 100%;
  }
}
</style>
