# DominoNext 项目介绍

## 项目概述

DominoNext 是一个基于 .NET 平台和 Avalonia 框架开发的音乐编辑工具，专注于 MIDI 文件处理与钢琴卷帘编辑功能。项目采用现代化架构设计，遵循 MVVM 模式与模块化思想，提供高效、可扩展的音乐创作体验。

项目包含两个核心部分：`MIDIModificationFramework` MIDI 处理框架和 `DominoNext` 主应用程序，前者提供 MIDI 文件的读写与修改能力，后者则实现了完整的钢琴卷帘编辑界面与用户交互逻辑。

## 核心组件

### 1. MIDIModificationFramework

MIDI 处理核心框架，提供 MIDI 1.0 文件的全方位处理能力：

- **基础功能**：MIDI 文件的读取、写入和修改
- **核心类库**：`MidiFile`、`MidiTrack`、`MidiWriter` 等实现 MIDI 数据处理
- **辅助工具**：
  - `FastList` 高效数据结构
  - `XZ` 压缩流工具，支持 MIDI 文件压缩处理
  - `EventParser` MIDI 事件解析器
  - `TrackReader` 轨道数据读取器

### 2. DominoNext 主应用

基于 Avalonia 的跨平台桌面应用，提供直观的音乐编辑界面：

- **核心编辑功能**：
  - 钢琴卷帘编辑器，支持音符创建、拖拽、调整长度等操作
  - 多工具支持：选择工具、铅笔工具、橡皮擦工具等
  - 网格对齐与量化功能
  - 项目元数据管理（标题、艺术家、 tempo 等）

- **架构特点**：
  - 模块化设计：功能按职责划分为独立模块（`NoteDragModule`、`NoteResizeModule` 等）
  - 状态管理：通过状态模式（`DragState`、`ResizeState` 等）管理编辑器状态
  - MVVM 架构：清晰分离视图、视图模型与业务逻辑
  - 高效渲染：优化的渲染系统确保 60FPS 流畅体验，包含多种渲染器（`NoteRenderer`、`CreatingNoteRenderer` 等）

- **个性化设置**：
  - 界面主题与颜色配置
  - 多语言支持（中文、English 等）
  - 快捷键自定义
  - 编辑参数调整（网格大小、缩放比例等）

## 项目结构
DominoNext/
├── DominoNext.sln # 解决方案文件
├── .gitattributes # Git 属性配置
├── .gitignore # Git 忽略文件配置
├── MIDIModificationFramework/ # MIDI 处理框架
│ ├── .gitignore
│ ├── EventParser.cs # MIDI 事件解析器
│ ├── Exceptions/ # 异常定义
│ ├── FastList.cs # 高效列表数据结构
│ ├── Interfaces/ # 接口定义
│ ├── MIDI Events/ # MIDI 事件类型
│ ├── MIDIModificationFramework.csproj
│ ├── MidiFile.cs # MIDI 文件处理类
│ ├── MidiTrack.cs # MIDI 轨道处理类
│ ├── MidiWriter.cs # MIDI 写入工具
│ ├── Note.cs # 音符数据模型
│ ├── ParallelStream.cs # 并行流处理
│ ├── Properties/ # 程序集属性
│ ├── README.md # 框架说明
│ ├── Sequence Functions/ # 序列处理函数
│ ├── TrackReader.cs # 轨道读取器
│ └── XZ.cs # XZ 压缩工具
└── DominoNext/ # 主应用程序
├── App.axaml # 应用入口配置
├── App.axaml.cs # 应用初始化逻辑
├── Assets/ # 资源文件
├── Behaviors/ # 行为定义
├── Converters/ # 数据转换器
├── Docs/ # 文档
│ └── Settings_System_Guide.md # 设置系统指南
├── DominoNext.csproj # 项目文件
├── FodyWeavers.xml # Fody 配置
├── Models/ # 数据模型
│ └── NoteModel.cs # 音符模型
├── Program.cs # 程序入口
├── Renderers/ # 渲染器
│ └── NoteRenderer.cs # 音符渲染器
├── Services/ # 服务
│ ├── Interfaces/
│ │ └── IProjectStorageService.cs # 项目存储服务接口
├── ViewLocator.cs # 视图定位器
├── ViewModels/ # 视图模型
│ └── Editor/ # 编辑器相关
│ ├── Modules/ # 编辑功能模块
│ │ └── NoteCreationModule.cs # 音符创建模块
│ ├── State/ # 状态管理
│ ├── REFACTORING_STATUS.md # 重构状态说明
│ └── REFACTORING_COMPLETE.md # 重构完成说明
└── Views/ # 视图
└── MainWindow.axaml.cs # 主窗口

## 技术亮点

1. **模块化架构**：通过模块分离实现单一职责原则，每个文件专注于一个小型功能（200-500行），模块间通过接口通信

2. **设计模式应用**：
   - 模块模式：功能按模块组织
   - 状态模式：管理编辑器状态
   - 命令模式：处理用户操作命令
   - 观察者模式：事件通知与通信
   - 渲染器模式：处理不同元素的渲染逻辑

3. **性能优化**：
   - 高效渲染系统确保60FPS流畅体验
   - 音符渲染使用样式缓存减少资源消耗
   - 量化算法优化音符编辑精度

4. **可扩展性**：
   - 插件化设计支持新增编辑工具
   - 模块化结构便于功能扩展
   - 完善的接口设计支持自定义实现

## 功能特性

- MIDI 文件导入/导出与编辑
- 钢琴卷帘式音符编辑（创建、拖拽、调整长度等）
- 多工具编辑模式（选择、绘制、擦除等）
- 自定义快捷键与界面设置
- 实时预览与网格对齐功能
- 项目元数据管理（标题、艺术家、节拍等）
- 跨平台支持（基于 Avalonia 框架）

## 开发与扩展

项目采用现代 C# 开发实践，代码结构清晰，便于二次开发与功能扩展。主要扩展方向包括：

- 新增编辑工具（如剪切工具）
- 扩展渲染效果
- 增加音频处理功能
- 支持更多文件格式

适合音乐软件开发者、MIDI 爱好者和需要音乐编辑功能集成的项目参考。
