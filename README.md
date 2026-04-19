# 个人投资组合管理系统

一个基于 `ASP.NET Core 8 + Vue 3` 的个人投资组合管理系统，面向本地自用和继续迭代开发的场景。项目当前已经完成主流程闭环，支持账户、持仓、交易、资产统计、AI 分析，以及独立的数据导入流程。

当前状态不是空壳 Demo，而是一个可运行、可演示、可继续扩展的 `MVP / 第一版`。  
按路线图进度看：

- 第一阶段 `账务闭环` 已完成
- 第二阶段 `持仓维护分层` 已完成事项 `1、2、3`
- 第三到第五阶段尚未开始落地

## 当前已完成内容

### 1. 仪表盘与资产统计

- 展示总资产、现金、持仓市值、账面盈亏、收益率
- 展示净值走势、资产分布、重点持仓
- 后端统一通过 `PortfolioService` 生成账户汇总、仪表盘和资产快照
- 启动时如果历史快照为空，会自动补齐最近 30 天快照
- 每次关键数据变更后会刷新当日资产快照

### 2. 账户管理

- 支持账户列表、详情、新增、编辑、删除
- 删除账户前会检查是否仍有持仓或交易记录
- 前端提供完整的账户 CRUD 页面

### 3. 交易记账闭环

- 新增交易会直接更新账户现金和持仓数量
- 买入会校验账户可用现金
- 卖出会校验当前持仓数量
- 买入会自动创建或加仓持仓，并计算新的加权成本
- 卖出会自动减少或清空持仓
- 交易已经改成“记账型”规则：不支持直接编辑和删除，修正必须通过反向交易完成

### 4. 持仓维护分层

- 持仓页面已经收口为“仓位结果页”
- 持仓数量、成本、所属账户、证券代码改为只读
- 当前只允许维护：
  - `name`
  - `type`
  - `currentPrice`
- 不再支持直接新增或删除持仓

### 5. 数据导入

- 新增独立的“数据导入”页面，对应前端路由 `/imports`
- 支持两类 CSV 导入：
  - `初始持仓导入`
  - `批量交易导入`
- 初始持仓导入只允许对空账户执行，用于建立初始基线
- 批量交易导入逐笔复用现有交易结算规则
- 批量交易导入采用整批事务，任何一行失败都会整批回滚

### 6. AI 分析

- 支持 AI 对话
- 支持账户组合分析
- 支持市场情绪分析
- 支持收益预测
- 支持投资报告生成
- 已实现“有外部 GLM Key 就调用，没有就走本地内置分析”的降级模式

### 7. 自动化验证

- 后端新增本地 verifier：`backend/tests/PersonalTouzi.Verifier`
- 已覆盖的关键验证包括：
  - 买入结算
  - 卖出结算
  - 超量卖出校验
  - 初始持仓导入
  - 失败批量交易导入回滚

## 当前核心业务规则

这是现在最重要的使用边界，README 里明确写清，避免误操作。

### 交易规则

- `POST /api/portfolio/transactions` 会真正入账
- `PUT /api/portfolio/transactions/{id}` 当前会被拒绝
- `DELETE /api/portfolio/transactions/{id}` 当前会被拒绝
- 修正历史交易时，应该新增反向交易，而不是改原记录

### 持仓规则

- `POST /api/portfolio/positions` 当前会被拒绝
- `DELETE /api/portfolio/positions/{id}` 当前会被拒绝
- `PUT /api/portfolio/positions/{id}` 只允许更新展示字段和现价
- 日常加减仓应该通过“交易记录”模块完成，而不是手工改持仓

### 导入规则

- 初始持仓导入只适用于空账户
- 已有交易或持仓的账户不能再做初始持仓导入
- 批量交易导入失败时，整批不会留下半成功数据

## 技术栈

### 后端

- `ASP.NET Core 8 Web API`
- `Entity Framework Core`
- `SQLite`
- `Swagger`

### 前端

- `Vue 3`
- `TypeScript`
- `Vite`
- `Pinia`
- `Vue Router`
- `Axios`
- `ECharts + vue-echarts`

### AI

- `GLM` 可选外部调用
- 无 API Key 时自动回退到本地内置分析逻辑

## 当前页面与模块

前端已接好的主要页面如下：

- `/dashboard`：仪表盘
- `/accounts`：账户管理
- `/positions`：持仓结果页与现价维护
- `/transactions`：交易记账
- `/imports`：初始持仓 / 批量交易导入
- `/ai`：AI 分析中心

## 项目结构

```text
personal_touzi/
├── backend/
│   ├── src/
│   │   ├── PersonalTouzi.Api/                 # Web API、控制器、程序入口
│   │   ├── PersonalTouzi.Core/                # 核心实体
│   │   └── PersonalTouzi.Infrastructure/      # 数据访问、组合服务、AI 服务、导入与结算服务
│   ├── tests/
│   │   └── PersonalTouzi.Verifier/            # 本地验证程序
│   ├── PersonalTouzi.sln
│   └── CONFIGURE_AI.md
├── frontend/
│   ├── src/
│   │   ├── api/
│   │   ├── components/
│   │   ├── router/
│   │   ├── stores/
│   │   └── views/
│   └── package.json
├── docs/
│   └── superpowers/plans/                     # 路线图与计划文档
└── README.md
```

## 快速开始

