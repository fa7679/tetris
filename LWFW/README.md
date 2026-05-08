# CDR-AI-Assistant

CorelDRAW AI 助手插件 - 为 CorelDRAW 打造的 AI 增强工具

## 功能特性

- **基础工具集**：一键转曲、批量导出 PNG、智能对齐
- **AI 文案生成**：支持 OpenAI GPT、Claude 等模型
- **AI 文生图**：支持 Stable Diffusion、Midjourney API
- **AI 图生图**：选中位图进行 AI 重绘
- **翻译功能**：集成 DeepL / Google Translate

## 系统要求

- CorelDRAW 2021 / 2023 / 2024 (Windows)
- .NET 8.0 Runtime
- AI API 服务（OpenAI / Claude / Stable Diffusion）

## 项目结构

```
CDR-AI-Assistant/
├── CDR-AI-Assistant.sln
├── src/CDR-AI-Assistant/
│   ├── CDR-AI-Assistant.csproj
│   ├── CorelAddIn.cs           ← VSTA 插件入口
│   ├── MainWindow.xaml(.cs)     ← 悬浮面板 UI
│   ├── Models/                  ← 数据模型
│   └── Services/                ← 核心服务
│       ├── CDRHelper.cs         ← CDR COM 操作封装
│       ├── ConfigManager.cs     ← 配置管理
│       └── AIServices/          ← AI 服务实现
└── README.md
```

## 开发

```bash
# 还原依赖
dotnet restore

# 编译
dotnet build

# 发布
dotnet publish -c Release
```

## 使用方法

1. 编译生成 `CDR-AI-Assistant.dll`
2. 将 DLL 复制到 CorelDRAW 插件目录
3. 在 CDR 中通过宏或插件管理器加载
4. 首次使用需在"设置"中配置 API Key

## 许可证

MIT License