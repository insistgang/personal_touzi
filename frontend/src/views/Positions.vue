<template>
  <div class="positions">
    <div class="page-header">
      <div class="header-content">
        <h1>持仓管理</h1>
        <p class="subtitle">管理和跟踪您的投资持仓</p>
      </div>
      <button class="btn-primary" @click="showAddForm = true">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"/>
          <line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        添加持仓
      </button>
    </div>

    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>加载中...</p>
    </div>
    <div v-else-if="error" class="error-container">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <p>{{ error }}</p>
    </div>
    <div v-else class="positions-content">
      <!-- 统计卡片 -->
      <div class="stats-cards">
        <div class="stat-card">
          <div class="stat-icon blue">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
              <circle cx="9" cy="7" r="4"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">总持仓数</span>
            <span class="stat-value">{{ positions?.length || 0 }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon green">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">盈利持仓</span>
            <span class="stat-value">{{ profitablePositions }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon red">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M23 6l-9.5 9.5-5-5L1 18"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">亏损持仓</span>
            <span class="stat-value">{{ losingPositions }}</span>
          </div>
        </div>
      </div>

      <!-- 持仓表格 -->
      <div class="positions-table-card">
        <div class="table-header">
          <h3>持仓列表</h3>
          <div class="table-filters">
            <input type="text" placeholder="搜索代码或名称..." class="search-input" v-model="searchQuery">
            <select class="filter-select" v-model="filterType">
              <option value="all">全部类型</option>
              <option value="stock">股票</option>
              <option value="fund">基金</option>
              <option value="bond">债券</option>
            </select>
          </div>
        </div>
        <div class="table-container">
          <table class="positions-table">
            <thead>
              <tr>
                <th>代码</th>
                <th>名称</th>
                <th>类型</th>
                <th>持仓数量</th>
                <th>成本价</th>
                <th>现价</th>
                <th>市值</th>
                <th>盈亏</th>
                <th>盈亏比例</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="position in filteredPositions" :key="position.id" class="position-row">
                <td class="symbol-cell">
                  <span class="symbol-badge">{{ position.symbol }}</span>
                </td>
                <td>{{ position.name }}</td>
                <td>
                  <span class="type-badge" :class="position.type">
                    {{ getTypeName(position.type) }}
                  </span>
                </td>
                <td>{{ position.quantity }}</td>
                <td>¥{{ position.costPrice.toFixed(2) }}</td>
                <td>¥{{ position.currentPrice.toFixed(2) }}</td>
                <td>¥{{ (position.quantity * position.currentPrice).toFixed(2) }}</td>
                <td :class="getGainLossClass(position)">
                  ¥{{ (position.quantity * (position.currentPrice - position.costPrice)).toFixed(2) }}
                </td>
                <td :class="getGainLossClass(position)">
                  {{ (((position.currentPrice - position.costPrice) / position.costPrice) * 100).toFixed(2) }}%
                </td>
                <td>
                  <div class="action-buttons">
                    <button class="action-btn edit" @click="editPosition(position)" title="编辑">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                        <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                      </svg>
                    </button>
                    <button class="action-btn delete" @click="deletePosition(position.id)" title="删除">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="3 6 5 6 21 6"/>
                        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
          <div v-if="filteredPositions.length === 0" class="empty-state">
            <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
              <polyline points="3.27 6.96 12 12.01 20.73 6.96"/>
              <line x1="12" y1="22.08" x2="12" y2="12"/>
            </svg>
            <p>暂无持仓数据</p>
          </div>
        </div>
      </div>
    </div>

    <!-- 添加/编辑持仓弹窗 -->
    <div v-if="showAddForm" class="modal-overlay" @click.self="showAddForm = false">
      <div class="modal">
        <div class="modal-header">
          <h3>{{ editingPosition ? '编辑持仓' : '添加持仓' }}</h3>
          <button class="close-btn" @click="closeModal">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <form @submit.prevent="savePosition" class="modal-body">
          <div class="form-group">
            <label>证券代码</label>
            <input type="text" v-model="formData.symbol" placeholder="例如: 000001" required>
          </div>
          <div class="form-group">
            <label>证券名称</label>
            <input type="text" v-model="formData.name" placeholder="例如: 平安银行" required>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>类型</label>
              <select v-model="formData.type" required>
                <option value="stock">股票</option>
                <option value="fund">基金</option>
                <option value="bond">债券</option>
              </select>
            </div>
            <div class="form-group">
              <label>持仓数量</label>
              <input type="number" v-model.number="formData.quantity" min="0" step="100" required>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>成本价</label>
              <input type="number" v-model.number="formData.costPrice" min="0" step="0.01" required>
            </div>
            <div class="form-group">
              <label>现价</label>
              <input type="number" v-model.number="formData.currentPrice" min="0" step="0.01" required>
            </div>
          </div>
          <div class="form-group">
            <label>账户</label>
            <select v-model.number="formData.accountId" required>
              <option v-for="account in accounts" :key="account.id" :value="account.id">
                {{ account.name }}
              </option>
            </select>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn-secondary" @click="closeModal">取消</button>
            <button type="submit" class="btn-primary">
              {{ editingPosition ? '保存' : '添加' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usePortfolioStore } from '@/stores/portfolio'

const portfolioStore = usePortfolioStore()

const loading = computed(() => portfolioStore.loading)
const error = computed(() => portfolioStore.error)
const positions = computed(() => portfolioStore.positions)
const accounts = computed(() => portfolioStore.accounts)

const searchQuery = ref('')
const filterType = ref('all')
const showAddForm = ref(false)
const editingPosition = ref<any>(null)

const formData = ref<{
  symbol: string
  name: string
  type: 'stock' | 'fund' | 'bond'
  quantity: number
  costPrice: number
  currentPrice: number
  accountId: number
}>({
  symbol: '',
  name: '',
  type: 'stock',
  quantity: 0,
  costPrice: 0,
  currentPrice: 0,
  accountId: 1
})

const profitablePositions = computed(() => {
  return positions.value?.filter(p => p.currentPrice > p.costPrice).length || 0
})

const losingPositions = computed(() => {
  return positions.value?.filter(p => p.currentPrice < p.costPrice).length || 0
})

const filteredPositions = computed(() => {
  if (!positions.value) return []
  return positions.value.filter(p => {
    const matchesSearch = p.symbol.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                         p.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchesType = filterType.value === 'all' || p.type === filterType.value
    return matchesSearch && matchesType
  })
})

const getTypeName = (type?: string): string => {
  const types: Record<string, string> = {
    stock: '股票',
    fund: '基金',
    bond: '债券'
  }
  return type ? (types[type] || type) : '未知'
}

const getGainLossClass = (position: any): string => {
  const gainLoss = position.currentPrice - position.costPrice
  return gainLoss >= 0 ? 'positive' : 'negative'
}

const editPosition = (position: any) => {
  editingPosition.value = position
  formData.value = {
    symbol: position.symbol,
    name: position.name,
    type: position.type,
    quantity: position.quantity,
    costPrice: position.costPrice,
    currentPrice: position.currentPrice,
    accountId: position.accountId
  }
  showAddForm.value = true
}

const deletePosition = async (id: number) => {
  if (confirm('确定要删除这个持仓吗？')) {
    await portfolioStore.deletePosition(id)
  }
}

const savePosition = async () => {
  if (editingPosition.value) {
    await portfolioStore.updatePosition(editingPosition.value.id, formData.value)
  } else {
    await portfolioStore.addPosition(formData.value)
  }
  closeModal()
}

const closeModal = () => {
  showAddForm.value = false
  editingPosition.value = null
  formData.value = {
    symbol: '',
    name: '',
    type: 'stock',
    quantity: 0,
    costPrice: 0,
    currentPrice: 0,
    accountId: 1
  }
}

onMounted(async () => {
  await portfolioStore.fetchAccounts()
  portfolioStore.fetchPositions()
})
</script>

<style scoped>
.positions {
  padding: 30px;
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

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  padding: 12px 24px;
  border-radius: 12px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s;
  box-shadow: 0 4px 14px rgba(102, 126, 234, 0.4);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
}

.stats-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 20px;
  margin-bottom: 24px;
}

.stat-card {
  background: #fff;
  border-radius: 16px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.06);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
}

.stat-icon.blue {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.stat-icon.green {
  background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
}

.stat-icon.red {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 12px;
  color: #6b7280;
  margin-bottom: 4px;
}

.stat-value {
  font-size: 22px;
  font-weight: 700;
  color: #1a1a2e;
}

.positions-table-card {
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #f0f0f0;
  flex-wrap: wrap;
  gap: 16px;
}

.table-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #1a1a2e;
}

.table-filters {
  display: flex;
  gap: 12px;
}

.search-input {
  padding: 10px 16px;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  font-size: 14px;
  width: 200px;
  transition: all 0.3s;
}

.search-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.filter-select {
  padding: 10px 16px;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  font-size: 14px;
  background: #fff;
  cursor: pointer;
}

.table-container {
  overflow-x: auto;
}

.positions-table {
  width: 100%;
  border-collapse: collapse;
}

.positions-table th {
  padding: 16px 20px;
  text-align: left;
  font-weight: 600;
  font-size: 13px;
  color: #6b7280;
  background: #fafbfc;
  border-bottom: 1px solid #f0f0f0;
}

.positions-table td {
  padding: 16px 20px;
  border-bottom: 1px solid #f5f5f5;
  font-size: 14px;
}

.position-row:hover {
  background: #fafbfc;
}

.symbol-badge {
  display: inline-block;
  padding: 4px 10px;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
  color: #667eea;
  border-radius: 6px;
  font-weight: 600;
  font-size: 12px;
}

.type-badge {
  display: inline-block;
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 500;
}

.type-badge.stock {
  background: rgba(102, 126, 234, 0.1);
  color: #667eea;
}

.type-badge.fund {
  background: rgba(67, 233, 123, 0.1);
  color: #43e97b;
}

.type-badge.bond {
  background: rgba(240, 147, 251, 0.1);
  color: #f093fb;
}

.positions-table td.positive {
  color: #10b981;
  font-weight: 600;
}

.positions-table td.negative {
  color: #ef4444;
  font-weight: 600;
}

.action-buttons {
  display: flex;
  gap: 8px;
}

.action-btn {
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s;
}

.action-btn.edit {
  background: rgba(102, 126, 234, 0.1);
  color: #667eea;
}

.action-btn.edit:hover {
  background: rgba(102, 126, 234, 0.2);
}

.action-btn.delete {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
}

.action-btn.delete:hover {
  background: rgba(239, 68, 68, 0.2);
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #9ca3af;
}

.empty-state svg {
  margin-bottom: 16px;
  opacity: 0.5;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(4px);
}

.modal {
  background: #fff;
  border-radius: 20px;
  width: 100%;
  max-width: 500px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
  animation: modalSlide 0.3s ease-out;
}

@keyframes modalSlide {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  border-bottom: 1px solid #f0f0f0;
}

.modal-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #1a1a2e;
}

.close-btn {
  width: 32px;
  height: 32px;
  border: none;
  background: #f5f5f5;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #6b7280;
  transition: all 0.3s;
}

.close-btn:hover {
  background: #e5e7eb;
  color: #1a1a2e;
}

.modal-body {
  padding: 24px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #374151;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  font-size: 14px;
  transition: all 0.3s;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

.btn-secondary {
  padding: 12px 24px;
  border: 1px solid #e5e7eb;
  background: #fff;
  color: #374151;
  border-radius: 10px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-secondary:hover {
  background: #f9fafb;
  border-color: #d1d5db;
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

.error-container svg {
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

@media (max-width: 768px) {
  .positions {
    padding: 20px;
  }

  .stats-cards {
    grid-template-columns: 1fr;
  }

  .table-filters {
    flex-direction: column;
  }

  .search-input {
    width: 100%;
  }

  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
