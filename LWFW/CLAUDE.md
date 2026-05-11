# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

CDR AI 助手 - 为 CorelDRAW Graphics Suite 27 开发的 WPF 插件，提供 AI 文案生成、图片生成、翻译等功能。

## 构建命令

```bash
cd src/CDR-AI-Assistant
dotnet build
```

## 插件部署

编译后 DLL 需要部署到 CDR Addons 目录：
- 路径：`C:\Program Files\Corel\CorelDRAW Graphics Suite\27\Programs64\Addons\CDRAIAssistant\`
- 必须文件：CDRAIAssistant.dll、AppUI.xslt、CorelDrw.addon、UserUI.xslt

## 架构说明

### 插件入口
- `zhushou.xaml` + `zhushou.xaml.cs` - WPF UserControl，作为 CDR 插件主界面
- `Services/CDRHelper.cs` - 通过 CoGetActiveObject 获取 CDR COM 对象

### AppUI.xslt 配置
- CDR 使用 XSLT 转换来注册插件
- `type="wpfhost"` 表示托管 WPF 控件
- `hostedType` 格式：`DLL路径,命名空间.类名`
- CDR AI 助手的 hostedType：`Addons\CDRAIAssistant\CDRAIAssistant.dll,CDR_AI_Assistant.Zhushou`
- 工具栏使用独立的 GUID（避免与其他插件冲突）

### CDR COM API
- 使用 `Corel.Interop.VGCore` 命名空间
- 通过 `CoGetActiveObject("CorelDRAW.Application")` 获取 CDR 实例
- 常用对象：Application、Document、Page、Shape、Selection

## 注意事项

- 不要修改秋裤插件（qiuku）的任何文件
- CDR 运行时会锁定 DLL，修改前需要关闭 CDR
- CDR 可能缓存旧的工作空间配置，重启无效时可按 F8 重置工作空间
- 如果插件显示空白，检查 XSLT 配置和 WPF 控件是否正确加载
- qiuku 插件使用 x86 (Intel i386) 架构，DLL 应编译为 x86 以保持兼容