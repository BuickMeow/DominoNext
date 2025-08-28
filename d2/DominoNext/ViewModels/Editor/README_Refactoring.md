# 钢琴卷帘编辑器重构说明

## 重构目标

原始的`PianoRollViewModel`、`EditorCommandsViewModel`和`NoteEditingLayer`文件过长，不符合MVVM设计的最佳实践。重构后按功能模块进行了拆分，提高了代码的可维护性和可测试性。

## 新的文件夹结构

```
DominoNext/
├── ViewModels/
│   └── Editor/
│       ├── Base/
│       │   ├── PianoRollViewModel.cs                 # 原始文件（保留）
│       │   └── PianoRollViewModelRefactored.cs       # 重构后的主ViewModel
│       ├── Commands/
│       │   ├── EditorCommandsViewModel.cs            # 原始文件（保留）
│       │   ├── EditorCommandsViewModelRefactored.cs  # 重构后的命令ViewModel
│       │   └── Handlers/                             # 工具处理器
│       │       ├── PencilToolHandler.cs              # 铅笔工具处理器
│       │       ├── SelectToolHandler.cs              # 选择工具处理器
│       │       ├── EraserToolHandler.cs              # 橡皮工具处理器
│       │       └── KeyboardCommandHandler.cs        # 键盘命令处理器
│       ├── Modules/                                  # 功能模块
│       │   ├── NoteDragModule.cs                     # 音符拖拽模块
│       │   ├── NoteResizeModule.cs                   # 音符调整大小模块
│       │   ├── NoteCreationModule.cs                 # 音符创建模块
│       │   ├── NoteSelectionModule.cs                # 音符选择模块
│       │   └── NotePreviewModule.cs                  # 音符预览模块
│       ├── State/                                    # 状态管理
│       │   ├── DragState.cs                          # 拖拽状态
│       │   ├── ResizeState.cs                        # 调整大小状态
│       │   └── SelectionState.cs                     # 选择状态
│       ├── Enums/
│       │   └── EditorTool.cs                         # 编辑器工具枚举
│       └── Models/
│           └── NoteDurationOption.cs                 # 音符时值选项模型
├── Views/
│   └── Controls/
│       └── Editing/
│           ├── NoteEditingLayer.cs                   # 原始文件（保留）
│           ├── NoteEditingLayerRefactored.cs         # 重构后的编辑层
│           ├── Rendering/                            # 渲染器
│           │   ├── NoteRenderer.cs                   # 音符渲染器
│           │   ├── DragPreviewRenderer.cs            # 拖拽预览渲染器
│           │   ├── ResizePreviewRenderer.cs          # 调整大小预览渲染器
│           │   ├── CreatingNoteRenderer.cs           # 创建音符渲染器
│           │   └── SelectionBoxRenderer.cs           # 选择框渲染器
│           └── Input/                                # 输入处理
│               ├── CursorManager.cs                  # 光标管理器
│               └── InputEventRouter.cs               # 输入事件路由器
```

## 重构的主要改进

### 1. 职责分离
- **状态管理**：`DragState`、`ResizeState`、`SelectionState` - 独立管理各种操作状态
- **功能模块**：每个模块负责单一功能（拖拽、调整大小、创建等）
- **渲染分离**：每个渲染器负责单一类型的渲染任务
- **输入处理**：光标管理和事件路由分离

### 2. 代码可维护性
- 每个文件职责单一，大小适中（通常200-500行）
- 模块间通过事件和接口松耦合
- 易于单元测试

### 3. 性能优化
- 渲染节流机制保持60FPS
- 缓存管理优化
- 事件订阅/取消订阅管理

## 如何使用重构后的代码

### 1. 替换原始ViewModel
```csharp
// 原来的方式
var pianoRollViewModel = new PianoRollViewModel(coordinateService);

// 新的方式
var pianoRollViewModel = new PianoRollViewModelRefactored(coordinateService);
```

### 2. 替换编辑层控件
```xml
<!-- 原来的XAML -->
<editing:NoteEditingLayer ViewModel="{Binding PianoRollViewModel}" />

<!-- 新的XAML -->
<editing:NoteEditingLayerRefactored ViewModel="{Binding PianoRollViewModel}" />
```

### 3. 访问模块功能
```csharp
// 通过模块访问功能
viewModel.DragModule.StartDrag(note, position);
viewModel.ResizeModule.StartResize(position, note, handle);
viewModel.CreationModule.StartCreating(position);

// 访问状态
bool isDragging = viewModel.DragState.IsDragging;
var draggingNotes = viewModel.DragState.DraggingNotes;
```

## 扩展指南

### 添加新工具
1. 在`EditorTool`枚举中添加新工具类型
2. 创建新的工具处理器（如`CutToolHandler`）
3. 在`EditorCommandsViewModelRefactored`中注册处理器

### 添加新功能模块
1. 创建状态类（继承或实现基础接口）
2. 创建模块类（实现业务逻辑）
3. 在`PianoRollViewModelRefactored`中注册模块
4. 如需要，创建对应的渲染器

### 添加新渲染效果
1. 在`Rendering`文件夹中创建新的渲染器
2. 在`NoteEditingLayerRefactored`的`Render`方法中调用

## 迁移建议

1. **渐进式迁移**：保留原始文件，逐步测试新的重构版本
2. **并行运行**：可以同时支持两个版本，通过配置切换
3. **测试覆盖**：确保重构后功能完整性不变
4. **性能验证**：验证重构后性能没有降低

## 设计模式应用

- **模块模式**：功能按模块组织
- **状态模式**：状态管理独立
- **命令模式**：操作通过命令处理
- **观察者模式**：事件驱动的模块通信
- **策略模式**：不同工具的处理策略
- **渲染器模式**：渲染逻辑分离

## 注意事项

1. 确保所有模块都正确实现了`SetPianoRollViewModel`方法
2. 注意事件订阅和取消订阅的生命周期管理
3. 性能敏感的操作保持节流机制
4. 保持原有的API兼容性（如果需要）