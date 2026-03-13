<template>
  <div class="position-table">
    <div class="table-header">
      <h3>持仓明细</h3>
      <div class="table-stats" v-if="positions.length > 0">
        <div class="stat-item">
          <span class="label">持仓数</span>
          <span class="value">{{ positions.length }}</span>
        </div>
        <div class="stat-item">
          <span class="label">总市值</span>
          <span class="value">¥{{ totalMarketValue.toLocaleString() }}</span>
        </div>
        <div class="stat-item" :class="totalGainLoss >= 0 ? 'positive' : 'negative'">
          <span class="label">总盈亏</span>
          <span class="value">{{ totalGainLoss >= 0 ? '+' : '' }}¥{{ totalGainLoss.toLocaleString() }}</span>
        </div>
      </div>
    </div>

    <div class="table-wrapper" v-if="positions.length > 0">
      <table>
        <thead>
          <tr>
            <th>代码</th>
            <th>名称</th>
            <th>持仓</th>
            <th>成本价</th>
            <th>现价</th>
            <th>市值</th>
            <th>盈亏</th>
            <th>盈亏比例</th>
            <th class="actions-th">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="position in positions" :key="position.id" class="position-row">
            <td class="symbol-cell">
              <span class="symbol-badge">{{ position.symbol }}</span>
            </td>
            <td class="name-cell">{{ position.name }}</td>
            <td class="quantity-cell">{{ position.quantity }}</td>
            <td class="cost-cell">¥{{ position.avgCost.toFixed(2) }}</td>
            <td class="price-cell">
              <span :class="position.currentPrice >= position.avgCost ? 'price-up' : 'price-down'">
                ¥{{ position.currentPrice.toFixed(2) }}
              </span>
            </td>
            <td class="value-cell">¥{{ position.marketValue.toFixed(2) }}</td>
            <td class="gain-loss-cell" :class="position.gainLoss >= 0 ? 'positive' : 'negative'">
              {{ position.gainLoss >= 0 ? '+' : '' }}¥{{ position.gainLoss.toFixed(2) }}
            </td>
            <td class="percent-cell">
              <span class="percent-badge" :class="position.gainLossPercent >= 0 ? 'positive' : 'negative'">
                {{ position.gainLossPercent >= 0 ? '+' : '' }}{{ position.gainLossPercent.toFixed(2) }}%
              </span>
            </td>
            <td class="actions-cell">
              <div class="action-buttons">
                <button @click="$emit('edit', position)" class="action-btn edit-btn" title="编辑">
                  <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                    <path d="M2 10.5V12.5H4L10.5 6L8 3.5L1.5 10H2V10.5Z" stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
                    <path d="M7.5 4L9.5 6" stroke="currentColor" stroke-width="1" stroke-linecap="round"/>
                  </svg>
                </button>
                <button @click="$emit('delete', position)" class="action-btn delete-btn" title="删除">
                  <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                    <path d="M2.5 4H11.5M4.5 4V3C4.5 2.45 4.95 2 5.5 2H8.5C9.05 2 9.5 2.45 9.5 3V4M3.5 4L4 11.5C4.05 12.05 4.5 12.5 5 12.5H9C9.5 12.5 9.95 12.05 10 11.5L10.5 4" stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
                  </svg>
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-else class="empty-state">
      <svg width="64" height="64" viewBox="0 0 64 64" fill="none">
        <rect x="12" y="16" width="40" height="32" rx="4" stroke="#e0e0e0" stroke-width="2"/>
        <path d="M12 26H52" stroke="#e0e0e0" stroke-width="2"/>
        <circle cx="20" cy="21" r="2" fill="#e0e0e0"/>
        <circle cx="26" cy="21" r="2" fill="#e0e0e0"/>
        <path d="M24 36L28 40L38 30" stroke="#e0e0e0" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      <p>暂无持仓数据</p>
      <span class="empty-hint">添加您的第一笔持仓开始追踪收益</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Position {
  id: number
  symbol: string
  name: string
  quantity: number
  avgCost: number
  currentPrice: number
  marketValue: number
  gainLoss: number
  gainLossPercent: number
  accountId: number
}

