import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src')
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5297',
        changeOrigin: true
      },
      '/portfolio': {
        target: 'http://localhost:5297/api',
        changeOrigin: true,
        rewrite: (path) => path
      }
    }
  }
})