### 环境要求

- `.NET 8 SDK`
- `Node.js 18+`

当前版本默认使用 `SQLite`，不需要单独安装 SQL Server 或 Redis。

### 后端启动

```bash
cd backend
dotnet restore
dotnet build PersonalTouzi.sln
dotnet run --project src/PersonalTouzi.Api
```

说明：

- 数据库文件会自动创建为 `backend/src/PersonalTouzi.Api/personaltouzi.db`
- 首次启动会自动写入测试账户、测试持仓、测试交易
- 首次启动还会自动补齐历史资产快照
- Swagger 地址为：`后端启动地址 + /swagger`

### 前端启动

```bash
cd frontend
npm install
npm run dev
```

默认开发地址通常为 `http://localhost:5173`。

## AI 配置

如果要启用外部 GLM 能力，可以参考：

- [backend/CONFIGURE_AI.md](backend/CONFIGURE_AI.md)

支持的常见配置方式：

- `.NET user-secrets`
- 环境变量 `AI__ApiKey`
- 本地 `appsettings.Development.json`

如果不配置 API Key，AI 页面依然可用，只是会使用项目内置分析逻辑而不是外部模型。

## 主要 API

### 投资组合

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/portfolio/dashboard` | 获取仪表盘汇总 |
| GET | `/api/portfolio/snapshots` | 获取资产快照 |
| GET | `/api/portfolio/net-value-trend` | 获取净值走势 |

### 账户

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/portfolio/accounts` | 获取账户列表 |
| GET | `/api/portfolio/accounts/{id}` | 获取账户详情 |
| POST | `/api/portfolio/accounts` | 创建账户 |
| PUT | `/api/portfolio/accounts/{id}` | 更新账户 |
| DELETE | `/api/portfolio/accounts/{id}` | 删除账户 |

### 持仓

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/portfolio/positions` | 获取持仓列表 |
| GET | `/api/portfolio/positions/by-account/{accountId}` | 获取指定账户持仓 |
| PUT | `/api/portfolio/positions/{id}` | 只允许更新 `name/type/currentPrice` |

### 交易

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/portfolio/transactions` | 获取交易记录 |
| GET | `/api/portfolio/transactions/by-account/{accountId}` | 获取指定账户交易记录 |
| POST | `/api/portfolio/transactions` | 新增交易并完成结算 |

### 导入

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/portfolio/imports/initial-positions` | 导入初始持仓 |
| POST | `/api/portfolio/imports/transactions` | 批量导入交易 |

### AI

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/ai/chat` | AI 对话 |
| POST | `/api/ai/analyze/{accountId}` | 组合分析 |
| POST | `/api/ai/report/{accountId}` | 投资报告 |
| POST | `/api/ai/sentiment` | 市场情绪 |
| POST | `/api/ai/predict/{accountId}` | 收益预测 |

## CSV 导入格式

### 初始持仓导入

表头：

```csv
symbol,name,type,quantity,costPrice,currentPrice
```

示例：

```csv
symbol,name,type,quantity,costPrice,currentPrice
510300,沪深300ETF,fund,1000,3.95,4.08
600036,招商银行,stock,200,34.50,36.10
```

### 批量交易导入

表头：

```csv
tradeDate,symbol,name,assetType,type,quantity,price,remark
```

示例：

```csv
tradeDate,symbol,name,assetType,type,quantity,price,remark
2026-04-01,510300,沪深300ETF,fund,buy,1000,3.95,首次建仓
2026-04-08,510300,沪深300ETF,fund,sell,300,4.05,阶段性止盈
```

## 已完成验证

当前已经实际跑过以下验证命令：

```bash
dotnet build backend/PersonalTouzi.sln
dotnet run --project backend/tests/PersonalTouzi.Verifier/PersonalTouzi.Verifier.csproj
cd frontend && npm run build
```

另外还做过真实 API 冒烟验证，确认过：

- 交易入账后现金和持仓会同步变化
- 交易不能直接编辑和删除
- 持仓不能直接新增和删除
- 初始持仓导入只允许空账户
- 失败的批量交易导入会整批回滚

## 当前开发程度

如果按“本地可运行、可演示、可继续迭代”的标准看，项目已经处于比较完整的 `MVP / 第一版` 阶段。

可以粗略理解为：

- 按个人使用 / 演示版标准：约 `75% - 80%`
- 按生产版标准：约 `50% - 60%`

## 当前未完成项

下面这些内容还没有做完，README 里保留说明，避免误解为已经生产可用：

- 还没有接入真实行情源
- 还没有落地 Redis 缓存
- 还没有登录认证和权限体系
- 还没有正式的数据迁移规范、部署方案、监控与审计体系
- 持仓维护分层的“操作说明 / 审计字段”还没做
- 当前更适合个人本地使用或继续开发，不适合直接作为多人生产系统上线

## 路线图状态

当前路线图文档在：

- [docs/superpowers/plans/2026-04-17-formal-usable-roadmap.md](docs/superpowers/plans/2026-04-17-formal-usable-roadmap.md)

截至 `2026-04-18` 的状态：

- 第一阶段 `账务闭环`：已完成
- 第二阶段 `持仓维护分层`：已完成事项 `1、2、3`
- 下一步优先项：补“操作说明 / 审计字段”，然后再进入真实行情与估值阶段

## License

MIT
