<template>
  <div class="accounts-page">
    <div class="page-header">
      <div class="header-copy">
        <h1>账户管理</h1>
        <p class="subtitle">维护资金账户、查看账户层级资产分布，并统一管理可用现金。</p>
      </div>
      <button class="btn-primary" @click="openCreateModal">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19" />
          <line x1="5" y1="12" x2="19" y2="12" />
        </svg>
        新建账户
      </button>
    </div>

    <div v-if="actionMessage" class="message success">{{ actionMessage }}</div>
    <div v-if="actionError" class="message error">{{ actionError }}</div>

    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>加载账户数据中...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10" />
        <line x1="12" y1="8" x2="12" y2="12" />
        <line x1="12" y1="16" x2="12.01" y2="16" />
      </svg>
      <p>{{ error }}</p>
    </div>

    <template v-else>
      <div class="stats-grid">
        <div class="stat-card">
          <span class="stat-label">账户数量</span>
          <span class="stat-value">{{ accounts.length }}</span>
        </div>
        <div class="stat-card">
          <span class="stat-label">现金余额</span>
          <span class="stat-value">¥{{ formatAmount(totalBalance) }}</span>
        </div>
        <div class="stat-card">
          <span class="stat-label">持仓市值</span>
          <span class="stat-value">¥{{ formatAmount(totalPositionsValue) }}</span>
        </div>
        <div class="stat-card emphasis">
          <span class="stat-label">总资产</span>
          <span class="stat-value">¥{{ formatAmount(totalAssets) }}</span>
        </div>
      </div>

      <div class="accounts-table-card">
        <div class="table-header">
          <h3>账户列表</h3>
          <span class="table-hint">删除账户前需要先清空该账户下的持仓和交易记录。</span>
        </div>

        <div v-if="accounts.length === 0" class="empty-state">
          <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <rect x="3" y="4" width="18" height="16" rx="2" />
            <path d="M7 8h10M7 12h6M7 16h4" />
          </svg>
          <p>还没有账户数据</p>
          <span>先创建一个账户，再继续录入持仓和交易。</span>
        </div>

        <div v-else class="table-wrapper">
          <table class="accounts-table">
            <thead>
              <tr>
                <th>账户名称</th>
                <th>券商 / 备注</th>
                <th>现金余额</th>
                <th>持仓市值</th>
                <th>总资产</th>
                <th>持仓数</th>
                <th>浮动盈亏</th>
                <th>创建时间</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="account in accounts" :key="account.id">
                <td>
                  <div class="account-name">
                    <span class="name">{{ account.name }}</span>
                    <span class="type">{{ account.type }}</span>
                  </div>
                </td>
                <td>{{ account.broker || '未填写' }}</td>
                <td>¥{{ formatAmount(account.balance) }}</td>
                <td>¥{{ formatAmount(account.positionsValue) }}</td>
                <td class="total-assets">¥{{ formatAmount(account.totalAssets) }}</td>
                <td>{{ account.positionCount }}</td>
                <td :class="account.gainLoss >= 0 ? 'positive' : 'negative'">
                  {{ account.gainLoss >= 0 ? '+' : '' }}¥{{ formatAmount(account.gainLoss) }}
                </td>
                <td>{{ formatDate(account.createdAt) }}</td>
                <td>
                  <div class="action-buttons">
                    <button class="action-btn edit" @click="openEditModal(account)">编辑</button>
                    <button class="action-btn delete" @click="removeAccount(account)">删除</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal">
        <div class="modal-header">
          <h3>{{ editingAccount ? '编辑账户' : '新建账户' }}</h3>
          <button class="close-btn" @click="closeModal">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>

        <form class="modal-body" @submit.prevent="saveAccount">
          <div class="form-group">
            <label>账户名称</label>
            <input v-model="formData.name" type="text" placeholder="例如：主账户 / 招商证券" required />
          </div>

          <div class="form-group">
            <label>券商 / 备注</label>
            <input v-model="formData.broker" type="text" placeholder="例如：华泰证券、长期投资账户" />
          </div>

          <div class="form-group">
            <label>现金余额</label>
            <input v-model.number="formData.balance" type="number" min="0" step="0.01" placeholder="0.00" required />
          </div>

          <div class="modal-footer">
            <button type="button" class="btn-secondary" @click="closeModal">取消</button>
            <button type="submit" class="btn-primary">{{ editingAccount ? '保存修改' : '创建账户' }}</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { usePortfolioStore } from '@/stores/portfolio'
import type { Account, AccountPayload } from '@/api'

const portfolioStore = usePortfolioStore()

const loading = computed(() => portfolioStore.loading)
const error = computed(() => portfolioStore.error)
const accounts = computed(() => portfolioStore.accounts)

const totalBalance = computed(() => accounts.value.reduce((sum, account) => sum + account.balance, 0))
const totalPositionsValue = computed(() => accounts.value.reduce((sum, account) => sum + account.positionsValue, 0))
const totalAssets = computed(() => accounts.value.reduce((sum, account) => sum + account.totalAssets, 0))

const showModal = ref(false)
const editingAccount = ref<Account | null>(null)
const actionMessage = ref('')
const actionError = ref('')

const createEmptyForm = (): AccountPayload => ({
  name: '',
  broker: '',
  balance: 0
})

const formData = ref<AccountPayload>(createEmptyForm())

const formatAmount = (value: number) =>
  value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

const formatDate = (value: string) =>
  new Date(value).toLocaleDateString('zh-CN')

const resetMessages = () => {
  actionMessage.value = ''
  actionError.value = ''
}