interface Props {
  positions: Position[]
}

const props = defineProps<Props>()

defineEmits<{
  edit: [position: Position]
  delete: [position: Position]
}>()

const totalMarketValue = computed(() => {
  return props.positions.reduce((sum, p) => sum + p.marketValue, 0)
})

const totalGainLoss = computed(() => {
  return props.positions.reduce((sum, p) => sum + p.gainLoss, 0)
})
</script>

<style scoped>
.position-table {
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.08);
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
  font-size: 18px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.table-stats {
  display: flex;
  gap: 24px;
}

.stat-item {
  display: flex;
  gap: 8px;
  font-size: 14px;
}

.stat-item .label {
  color: #999;
}

.stat-item .value {
  color: #333;
  font-weight: 600;
}

.stat-item.positive .value {
  color: #f56c6c;
}

.stat-item.negative .value {
  color: #67c23a;
}

.table-wrapper {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th {
  padding: 14px 16px;
  text-align: left;
  background: #fafafa;
  font-weight: 600;
  font-size: 13px;
  color: #666;
  border-bottom: 1px solid #f0f0f0;
  white-space: nowrap;
}

th.actions-th {
  text-align: center;
  width: 100px;
}

td {
  padding: 14px 16px;
  border-bottom: 1px solid #f5f5f5;
  font-size: 14px;
  color: #333;
}

.position-row {
  transition: all 0.2s;
}

.position-row:hover {
  background: #fafafa;
}

.position-row:last-child td {
  border-bottom: none;
}

.symbol-cell {
  font-weight: 500;
}

.symbol-badge {
  padding: 4px 10px;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
  border-radius: 6px;
  color: #667eea;
  font-weight: 600;
  font-size: 13px;
}

.name-cell {
  color: #666;
}

.quantity-cell {
  font-weight: 500;
}

.cost-cell {
  color: #999;
  font-family: 'SF Mono', 'Monaco', 'Consolas', monospace;
}

.price-cell {
  font-family: 'SF Mono', 'Monaco', 'Consolas', monospace;
  font-weight: 600;
}

.price-up {
  color: #f56c6c;
}

.price-down {
  color: #67c23a;
}

.value-cell {
  font-weight: 500;
  font-family: 'SF Mono', 'Monaco', 'Consolas', monospace;
}

.gain-loss-cell {
  font-weight: 600;
  font-family: 'SF Mono', 'Monaco', 'Consolas', monospace;
}

.gain-loss-cell.positive {
  color: #f56c6c;
}

.gain-loss-cell.negative {
  color: #67c23a;
}

.percent-cell {
  text-align: center;
}

.percent-badge {
  display: inline-block;
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  font-family: 'SF Mono', 'Monaco', 'Consolas', monospace;
}

.percent-badge.positive {
  background: rgba(245, 108, 108, 0.1);
  color: #f56c6c;
}

.percent-badge.negative {
  background: rgba(103, 194, 58, 0.1);
  color: #67c23a;
}

.actions-cell {
  text-align: center;
}

.action-buttons {
  display: flex;
  justify-content: center;
  gap: 8px;
}

.action-btn {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.edit-btn {
  background: rgba(102, 126, 234, 0.1);
  color: #667eea;
}

.edit-btn:hover {
  background: #667eea;
  color: #fff;
}

.delete-btn {
  background: rgba(245, 108, 108, 0.1);
  color: #f56c6c;
}

.delete-btn:hover {
  background: #f56c6c;
  color: #fff;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #999;
}

.empty-state svg {
  margin-bottom: 16px;
}

.empty-state p {
  margin: 0 0 8px 0;
  font-size: 16px;
  color: #666;
}

.empty-hint {
  font-size: 13px;
  color: #999;
}

@media (max-width: 768px) {
  .table-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .table-stats {
    flex-wrap: wrap;
    gap: 16px;
  }
}
</style>
