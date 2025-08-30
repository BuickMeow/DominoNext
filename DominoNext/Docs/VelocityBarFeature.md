# 音符力度条功能实现说明

## 功能概述

我们成功实现了一个完整的音符力度条功能，类似于各种音乐制作软件或DAW中的力度条显示。该功能包括：

### 主要特性

1. **力度条显示**：在事件视图区域显示每个音符的力度条，高度对应MIDI力度值(0-127)
2. **多种交互模式**：
   - **选择工具模式**：可拖拽力度手柄来调整选中音符的力度
   - **铅笔工具模式**：可手绘力度曲线，快速修改多个音符的力度
3. **视觉反馈**：不同状态的音符有不同的颜色显示（正常、选中、拖拽、编辑中）
4. **实时预览**：编辑过程中显示力度变化的实时预览

## 架构设计

### 文件结构

```
DominoNext/
├── Views/Controls/Canvas/
│   └── VelocityViewCanvas.cs          # 力度视图画布
├── Renderers/
│   └── VelocityBarRenderer.cs         # 力度条渲染器
├── ViewModels/Editor/Modules/
│   └── VelocityEditingModule.cs       # 力度编辑模块
├── ViewModels/Editor/State/
│   └── VelocityEditingState.cs        # 力度编辑状态管理
└── ViewModels/Editor/Commands/Handlers/
    └── VelocityToolHandler.cs         # 力度工具处理器
```

### 架构原则

1. **MVVM模式**：严格分离视图和业务逻辑
2. **模块化设计**：每个功能独立封装，易于维护和扩展
3. **状态管理**：统一的状态管理机制，确保数据一致性
4. **渲染分离**：独立的渲染器，支持不同的显示效果

## 技术实现

### 1. VelocityViewCanvas
- **继承**：Control基类
- **功能**：力度视图的主画布，处理鼠标交互
- **特点**：
  - 叠加在EventViewCanvas之上
  - 支持透明背景
  - 处理鼠标事件并委托给力度编辑模块

### 2. VelocityBarRenderer
- **功能**：负责绘制各种状态的力度条
- **支持的渲染类型**：
  - Normal：正常状态
  - Selected：选中状态
  - Editing：编辑中状态
  - Dragging：拖拽状态
- **特点**：
  - 基于力度值动态计算透明度
  - 支持力度值文本显示
  - 编辑路径预览

### 3. VelocityEditingModule
- **功能**：核心业务逻辑处理
- **支持的编辑模式**：
  - **选择工具模式**：拖拽调整选中音符力度
  - **铅笔工具模式**：手绘力度曲线
- **特点**：
  - 状态驱动的编辑逻辑
  - 支持多音符同时编辑
  - 原始力度值保存（支持撤销）

### 4. VelocityEditingState
- **功能**：力度编辑状态管理
- **使用**：ObservableObject和ObservableProperty
- **管理的状态**：
  - 编辑标志
  - 编辑路径
  - 当前编辑位置
  - 编辑中的音符列表
  - 原始力度值字典

## 集成方式

### 在PianoRollViewModel中集成
```csharp
public VelocityEditingModule VelocityEditingModule { get; }

// 在构造函数中初始化
VelocityEditingModule = new VelocityEditingModule(_coordinateService);
VelocityEditingModule.SetPianoRollViewModel(this);

// 订阅事件
VelocityEditingModule.OnVelocityUpdated += InvalidateVisual;
```

### 在XAML中集成
```xml
<!-- 使用 Grid 来叠加事件视图和力度视图 -->
<Grid Width="5000">
    <!-- 背景层：事件视图网格和时间线 -->
    <canvas:EventViewCanvas ViewModel="{Binding}" IsHitTestVisible="False"/>
    
    <!-- 力度视图层：可交互的力度条显示 -->
    <canvas:VelocityViewCanvas ViewModel="{Binding}" IsHitTestVisible="True"/>
</Grid>
```

## 使用方法

### 选择工具模式
1. 选择选择工具
2. 选中一个或多个音符
3. 在力度视图中点击并拖拽来调整力度
4. 所有选中的音符会同时调整

### 铅笔工具模式
1. 选择铅笔工具
2. 在力度视图中按下鼠标并拖拽
3. 经过的音符会根据鼠标Y位置设置对应的力度值
4. 支持连续绘制力度曲线

## 扩展点

1. **新的渲染效果**：在VelocityBarRenderer中添加新的渲染类型
2. **新的编辑模式**：在VelocityEditingModule中添加新的工具处理逻辑
3. **撤销/重做**：利用已保存的原始力度值实现撤销功能
4. **批量操作**：基于现有的选择机制实现批量力度调整
5. **数值输入**：添加直接数值输入的力度编辑方式

## 性能优化

1. **视口裁剪**：只渲染可见区域内的力度条
2. **缓存机制**：音符位置和尺寸计算结果缓存
3. **事件优化**：合理的事件订阅和取消订阅
4. **渲染优化**：避免不必要的重绘

这个实现为DominoNext提供了一个专业级的力度编辑功能，支持直观的可视化操作和灵活的编辑模式。