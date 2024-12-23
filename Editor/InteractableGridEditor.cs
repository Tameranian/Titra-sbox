// using Editor;
// using System;
// using System.Collections.Generic;

// [EditorApp("Interactable Grid", "settings_suggest", "Configure interactable grid positions")]
// public class InteractableGridApp : BaseWindow
// {
//     private InteractableGridEditorWidget editor;

// 	Window View;
//     public InteractableGridApp()
//     {
//         WindowTitle = "Interactable Grid Editor";
//         SetWindowIcon("emoji_emotions");
        
//         Size = new Vector2(640, 640);
// 		View = new Window( this );

//         Layout = Layout.Column();
// 		Layout.Add( View, 1 );
        
// 		Rebuild();
// 		Show();
//     }

//     [EditorEvent.Hotload]
// 	public void Rebuild()
// 	{
//         editor = Layout.Add(new InteractableGridEditorWidget(this));
// 	}
// }

// // The control widget that appears in property grids
// [CustomEditor(typeof(GridPosition))]
// public class GridPositionControlWidget : ControlWidget
// {
//     public GridPositionControlWidget(SerializedProperty property) : base(property)
//     {
//         SetSizeMode(SizeMode.Default, SizeMode.Default);
//         Layout = Layout.Column();
//         Layout.Spacing = 2;
//         Cursor = CursorShape.Finger;
//     }

//     protected override void PaintOver()
//     {
//         GridPosition position = SerializedProperty.GetValue<GridPosition>();
//         Vector2 iconSize = Theme.RowHeight - 4;
//         var iconRect = Rect.FromPoints(2, iconSize + 2);
        
//         Paint.SetBrush(Theme.Primary.WithAlpha(0.1f));
//         Paint.DrawRect(iconRect, 2);
//         Paint.SetPen(Theme.Primary);
//         Paint.DrawIcon(iconRect, "settings_suggest", 12);
//         Paint.SetPen(Theme.ControlText);
//         Paint.DrawText(LocalRect.Shrink(iconRect.Right + 4, 0, 4, 0), 
//             $"({position.X}, {position.Y}, {position.Z}) - {position.InteractableType}", 
//             TextFlag.LeftCenter);
//     }

//     protected override void OnMouseReleased(MouseEvent e)
//     {
//         base.OnMouseReleased(e);
//         if (e.LeftMouseButton)
//         {
//             GridPosition position = SerializedProperty.GetValue<GridPosition>();
//             InteractableGridEditorWidget.OpenPopup(this, position, x => { 
//                 SerializedProperty.SetValue(x);
//                 Update();
//             });
//         }
//     }
// }

// // The editor widget that shows the grid and handles interaction
// public partial class InteractableGridEditorWidget : Widget
// {
//     public Action<GridPosition> ValueChanged { get; set; }

//     Layout HeaderLayout;
//     GridLayout GridLayout;
//     Label DepthLabel;
//     FloatSlider GridSizeSlider;
//     GridPosition _value;
//     int gridSize = 3;

//     public GridPosition Value
//     {
//         get => _value;
//         set
//         {
//             _value = value;
//             Rebuild();
//             ValueChanged?.Invoke(_value);
//         }
//     }

//     public InteractableGridEditorWidget(Widget parent = null) : base(parent)
//     {
//         _value = new GridPosition();
        
//         Layout = Layout.Column();
//         Layout.Spacing = 8;
//         Layout.Margin = 16;

//         // Header with depth controls
//         HeaderLayout = Layout.Row();
//         HeaderLayout.Margin = 8;
//         HeaderLayout.Spacing = 2;

//         var buttonUp = HeaderLayout.Add(new IconButton("arrow_upward", () => ChangeDepth(1), this));
//         DepthLabel = HeaderLayout.Add(new Label(this));
//         DepthLabel.Text = $"Depth: {_value.Z}";
//         var buttonDown = HeaderLayout.Add(new IconButton("arrow_downward", () => ChangeDepth(-1), this));

//         Layout.Add(HeaderLayout);

