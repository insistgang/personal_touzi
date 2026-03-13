<template>
  <div class="ai-chat">
    <div class="chat-header">
      <h3>AI 投资助手</h3>
      <button @click="clearHistory" class="clear-btn">清空对话</button>
    </div>

    <div class="chat-messages" ref="messagesContainer">
      <div
        v-for="(msg, index) in messages"
        :key="index"
        :class="['message', msg.role]"
      >
        <div class="message-content">
          <div class="message-role">{{ msg.role === 'user' ? '我' : 'AI助手' }}</div>
          <div class="message-text">{{ msg.content }}</div>
        </div>
      </div>
      <div v-if="loading" class="message assistant">
        <div class="message-content">
          <div class="message-role">AI助手</div>
          <div class="message-text typing">正在思考中...</div>
        </div>
      </div>
    </div>

    <div class="quick-questions" v-if="messages.length === 0">
      <button
        v-for="q in quickQuestions"
        :key="q"
        @click="sendQuickQuestion(q)"
        class="quick-btn"
      >
        {{ q }}
      </button>
    </div>

    <div class="chat-input">
      <textarea
        v-model="inputMessage"
        @keydown.enter.exact.prevent="sendMessage"
        placeholder="输入您的问题，按 Enter 发送..."
        rows="2"
      />
      <button @click="sendMessage" :disabled="!inputMessage.trim() || loading">
        发送
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue'
import api, { apiAI } from '@/api/index.ts'

interface Message {
  role: 'user' | 'assistant'
  content: string
}

const quickQuestions = [
  '分析我的持仓',
  '预测未来收益',
  '评估投资风险',
  '市场行情分析',
  '优化投资建议'
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

const sendMessage = async () => {
  const text = inputMessage.value.trim()
  if (!text || loading.value) return

  messages.value.push({ role: 'user', content: text })
  inputMessage.value = ''
  loading.value = true
  scrollToBottom()

  try {
    const response = await apiAI.chat(text)
    messages.value.push({
      role: 'assistant',
      content: response.message || response.data?.content || '抱歉，我暂时无法回答这个问题。'
    })
  } catch (error) {
    console.error('Chat error:', error)
    messages.value.push({
      role: 'assistant',
      content: '抱歉，发生了错误，请稍后再试。'
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
  messages.value = []
}

onMounted(() => {
  // 欢迎消息
  messages.value.push({
    role: 'assistant',
    content: '您好！我是您的 AI 投资助手。我可以帮您分析持仓、预测收益、评估风险等。请问有什么可以帮您的吗？'
  })
  scrollToBottom()
})
</script>

<style scoped>
.ai-chat {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #eee;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
}

.chat-header h3 {
  margin: 0;
  font-size: 16px;
}

.clear-btn {
  padding: 4px 12px;
  border: 1px solid rgba(255, 255, 255, 0.5);
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.3s;
}

.clear-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

.message {
  margin-bottom: 16px;
  display: flex;
}

.message.user {
  justify-content: flex-end;
}

.message-content {
  max-width: 80%;
  padding: 12px 16px;
  border-radius: 8px;
}

.message.user .message-content {
  background: #667eea;
  color: #fff;
}

.message.assistant .message-content {
  background: #f5f5f5;
  color: #333;
}

.message-role {
  font-size: 11px;
  opacity: 0.7;
  margin-bottom: 4px;
}

.message-text {
  font-size: 14px;
  line-height: 1.5;
}

.message-text.typing {
  color: #999;
  font-style: italic;
}

.quick-questions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 12px 20px;
  border-top: 1px solid #eee;
  border-bottom: 1px solid #eee;
}

.quick-btn {
  padding: 6px 12px;
  border: 1px solid #ddd;
  background: #f9f9f9;
  border-radius: 16px;
  cursor: pointer;
  font-size: 12px;
  color: #666;
  transition: all 0.3s;
}

.quick-btn:hover {
  background: #667eea;
  color: #fff;
  border-color: #667eea;
}

.chat-input {
  display: flex;
  gap: 8px;
  padding: 12px 20px;
  border-top: 1px solid #eee;
}

.chat-input textarea {
  flex: 1;
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  resize: none;
  font-family: inherit;
  font-size: 14px;
  outline: none;
}

.chat-input textarea:focus {
  border-color: #667eea;
}

.chat-input button {
  padding: 8px 20px;
  background: #667eea;
  color: #fff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s;
}

.chat-input button:hover:not(:disabled) {
  background: #5568d3;
}

.chat-input button:disabled {
  background: #ccc;
  cursor: not-allowed;
}
</style>
