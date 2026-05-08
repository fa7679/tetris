# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

俄罗斯方块桌面游戏，使用 Python tkinter 开发。

## 运行方式

```bash
python tetris.py
```

或双击 `启动游戏.bat`

## 游戏控制

| 按键 | 功能 |
|-----|------|
| ← / A | 左移 |
| → / D | 右移 |
| ↓ / S | 加速下落 |
| ↑ / W | 旋转 |
| 空格 | 硬降（直接落到底部） |
| P | 暂停/继续 |
| R | 重新开始（游戏结束后） |
| Esc | 退出 |

## 架构说明

- **SHAPES**: 7种方块 (I, O, T, S, Z, J, L) 使用 4x4 矩阵定义
- **COLORS / DARK_COLORS**: 方块颜色和暗色边框
- **Tetromino 类**: 方块对象，包含旋转逻辑
- **TetrisGame 类**: 游戏主逻辑，网格碰撞检测，消行计分

## 分数规则

- 1行: 100分 × 等级
- 2行: 300分 × 等级
- 3行: 500分 × 等级
- 4行 (Tetris): 800分 × 等级

每消除10行升一级，速度提升10%。

## 游戏结束

新方块无法放置时触发 game_over 显示最终分数。