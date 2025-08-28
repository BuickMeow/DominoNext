# ���پ����༭���ع�˵��

## �ع�Ŀ��

ԭʼ��`PianoRollViewModel`��`EditorCommandsViewModel`��`NoteEditingLayer`�ļ�������������MVVM��Ƶ����ʵ�����ع��󰴹���ģ������˲�֣�����˴���Ŀ�ά���ԺͿɲ����ԡ�

## �µ��ļ��нṹ

```
DominoNext/
������ ViewModels/
��   ������ Editor/
��       ������ Base/
��       ��   ������ PianoRollViewModel.cs                 # ԭʼ�ļ���������
��       ��   ������ PianoRollViewModelRefactored.cs       # �ع������ViewModel
��       ������ Commands/
��       ��   ������ EditorCommandsViewModel.cs            # ԭʼ�ļ���������
��       ��   ������ EditorCommandsViewModelRefactored.cs  # �ع��������ViewModel
��       ��   ������ Handlers/                             # ���ߴ�����
��       ��       ������ PencilToolHandler.cs              # Ǧ�ʹ��ߴ�����
��       ��       ������ SelectToolHandler.cs              # ѡ�񹤾ߴ�����
��       ��       ������ EraserToolHandler.cs              # ��Ƥ���ߴ�����
��       ��       ������ KeyboardCommandHandler.cs        # �����������
��       ������ Modules/                                  # ����ģ��
��       ��   ������ NoteDragModule.cs                     # ������קģ��
��       ��   ������ NoteResizeModule.cs                   # ����������Сģ��
��       ��   ������ NoteCreationModule.cs                 # ��������ģ��
��       ��   ������ NoteSelectionModule.cs                # ����ѡ��ģ��
��       ��   ������ NotePreviewModule.cs                  # ����Ԥ��ģ��
��       ������ State/                                    # ״̬����
��       ��   ������ DragState.cs                          # ��ק״̬
��       ��   ������ ResizeState.cs                        # ������С״̬
��       ��   ������ SelectionState.cs                     # ѡ��״̬
��       ������ Enums/
��       ��   ������ EditorTool.cs                         # �༭������ö��
��       ������ Models/
��           ������ NoteDurationOption.cs                 # ����ʱֵѡ��ģ��
������ Views/
��   ������ Controls/
��       ������ Editing/
��           ������ NoteEditingLayer.cs                   # ԭʼ�ļ���������
��           ������ NoteEditingLayerRefactored.cs         # �ع���ı༭��
��           ������ Rendering/                            # ��Ⱦ��
��           ��   ������ NoteRenderer.cs                   # ������Ⱦ��
��           ��   ������ DragPreviewRenderer.cs            # ��קԤ����Ⱦ��
��           ��   ������ ResizePreviewRenderer.cs          # ������СԤ����Ⱦ��
��           ��   ������ CreatingNoteRenderer.cs           # ����������Ⱦ��
��           ��   ������ SelectionBoxRenderer.cs           # ѡ�����Ⱦ��
��           ������ Input/                                # ���봦��
��               ������ CursorManager.cs                  # ��������
��               ������ InputEventRouter.cs               # �����¼�·����
```

## �ع�����Ҫ�Ľ�

### 1. ְ�����
- **״̬����**��`DragState`��`ResizeState`��`SelectionState` - ����������ֲ���״̬
- **����ģ��**��ÿ��ģ�鸺��һ���ܣ���ק��������С�������ȣ�
- **��Ⱦ����**��ÿ����Ⱦ������һ���͵���Ⱦ����
- **���봦��**����������¼�·�ɷ���

### 2. �����ά����
- ÿ���ļ�ְ��һ����С���У�ͨ��200-500�У�
- ģ���ͨ���¼��ͽӿ������
- ���ڵ�Ԫ����

### 3. �����Ż�
- ��Ⱦ�������Ʊ���60FPS
- ��������Ż�
- �¼�����/ȡ�����Ĺ���

## ���ʹ���ع���Ĵ���

### 1. �滻ԭʼViewModel
```csharp
// ԭ���ķ�ʽ
var pianoRollViewModel = new PianoRollViewModel(coordinateService);

// �µķ�ʽ
var pianoRollViewModel = new PianoRollViewModelRefactored(coordinateService);
```

### 2. �滻�༭��ؼ�
```xml
<!-- ԭ����XAML -->
<editing:NoteEditingLayer ViewModel="{Binding PianoRollViewModel}" />

<!-- �µ�XAML -->
<editing:NoteEditingLayerRefactored ViewModel="{Binding PianoRollViewModel}" />
```

### 3. ����ģ�鹦��
```csharp
// ͨ��ģ����ʹ���
viewModel.DragModule.StartDrag(note, position);
viewModel.ResizeModule.StartResize(position, note, handle);
viewModel.CreationModule.StartCreating(position);

// ����״̬
bool isDragging = viewModel.DragState.IsDragging;
var draggingNotes = viewModel.DragState.DraggingNotes;
```

## ��չָ��

### ����¹���
1. ��`EditorTool`ö��������¹�������
2. �����µĹ��ߴ���������`CutToolHandler`��
3. ��`EditorCommandsViewModelRefactored`��ע�ᴦ����

### ����¹���ģ��
1. ����״̬�ࣨ�̳л�ʵ�ֻ����ӿڣ�
2. ����ģ���ࣨʵ��ҵ���߼���
3. ��`PianoRollViewModelRefactored`��ע��ģ��
4. ����Ҫ��������Ӧ����Ⱦ��

### �������ȾЧ��
1. ��`Rendering`�ļ����д����µ���Ⱦ��
2. ��`NoteEditingLayerRefactored`��`Render`�����е���

## Ǩ�ƽ���

1. **����ʽǨ��**������ԭʼ�ļ����𲽲����µ��ع��汾
2. **��������**������ͬʱ֧�������汾��ͨ�������л�
3. **���Ը���**��ȷ���ع����������Բ���
4. **������֤**����֤�ع�������û�н���

## ���ģʽӦ��

- **ģ��ģʽ**�����ܰ�ģ����֯
- **״̬ģʽ**��״̬�������
- **����ģʽ**������ͨ�������
- **�۲���ģʽ**���¼�������ģ��ͨ��
- **����ģʽ**����ͬ���ߵĴ������
- **��Ⱦ��ģʽ**����Ⱦ�߼�����

## ע������

1. ȷ������ģ�鶼��ȷʵ����`SetPianoRollViewModel`����
2. ע���¼����ĺ�ȡ�����ĵ��������ڹ���
3. �������еĲ������ֽ�������
4. ����ԭ�е�API�����ԣ������Ҫ��