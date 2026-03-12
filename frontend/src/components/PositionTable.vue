<template>
  <div class="position-table">
    <table v-if="positions.length > 0">
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
        </tr>
      </thead>
      <tbody>
        <tr v-for="position in positions" :key="position.id">
          <td>{{ position.symbol }}</td>
          <td>{{ position.name }}</td>
          <td>{{ position.quantity }}</td>
          <td>¥{{ position.avgCost.toFixed(2) }}</td>
          <td>¥{{ position.currentPrice.toFixed(2) }}</td>
          <td>¥{{ position.marketValue.toFixed(2) }}</td>
          <td :class="{ positive: position.gainLoss >= 0, negative: position.gainLoss < 0 }">
            ¥{{ position.gainLoss.toFixed(2) }}
          </td>
          <td :class="{ positive: position.gainLossPercent >= 0, negative: position.gainLossPercent < 0 }">
            {{ position.gainLossPercent.toFixed(2) }}%
          </td>
        </tr>
      </tbody>
    </table>
    <div v-else class="empty">
      暂无持仓数据
    </div>
  </div>
</template>

<script setup lang="ts">
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

defineProps<Props>()
</script>

<style scoped>
.position-table {
  width: 100%;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #eee;
}

th {
  background: #f5f5f5;
  font-weight: 600;
}

tr:hover {
  background: #f9f9f9;
}

.positive {
  color: #f56c6c;
}

.negative {
  color: #67c23a;
}

.empty {
  text-align: center;
  padding: 40px;
  color: #999;
}
</style>
