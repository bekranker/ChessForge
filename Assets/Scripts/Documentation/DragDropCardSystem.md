# Drag & Drop Card System Guide (World Space Canvas)

## ✅ **System Overview**

The drag & drop card system is optimized for world space canvas and allows players to:
1. **Drag cards** directly from their hand slots using IPointer interfaces
2. **Drop cards** onto valid board tiles to place pieces
3. **Visual feedback** during drag operations with world space positioning
4. **Automatic card destruction** when successfully placed

## 🎯 **How It Works**

### **Drag Mechanics (World Space)**
- **Start Drag**: Click and hold on any card in your hand
- **During Drag**: Card follows mouse with proper world space positioning
- **Visual Feedback**: Board tiles show green (valid) or red (invalid) when hovering
- **End Drag**: Release mouse button to attempt placement

### **Drop Mechanics**
- **Valid Drop**: Card creates piece on board and gets destroyed
- **Invalid Drop**: Card returns to original position in hand
- **Turn Management**: Successfully placing a card ends your turn

## 🔧 **Technical Implementation (World Space Canvas)**

### **CardVisual Component**
- Implements `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`
- **World Space Positioning**: Properly converts screen coordinates to world space
- **Drag Offset Calculation**: Maintains proper offset during drag for smooth movement
- **Multi-layered Raycasting**: Tries UI raycast, 3D raycast, and 2D raycast for maximum compatibility
- **Automatic canvas detection**: Finds world space canvas and configures accordingly

### **BoardTile Component**  
- Implements `IDropHandler`, `IPointerEnterHandler`, `IPointerExitHandler`
- **Dual Collider System**: Has both 2D and 3D colliders for comprehensive raycast detection
- **World Space Compatibility**: Works with both UI events and physics raycasts
- **Visual Feedback Optimization**: Smart hover state management to prevent flickering

### **CardSystem Integration**
- New method: `TryPlaceCardAtPosition(Card card, Vector2Int position)`
- Handles placement validation and piece creation
- Manages card removal from hand
- Updates game state and ends turn

## 🎮 **Player Experience**

### **Visual States**
- **Normal**: White card background
- **Hover**: Cyan card background  
- **Dragging**: Semi-transparent white background with scale increase
- **Disabled**: Gray background (non-interactable)

### **Board Feedback**
- **Green Tiles**: Valid drop zones during drag
- **Red Tiles**: Invalid drop zones during drag  
- **Smart Hovering**: Only one tile highlighted at a time

### **Interaction Flow**
1. **Draw Phase**: Cards animate from deck to hand slots
2. **Selection**: Drag any card from your hand with world space movement
3. **Placement**: Drop on valid tile to create piece
4. **Cleanup**: Card disappears, piece appears, turn ends

## 🛠️ **Setup Requirements (World Space Canvas)**

### **Canvas Setup**
- **Render Mode**: World Space (required)
- **Event Camera**: Assigned to main camera
- **Canvas Scaler**: World space sizing
- **GraphicRaycaster**: Present for UI event detection

### **Card Prefab Setup**  
```
CardPrefab (RectTransform + Image + CardVisual)
├── PieceIcon (Image) - Shows piece sprite
├── PieceText (TextMeshProUGUI) - Shows piece name
└── SelectionHighlight (Image) - Visual selection indicator
```

### **Board Tile Setup**
- `BoardTile` component with drag/drop interfaces
- `SpriteRenderer` for visual feedback
- `BoxCollider2D` for 2D physics raycast
- `BoxCollider` for 3D physics raycast (world space compatibility)

### **World Space Requirements**
- Main camera reference for screen-to-world conversion
- Proper canvas plane distance configuration
- LayerMask setup for board tile detection

## 🎨 **Customization Options**

### **Drag Behavior (World Space)**
- `dragScale`: How much to scale card during drag (default: 1.2x)
- `draggingColor`: Color tint during drag (default: semi-transparent)
- `boardLayerMask`: Layer mask for board tile detection
- World space positioning offset for smooth dragging

### **Visual Feedback**
- `validDropColor`: Tile color for valid drops (default: green)
- `invalidDropColor`: Tile color for invalid drops (default: red)
- `normalColor`: Default tile color
- Smart hover state management prevents multiple tiles highlighting

### **World Space Settings**
- Canvas plane distance configuration
- Camera reference assignment
- Multi-raycast system for comprehensive detection

## 🔍 **Debugging (World Space)**

### **Common Issues**
- **Cards not draggable**: Check if `CardVisual` has drag interfaces and world camera reference
- **Wrong positioning**: Verify canvas render mode is World Space and plane distance is set
- **No drop detection**: Ensure `BoardTile` has both 2D and 3D colliders
- **Cards return to hand**: Validate `CanPlacePieceAt` logic and raycast layer masks

### **Debug Messages**
- `🎯 Started dragging [PieceType] card` - Drag began
- `🎯 Drag offset calculated: (x, y, z)` - World space offset computed
- `🎯 Dropped card on [UI/world/2D] board tile at (x, y)` - Drop detected with method
- `✅ Successfully placed [PieceType] at (x, y)` - Placement succeeded
- `❌ Cannot place [PieceType] at (x, y)` - Placement failed
- `↩️ Returned [PieceType] card to original position` - Invalid drop

## 📋 **Component Dependencies**

- `CardVisual` - Individual card interaction and world space drag logic
- `CardSystem` - Card management and placement validation
- `BoardTile` - Drop zone detection with dual collider system
- `BoardManager` - Placement validation and piece creation
- `GameManager` - Phase management and turn handling
- `Camera` - World space coordinate conversion
- `EventSystem` - Unity UI event processing
- `Canvas` - World space rendering and event detection

## 🚀 **Performance Notes (World Space)**

- Cards use `CanvasGroup` to disable raycasting during drag
- Visual feedback uses sprite color changes (no new objects)
- Drag preview reuses existing card object (no duplication)
- Multi-raycast system with early termination for efficiency
- Smart hover state prevents unnecessary visual updates
- Automatic cleanup prevents memory leaks

## ⚙️ **World Space Canvas Specific Features**

- **Screen-to-World Conversion**: Proper mouse position translation
- **Drag Offset Management**: Maintains natural drag feel
- **Multi-Raycast System**: UI → 3D Physics → 2D Physics fallback
- **Canvas Compatibility**: Auto-detects and configures for world space
- **Camera Integration**: Seamless integration with main camera

The drag & drop system provides an intuitive and responsive way for players to place cards on the board while maintaining full compatibility with world space canvas setups! 🎯