//         // Grid size slider
//         GridSizeSlider = Layout.Add(new FloatSlider(this)
//         {
//             Minimum = 1,
//             Maximum = 5,
//             Value = 1,
//             OnValueEdited = () => ChangeGridSize((int)GridSizeSlider.Value)
//         });

//         // Grid layout
//         GridLayout = Layout.Grid();
//         GridLayout.Spacing = 2;
//         Layout.Add(GridLayout);

//         Rebuild();
//     }

//     private void Rebuild()
//     {
//         GridLayout.Clear(true);

//         // Create grid cells
//         for (int y = 0; y < gridSize; y++)
//         {
//             for (int x = 0; x < gridSize; x++)
//             {
//                 var isCenter = x == gridSize / 2 && y == gridSize / 2;
//                 var btn = CreateGridButton(x, y, isCenter);
//                 GridLayout.AddCell(x, y, btn);
//             }
//         }

//         DepthLabel.Text = $"Depth: {_value.Z}";
//     }

//     private Button CreateGridButton(int x, int y, bool isCenter)
//     {
//         var btn = new Button($"({x},{y},{_value.Z})", this)
//         {
//             MinimumSize = new Vector2(80, 80)
//         };
        
//         if (isCenter)
//         {
//             btn.Tint = Theme.Primary;
//             btn.Enabled = _value.Z != 0; // Allow center button to be used except on depth 0
//         }
//         else
//         {
//             btn.Clicked = () => ShowConfigDialog(x, y);
            
//             if (_value.X == x && _value.Y == y && !string.IsNullOrEmpty(_value.InteractableType))
//             {
//                 btn.Tint = Theme.Green;
//             }
//         }

//         return btn;
//     }

//     private void ChangeDepth(int delta)
//     {
//         _value.Z += delta;
//         Rebuild();
//         ValueChanged?.Invoke(_value);
//     }

//     private void ChangeGridSize(int newSize)
//     {
//         gridSize = (newSize * 2) + 1; // Ensure grid size is always odd
//         Rebuild();
//     }

//     private void ShowConfigDialog(int x, int y)
//     {
//         var dialog = new Dialog();
//         dialog.WindowTitle = $"Configure Grid Position ({x},{y},{_value.Z})";
//         dialog.Size = new Vector2(300, 250);

//         var layout = dialog.Layout = Layout.Column();
//         layout.Margin = 16;
//         layout.Spacing = 8;

//         var position = new GridPosition
//         {
//             X = x,
//             Y = y,
//             Z = _value.Z,
//             InteractableType = _value.InteractableType,
//             FunctionName = _value.FunctionName
//         };

//         var controlSheet = new ControlSheet();
//         var serializedPosition = position.GetSerialized();
//         controlSheet.AddObject(serializedPosition);
//         layout.Add(controlSheet);

//         // Buttons
//         var buttons = layout.Add(new Widget());
//         buttons.Layout = Layout.Row();
//         buttons.Layout.Spacing = 8;

//         var confirmButton = new Button.Primary("Confirm", "check");
//         confirmButton.Clicked = () =>
//         {
//             Value = position;
//             dialog.Close();
//         };
//         buttons.Layout.Add(confirmButton);

//         var cancelButton = new Button("Cancel", "close");
//         cancelButton.Clicked = () => dialog.Close();
//         buttons.Layout.Add(cancelButton);

//         dialog.Show();
//     }

//     public static void OpenPopup(Widget parent, GridPosition input, Action<GridPosition> onChange)
//     {
//         var popup = new PopupWidget(parent);
//         popup.Visible = false;
//         popup.FixedWidth = 400;
//         popup.Layout = Layout.Column();
//         popup.Layout.Margin = 8;

//         var editor = popup.Layout.Add(new InteractableGridEditorWidget(popup), 1);
//         editor.Value = input;
//         editor.ValueChanged = onChange;

//         popup.OpenAtCursor();
//     }

//     [EditorEvent.Hotload]
//     public void OnHotload()
//     {
//         Rebuild();
//     }
// }

// // The data structure that holds the grid position configuration
// public class GridPosition
// {
//     public int X { get; set; }
//     public int Y { get; set; }
//     public int Z { get; set; }
//     public string InteractableType { get; set; } = "";
//     public string FunctionName { get; set; } = "";
// }