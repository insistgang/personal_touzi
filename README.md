# 个人量化投资组合管理系统

一个基于 ASP.NET Core + Vue.js 的个人量化投资组合管理系统，支持多账户资产管理、持仓管理、交易记录和收益分析。

## 技术栈

### 后端
- ASP.NET Core 8.0 Web API
- Entity Framework Core (Code First)
- SQL Server
- Redis (行情数据缓存)
- Swagger API 文档

### 前端
- Vue.js 3 + TypeScript
- Vite
- ECharts + vue-echarts
- Vue Router
- Pinia (状态管理)
- Axios

## 功能特性

- 📊 **持仓管理**: 多账户持仓录入、实时盈亏计算
- 💹 **交易记录**: 买卖记录管理、交易统计
- 📈 **资产快照**: 历史收益曲线、资产分布分析
- 🔥 **Redis 缓存**: 行情数据缓存，响应时间 < 50ms
- 📉 **可视化图表**: 资产分布饼图、净值走势折线图

## 项目结构

```
personal_touzi/
├── backend/                    # 后端项目
│   ├── src/
│   │   ├── PersonalTouzi.Api/          # Web API 层
│   │   ├── PersonalTouzi.Core/         # 领域模型层
│   │   └── PersonalTouzi.Infrastructure/ # 基础设施层
│   └── PersonalTouzi.sln
├── frontend/                   # 前端项目
│   ├── src/
│   │   ├── views/              # 页面组件
│   │   ├── components/         # 通用组件
│   │   ├── api/                # API 请求
│   │   ├── stores/             # 状态管理
│   │   └── router/             # 路由配置
│   └── package.json
└── README.md
```

## 快速开始

### 环境要求
- .NET 8.0 SDK
- Node.js 18+
- SQL Server 2019+
- Redis 6+

### 后端启动

```bash
cd backend
dotnet restore
dotnet ef database update --project src/PersonalTouzi.Infrastructure --startup-project src/PersonalTouzi.Api
dotnet run --project src/PersonalTouzi.Api
```

API 文档: http://localhost:5000/swagger

### 前端启动

```bash
cd frontend
npm install
npm run dev
```

前端地址: http://localhost:5173

## API 接口

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | /api/positions | 获取持仓列表 |
| POST | /api/positions | 新增持仓 |
| GET | /api/transactions | 获取交易记录 |
| POST | /api/transactions | 新增交易记录 |
| GET | /api/snapshots | 获取资产快照 |
| GET | /api/snapshots/history | 获取收益曲线 |

## License

MIT
