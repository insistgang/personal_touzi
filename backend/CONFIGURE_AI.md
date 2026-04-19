# AI 功能配置指南

## 配置 API Key

由于安全原因，API Key 不应该直接写在 appsettings.json 中。请使用以下方式之一配置：

### 方式 1: 使用 .NET 用户机密（推荐）

```bash
cd backend/src/PersonalTouzi.Api
dotnet user-secrets init
dotnet user-secrets set "AI:ApiKey" "your-actual-api-key"
```

### 方式 2: 使用环境变量

```bash
# Windows
set AI__ApiKey=your-actual-api-key

# Linux/macOS
export AI__ApiKey=your-actual-api-key
```

### 方式 3: 修改 appsettings.Development.json

在 `backend/src/PersonalTouzi.Api/appsettings.Development.json` 中添加：

```json
{
  "AI": {
    "ApiKey": "your-actual-api-key"
  }
}
```

**注意**: appsettings.Development.json 不会被提交到 Git（已添加到 .gitignore）。

## 获取 GLM API Key

1. 访问 [智谱 AI 开放平台](https://open.bigmodel.cn/)
2. 注册并登录账号
3. 在控制台中创建 API Key
4. 将获取的 Key 按照上述方式配置
