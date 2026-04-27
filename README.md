# Room Intelligence Pro (Revit 2026)

Room Intelligence Pro 是面向建筑设计师的 Revit 商业级插件，核心目标是提升房间数据治理效率：
- 房间信息批量编辑
- 缺失房间检测与生成（Preview + 执行）
- DWG 文本导入与房间名匹配
- 规则引擎审核

---

## 1. Release Readiness Review（面向 Autodesk App Store）

### 1.1 功能完整度
当前代码已经具备核心引擎与基础 UI，但仍处于 **MVP+** 状态：
- ✅ 已具备：Room Editor、缺失房间 Preview/生成、DWG CSV 解析、匹配核心算法、规则引擎核心。
- ⚠️ 待完善：规范检查可视化页面、导出报告正式功能、数据管理完整流程页。

### 1.2 稳定性
- 已包含事务保护、批量保存、中文错误提示、未保存关闭确认。
- 建议发布前补齐自动化测试与真实项目回归测试（中/大型项目样本）。

### 1.3 UX 成熟度
- 已有 Premium Revit Utility 风格（Glassmorphism / Tonal Layering / Bento Cards / Command Dock）。
- 已新增 Workflow Hub（六步流程导航），降低学习成本。
- 建议下一步：冲突审阅页、规则结果页、导出中心页。

### 1.4 安装体验
- 采用 `.addin` + DLL 标准部署方式。
- 建议发布前提供 MSI 安装器，并支持一键卸载与日志收集。

### 1.5 配置默认值
建议默认值（可在后续配置页开放）：
- 缺失房间最小面积：`1.0㎡`
- 匹配坐标变换：平移 `0,0`，旋转 `0°`
- Room 保存批次：`200`

### 1.6 日志
当前项目尚无统一日志组件，建议 App Store 发布前增加：
- 文件日志（按日期滚动）
- 错误码 + 用户操作上下文
- 一键导出诊断包

### 1.7 文档 / 用户帮助
本仓库已补充：
- `docs/user-manual/TOC.md`（用户手册目录）
- `CHANGELOG.md`（更新日志模板）
- `docs/release/APPSTORE_RELEASE_CHECKLIST.md`（发布核对清单）
- `docs/release/VERSIONING.md`（版本策略建议）

---

## 2. 快速启动（开发者）

1. 安装 Autodesk Revit 2026
2. 确认 `RevitAPI.dll`、`RevitAPIUI.dll` 路径
3. 构建 `RoomIntelligencePro.Addin`
4. 将 DLL 与 `.addin` 部署到 Revit Addins 目录

---

## 3. 版本规划建议（摘要）
- `0.1.x`：内部验证版（MVP）
- `0.2.x`：Beta（完整流程页 + 导出）
- `1.0.0`：首个商店正式版

详见 `docs/release/VERSIONING.md`。
