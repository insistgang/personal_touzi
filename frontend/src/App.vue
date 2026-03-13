<template>
  <div id="app">
    <nav class="navbar">
      <div class="nav-brand">
        <div class="brand-icon">
          <svg width="32" height="32" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect width="32" height="32" rx="8" fill="url(#gradient)"/>
            <path d="M16 8L20 12H18V16H20L16 20L12 16H14V12H12L16 8Z" fill="white"/>
            <defs>
              <linearGradient id="gradient" x1="0" y1="0" x2="32" y2="32" gradientUnits="userSpaceOnUse">
                <stop stop-color="#667eea"/>
                <stop offset="1" stop-color="#764ba2"/>
              </linearGradient>
            </defs>
          </svg>
        </div>
        <span class="brand-text">投资组合管理</span>
      </div>
      <div class="nav-links">
        <router-link to="/dashboard" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="7" height="7"/>
            <rect x="14" y="3" width="7" height="7"/>
            <rect x="14" y="14" width="7" height="7"/>
            <rect x="3" y="14" width="7" height="7"/>
          </svg>
          <span>仪表盘</span>
        </router-link>
        <router-link to="/positions" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 3v18h18"/>
            <path d="M18 9l-5 5-4-4-3 3"/>
          </svg>
          <span>持仓</span>
        </router-link>
        <router-link to="/transactions" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 2v20M17 7l-5-5-5 5M17 17l-5 5-5-5"/>
          </svg>
          <span>交易记录</span>
        </router-link>
        <router-link to="/accounts" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
            <circle cx="12" cy="7" r="4"/>
          </svg>
          <span>账户</span>
        </router-link>
        <router-link to="/ai" class="nav-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 2L2 7l10 5 10-5-10-5z"/>
            <path d="M2 17l10 5 10-5"/>
            <path d="M2 12l10 5 10-5"/>
          </svg>
          <span>AI 分析</span>
        </router-link>
      </div>
      <div class="nav-actions">
        <button class="icon-btn" title="设置">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="3"/>
            <path d="M12 1v6m0 6v6m9-9h-6m-6 0H3m15.364 6.364l-4.243-4.243M9.879 9.879L5.636 5.636m12.728 12.728l-4.243-4.243M9.879 14.121l-4.243 4.243"/>
          </svg>
        </button>
        <div class="user-avatar">
          <svg width="32" height="32" viewBox="0 0 32 32" fill="none">
            <circle cx="16" cy="16" r="16" fill="url(#avatarGradient)"/>
            <path d="M16 14c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" fill="white"/>
            <defs>
              <linearGradient id="avatarGradient" x1="0" y1="0" x2="32" y2="32" gradientUnits="userSpaceOnUse">
                <stop stop-color="#667eea"/>
                <stop offset="1" stop-color="#764ba2"/>
              </linearGradient>
            </defs>
          </svg>
        </div>
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
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
  color: #333;
  min-height: 100vh;
}

#app {
  min-height: 100vh;
}

.navbar {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.3);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 30px;
  height: 70px;
  position: sticky;
  top: 0;
  z-index: 100;
  backdrop-filter: blur(10px);
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.brand-icon {
  display: flex;
  align-items: center;
}

.brand-text {
  font-size: 20px;
  font-weight: 700;
  color: #fff;
  letter-spacing: 0.5px;
}

.nav-links {
  display: flex;
  gap: 8px;
  align-items: center;
}

.nav-link {
  text-decoration: none;
  color: rgba(255, 255, 255, 0.85);
  padding: 10px 18px;
  border-radius: 12px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
  font-size: 14px;
  position: relative;
  overflow: hidden;
}

.nav-link::before {
  content: '';
  position: absolute;
  inset: 0;
  background: rgba(255, 255, 255, 0.1);
  opacity: 0;
  transition: opacity 0.3s;
  border-radius: 12px;
}

.nav-link:hover::before {
  opacity: 1;
}

.nav-link:hover,
.nav-link.router-link-active {
  color: #fff;
  background: rgba(255, 255, 255, 0.2);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.nav-link.router-link-active {
  background: rgba(255, 255, 255, 0.25);
  font-weight: 600;
}

.nav-link svg {
  flex-shrink: 0;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.icon-btn {
  background: rgba(255, 255, 255, 0.15);
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s;
  color: #fff;
}

.icon-btn:hover {
  background: rgba(255, 255, 255, 0.25);
  transform: rotate(45deg);
}

.user-avatar {
  cursor: pointer;
  transition: transform 0.3s;
}

.user-avatar:hover {
  transform: scale(1.05);
}

.main-content {
  flex: 1;
  min-height: calc(100vh - 70px);
}

@media (max-width: 1024px) {
  .navbar {
    padding: 0 20px;
  }

  .nav-links {
    gap: 4px;
  }

  .nav-link {
    padding: 8px 12px;
  }

  .nav-link span {
    display: none;
  }

  .nav-link svg {
    margin: 0;
  }
}

@media (max-width: 768px) {
  .brand-text {
    display: none;
  }

  .nav-actions {
    gap: 8px;
  }
}
</style>
