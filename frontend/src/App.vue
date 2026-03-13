<template>
  <div id="app">
    <nav class="navbar">
      <div class="nav-brand">投资组合管理</div>
      <div class="nav-links">
        <router-link to="/dashboard">仪表盘</router-link>
        <router-link to="/positions">持仓</router-link>
        <router-link to="/transactions">交易记录</router-link>
        <router-link to="/accounts">账户</router-link>
        <router-link to="/ai">AI 分析</router-link>
      </div>
    </nav>
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
  background: #f5f5f5;
  color: #333;
}

#app {
  min-height: 100vh;
}

.navbar {
  background: #fff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  padding: 0 20px;
  height: 60px;
}

.nav-brand {
  font-size: 18px;
  font-weight: bold;
  color: #409eff;
  margin-right: 40px;
}

.nav-links {
  display: flex;
  gap: 20px;
}

.nav-links a {
  text-decoration: none;
  color: #666;
  padding: 8px 16px;
  border-radius: 4px;
  transition: all 0.3s;
}

.nav-links a:hover,
.nav-links a.router-link-active {
  color: #409eff;
  background: rgba(64, 158, 255, 0.1);
}

.main-content {
  flex: 1;
}
</style>