const openCreateModal = () => {
  resetMessages()
  editingAccount.value = null
  formData.value = createEmptyForm()
  showModal.value = true
}

const openEditModal = (account: Account) => {
  resetMessages()
  editingAccount.value = account
  formData.value = {
    name: account.name,
    broker: account.broker,
    balance: account.balance
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
  editingAccount.value = null
  formData.value = createEmptyForm()
}

const saveAccount = async () => {
  resetMessages()

  try {
    if (editingAccount.value) {
      await portfolioStore.updateAccount(editingAccount.value.id, formData.value)
      actionMessage.value = '账户已更新。'
    } else {
      await portfolioStore.addAccount(formData.value)
      actionMessage.value = '账户已创建。'
    }

    closeModal()
  } catch (err: any) {
    actionError.value = err.response?.data?.error || err.response?.data?.message || '保存账户失败'
  }
}

const removeAccount = async (account: Account) => {
  resetMessages()

  if (!confirm(`确定删除账户“${account.name}”吗？`)) {
    return
  }

  try {
    await portfolioStore.deleteAccount(account.id)
    actionMessage.value = '账户已删除。'
  } catch (err: any) {
    actionError.value = err.response?.data?.error || err.response?.data?.message || '删除账户失败'
  }
}

onMounted(async () => {
  await portfolioStore.fetchAccounts()
  await portfolioStore.fetchDashboard()
})
</script>

<style scoped>
.accounts-page {
  max-width: 1400px;
  margin: 0 auto;
  padding: 30px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 20px;
  margin-bottom: 24px;
}

.header-copy h1 {
  margin: 0 0 6px;
  font-size: 28px;
  color: #1a1a2e;
}

.subtitle {
  margin: 0;
  max-width: 680px;
  color: #6b7280;
  line-height: 1.6;
}

.message {
  padding: 14px 16px;
  border-radius: 12px;
  margin-bottom: 16px;
  font-size: 14px;
}

.message.success {
  background: rgba(16, 185, 129, 0.12);
  color: #047857;
}

.message.error {
  background: rgba(239, 68, 68, 0.12);
  color: #b91c1c;
}

.btn-primary,
.btn-secondary {
  border: none;
  border-radius: 12px;
  padding: 12px 20px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.24);
}

.btn-primary:hover {
  transform: translateY(-1px);
}

.btn-secondary {
  background: #f3f4f6;
  color: #374151;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.stat-card {
  padding: 20px;
  border-radius: 18px;
  background: #fff;
  box-shadow: 0 6px 24px rgba(15, 23, 42, 0.06);
}

.stat-card.emphasis {
  background: linear-gradient(135deg, #1f3c88 0%, #2c7be5 100%);
  color: #fff;
}

.stat-label {
  display: block;
  margin-bottom: 8px;
  color: #6b7280;
  font-size: 13px;
}

.stat-card.emphasis .stat-label,
.stat-card.emphasis .stat-value {
  color: #fff;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #111827;
}

.accounts-table-card {
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 8px 30px rgba(15, 23, 42, 0.08);
  overflow: hidden;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  border-bottom: 1px solid #eef2f7;
}

.table-header h3 {
  margin: 0;
  font-size: 18px;
  color: #111827;
}

.table-hint {
  color: #6b7280;
  font-size: 13px;
}

.table-wrapper {
  overflow-x: auto;
}

.accounts-table {
  width: 100%;
  border-collapse: collapse;
}

.accounts-table th,
.accounts-table td {
  padding: 16px 18px;
  text-align: left;
  border-bottom: 1px solid #f3f4f6;
  white-space: nowrap;
}

.accounts-table th {
  font-size: 12px;
  color: #6b7280;
  background: #fafafa;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.account-name {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.account-name .name {
  font-weight: 700;
  color: #111827;
}

.account-name .type {
  font-size: 12px;
  color: #6b7280;
}

.total-assets {
  font-weight: 700;
}

.positive {
  color: #059669;
}

.negative {
  color: #dc2626;
}

.action-buttons {
  display: flex;
  gap: 8px;
}

.action-btn {
  border: none;
  border-radius: 10px;
  padding: 8px 12px;
  font-size: 13px;
  cursor: pointer;
}

.action-btn.edit {
  background: rgba(37, 99, 235, 0.12);
  color: #1d4ed8;
}

.action-btn.delete {
  background: rgba(239, 68, 68, 0.12);
  color: #b91c1c;
}

.loading-container,
.error-container,
.empty-state {
  padding: 60px 20px;
  text-align: center;
  color: #6b7280;
}

.spinner {
  width: 46px;
  height: 46px;
  margin: 0 auto 16px;
  border: 3px solid #e5e7eb;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.42);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  z-index: 1000;
}

.modal {
  width: 100%;
  max-width: 520px;
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 20px 60px rgba(15, 23, 42, 0.2);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 22px 24px;
  border-bottom: 1px solid #eef2f7;
}

.modal-header h3 {
  margin: 0;
  font-size: 18px;
}

.close-btn {
  border: none;
  background: #f3f4f6;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  cursor: pointer;
}

.modal-body {
  padding: 24px;
}

.form-group {
  margin-bottom: 18px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #374151;
  font-size: 14px;
  font-weight: 600;
}

.form-group input {
  width: 100%;
  padding: 12px 14px;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  font-size: 14px;
}

.form-group input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.12);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 1024px) {
  .stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .accounts-page {
    padding: 20px;
  }

  .page-header,
  .table-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }
}
</style>
