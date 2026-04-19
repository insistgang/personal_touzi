<template>
  <div class="ai-chat">
    <div class="chat-header">
      <div class="header-left">
        <div class="ai-avatar">🤖</div>
        <div class="header-info">
          <h3>AI 投资助手</h3>
          <span class="status">在线</span>
        </div>
      </div>
      <button @click="clearHistory" class="clear-btn">清空对话</button>
    </div>

    <div class="chat-messages" ref="messagesContainer">
      <div
        v-for="(msg, index) in messages"
        :key="index"
        :class="['message', msg.role]"
      >
        <div class="avatar">{{ msg.role === 'user' ? '👤' : '🤖' }}</div>
        <div class="message-content">
          <div class="message-text" v-html="formatMessage(msg.content)"></div>
        </div>
      </div>
      <div v-if="loading" class="message assistant">
        <div class="avatar">🤖</div>
        <div class="message-content">
          <div class="message-text typing">
            <span class="dot"></span>
            <span class="dot"></span>
            <span class="dot"></span>
          </div>
        </div>
      </div>
    </div>

    <div class="quick-questions" v-if="messages.length <= 1">
      <div class="quick-title">快速提问</div>
      <div class="quick-grid">
        <button
          v-for="q in quickQuestions"
          :key="q.text"
          @click="sendQuickQuestion(q.text)"
          class="quick-btn"
        >
          <span class="quick-icon">{{ q.icon }}</span>
          <span>{{ q.text }}</span>
        </button>
      </div>
    </div>

    <div class="chat-input">
      <textarea
        v-model="inputMessage"
        @keydown.enter.exact.prevent="sendMessage"
        placeholder="输入您的问题，按 Enter 发送..."
        rows="1"
      />
      <button @click="sendMessage" :disabled="!inputMessage.trim() || loading" class="send-btn">
        <span v-if="!loading">发送</span>
        <span v-else class="loading-spinner"></span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue'
import { apiAI } from '@/api/index.ts'

interface Message {
  role: 'user' | 'assistant'
  content: string
}

const quickQuestions = [
  { icon: '📊', text: '分析我的持仓' },
  { icon: '📈', text: '预测未来收益' },
  { icon: '⚠️', text: '评估投资风险' },
  { icon: '🌍', text: '市场行情分析' },
  { icon: '💡', text: '优化投资建议' },
  { icon: '📝', text: '生成投资报告' }
]

const messages = ref<Message[]>([])
const inputMessage = ref('')
const loading = ref(false)
const messagesContainer = ref<HTMLElement>()

const scrollToBottom = () => {
  nextTick(() => {
    if (messagesContainer.value) {
      messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
    }
  })
}

const formatMessage = (content: string) => {
  return content.replace(/\n/g, '<br>')
}

const sendMessage = async () => {
  const text = inputMessage.value.trim()
  if (!text || loading.value) return

  messages.value.push({ role: 'user', content: text })
  inputMessage.value = ''
  loading.value = true
  scrollToBottom()

  try {
    const response = await apiAI.chat(text)
    // 后端直接返回字符串
    const content = typeof response === 'string' ? response : (response as any).message || (response as any).content || '抱歉，我暂时无法回答这个问题。'
    messages.value.push({
      role: 'assistant',
      content: content
    })
  } catch (error: any) {
    console.error('Chat error:', error)
    messages.value.push({
      role: 'assistant',
      content: '抱歉，发生了错误：' + (error.message || '请稍后再试')
    })
  } finally {
    loading.value = false
    scrollToBottom()
  }
}

const sendQuickQuestion = (question: string) => {
  inputMessage.value = question
  sendMessage()
}

const clearHistory = () => {
  messages.value = [{
    role: 'assistant',
    content: '您好！我是您的 AI 投资助手。我可以帮您分析持仓、预测收益、评估风险等。请问有什么可以帮您的吗？'
  }]
}

defineExpose({
  askQuestion: sendQuickQuestion,
  clearHistory
})

onMounted(() => {
  clearHistory()
  scrollToBottom()
})
</script>

<style scoped>
.ai-chat {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: linear-gradient(180deg, #f8fafc 0%, #fff 100%);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.ai-avatar {
  width: 40px;
  height: 40px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
}

.header-info h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.status {
  font-size: 12px;
  opacity: 0.8;
}

.status::before {
  content: '●';
  color: #4ade80;
  margin-right: 4px;
}

.clear-btn {
  padding: 6px 14px;
  border: 1px solid rgba(255, 255, 255, 0.3);
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
  border-radius: 8px;
  cursor: pointer;
  font-size: 13px;
  transition: all 0.2s;
}

.clear-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.message {
  display: flex;
  gap: 12px;
  align-items: flex-start;
}

.message.user {
  flex-direction: row-reverse;
}

.avatar {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  flex-shrink: 0;
  background: #f1f5f9;
}

.message.user .avatar {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.message-content {
  max-width: 75%;
  padding: 14px 18px;
  border-radius: 16px;
  font-size: 14px;
  line-height: 1.6;
}

.message.user .message-content {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border-bottom-right-radius: 4px;
}

.message.assistant .message-content {
  background: #f1f5f9;
  color: #334155;
  border-bottom-left-radius: 4px;
}

.message-text.typing {
  display: flex;
  gap: 4px;
  padding: 4px 0;
}

.dot {
  width: 8px;
  height: 8px;
  background: #94a3b8;
  border-radius: 50%;
  animation: typing 1.4s infinite;
}

.dot:nth-child(2) { animation-delay: 0.2s; }
.dot:nth-child(3) { animation-delay: 0.4s; }

@keyframes typing {
  0%, 60%, 100% { transform: translateY(0); }
  30% { transform: translateY(-6px); }
}

.quick-questions {
  padding: 16px 20px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
}

.quick-title {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 12px;
  font-weight: 500;
}

.quick-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
}

.quick-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 12px;
  border: 1px solid #e2e8f0;
  background: #fff;
  border-radius: 10px;
  cursor: pointer;
  font-size: 12px;
  color: #475569;
  transition: all 0.2s;
  text-align: left;
}

.quick-btn:hover {
  border-color: #667eea;
  color: #667eea;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.15);
}

.quick-icon {
  font-size: 16px;
}

.chat-input {
  display: flex;
  gap: 10px;
  padding: 16px 20px;
  background: #fff;
  border-top: 1px solid #e2e8f0;
}

.chat-input textarea {
  flex: 1;
  padding: 12px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  resize: none;
  font-family: inherit;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
}

.chat-input textarea:focus {
  border-color: #667eea;
}

.send-btn {
  padding: 0 24px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  border-radius: 12px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.send-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.send-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
