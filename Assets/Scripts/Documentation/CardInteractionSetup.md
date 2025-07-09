# Card Interaction System Setup Guide

## ✅ **System Overview**

The card interaction system allows players to:
1. **View cards** in their hand slots
2. **Select cards** by clicking on them
3. **Place cards** on the board by clicking tiles
4. **Use keyboard shortcuts** for faster gameplay

## 🔧 **Setup Requirements**

### 1. **Card Visual Prefab Setup**
Create a card prefab with these components:
- `GameObject` with `RectTransform`
- `Image` component (card background)
- `CardVisual` script
- `CardPrefabSetup` script (optional, for auto-setup)

### 2. **Card Prefab Structure**
```
CardPrefab (Image + CardVisual + CardPrefabSetup)
├── PieceIcon (Image) - Shows piece sprite
├── PieceText (TextMeshProUGUI) - Shows piece name
└── SelectionHighlight (Image) - Yellow highlight when selected
```

### 3. **CardSystem Configuration**
In the `CardSystem` component, assign:
- `cardVisualPrefab` - Your UI card prefab
- `slotPrefab` - UI slot prefab for card slots
- `playerSlotParents[0]` - UI container for Player 1's card slots
- `pieceSprites[]` - Array of piece sprites for visual representation

### 4. **Input Manager**
Add `GameInputManager` to a GameObject in the scene for keyboard shortcuts.

## 🎮 **How It Works**

### **Card Drawing & Visual Spawning**
1. Cards are drawn from each player's deck
2. Visual cards spawn and animate to hand slots
3. Only human player's cards are visible

### **Card Selection**
1. Click on a card in your hand to select it
2. Selected card gets yellow highlight
3. Use `R` key to select random card
4. Use `1-9` keys to select specific card by position

### **Card Placement**
1. Select a card first
2. Click on a valid board tile to place it
3. The card visual disappears and a piece is created on the board
4. Only valid deployment zones allow placement

### **Visual States**
- **Normal**: White background
- **Selected**: Yellow background with highlight
- **Hover**: Cyan background
- **Disabled**: Gray background

## 🎯 **Usage Instructions**

### **For Players:**
1. **Start Game**: Cards automatically appear in your hand
2. **Select Card**: Click on any card in your hand OR press `R` for random
3. **Place Card**: Click on a valid board tile (in your deployment zone)
4. **Keyboard Shortcuts**:
   - `R` - Random card selection
   - `D` - Draw new card from deck
   - `H` - Show hand contents
   - `1-9` - Select specific card
   - `B` - Auto-complete betting phase

### **For Developers:**
1. Assign the card visual prefab to `CardSystem.cardVisualPrefab`
2. Create UI slot prefabs and assign to `CardSystem.slotPrefab`
3. Set up slot parent containers in `CardSystem.playerSlotParents`
4. Add piece sprites to `CardSystem.pieceSprites` array
5. Add `GameInputManager` to scene for keyboard controls

## 🔍 **Debugging**

### **Common Issues:**
- **Cards not appearing**: Check if `cardVisualPrefab` is assigned and slots are created
- **Cards not selectable**: Ensure cards have `CardVisual` component and proper UI setup
- **Placement not working**: Verify `BoardTile` components on board tiles
- **No keyboard input**: Add `GameInputManager` to scene

### **Debug Console Messages:**
- `🎴 Selected [PieceType] card!` - Card successfully selected
- `✅ Spawned card UI prefab` - Card visual created successfully
- `❌ No slot available` - Not enough slots for cards
- `🎯 Selected card [N]` - Keyboard selection worked

## 📋 **Component Dependencies**

- `CardSystem` - Main card management
- `CardVisual` - Individual card interaction
- `BoardTile` - Board tile clicking for placement
- `GameManager` - Game state management
- `GameInputManager` - Keyboard input handling
- `CardPrefabSetup` - Auto-prefab configuration

## 🎨 **Customization**

### **Visual Appearance:**
Edit colors in `CardVisual`:
- `normalColor` - Default card color
- `selectedColor` - Selected card color
- `hoverColor` - Mouse hover color
- `disabledColor` - Non-interactable color

### **Keyboard Controls:**
Edit keys in `GameInputManager`:
- `selectRandomCardKey` - Random selection key
- `drawCardKey` - Draw card key
- `completeBettingKey` - Auto-bet key
- `showHandKey` - Show hand key

The system is now ready for players to interact with cards from their deck and place them on the board! 🎯