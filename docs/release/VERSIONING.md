# 版本策略建议（Room Intelligence Pro）

采用语义化版本（SemVer）：`MAJOR.MINOR.PATCH`

- `MAJOR`：破坏性变更（接口、数据结构、核心工作流）
- `MINOR`：新增功能（向后兼容）
- `PATCH`：修复与性能优化

## 建议里程碑

- `0.1.x`：内部 Alpha（引擎可用，流程基础可跑）
- `0.2.x`：外部 Beta（流程闭环、报告导出、稳定性增强）
- `1.0.0`：App Store 首发（完整文档 + 安装体验 + 支持渠道）

## 发布节奏
- 双周迭代一个 `MINOR`
- 紧急修复通过 `PATCH`
- 每次发布必须更新 `CHANGELOG.md`
