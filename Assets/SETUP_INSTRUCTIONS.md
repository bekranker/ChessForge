# ChessForge AI and Turn-Based Setup Instructions

## Files Created/Modified

### New Files:
1. `/Assets/Scripts/AI/AIManager.cs` - AI system for computer opponent
2. `/Assets/Scripts/UI/GameUIManager.cs` - UI management system
3. `/Assets/Scripts/UI/WinLoseUI.cs` - Win/lose display system
4. `/Assets/SETUP_INSTRUCTIONS.md` - This file

### Modified Files:
1. `/Assets/Scripts/ChessGameManager.cs` - Enhanced with turn-based system and AI integration
2. `/Assets/Scripts/Pieces/ChessPiece.cs` - Added attack functionality and game manager notifications
3. `/Assets/Scripts/Pieces/PieceCard.cs` - Added game manager notification when cards are placed
4. `/Assets/Scripts/Bet/BetPiece.cs` - Enhanced betting validation and feedback
5. `/Assets/Scripts/ChessBoard.cs` - Improved placement tiles for both players

## Scene Setup Instructions

### 1. Add AI Manager
1. Create an empty GameObject named "AIManager"
2. Add the `AIManager` component to it
3. Place it under the "MANAGERS" GameObject in your scene
4. In the AIManager inspector:
   - Set AI Thinking Time (default: 2 seconds)
   - Set Search Depth (default: 3)
   - Set AI Color to `Black`
   - Assign AI Deck Data (PieceCardSO assets) in the inspector

### 2. Update Game Manager
1. Select your existing "GameManager" GameObject
2. In the ChessGameManager inspector:
   - Assign the AIManager reference
   - Set Bet Time (default: 30 seconds)
   - Set Play Time (default: 60 seconds)  
   - Set Setup Turns Per Player (default: 3)
   - Check "Is Player Vs AI" checkbox
   - Set Player Color to `White`

### 3. Create Win/Lose UI
1. Create a new Canvas if you don't have one for UI
2. Create UI elements:
   - Create a Panel named "WinLabel" with a Text component saying "YOU WIN!"
   - Create a Panel named "LoseLabel" with a Text component saying "YOU LOSE!"
   - Set both panels to inactive initially
3. Assign these panels to the ChessGameManager's Win Label and Lose Label fields

### 4. Create Game Info UI (Optional)
1. Create UI Text elements for:
   - Phase Display (shows current game phase)
   - Player Turn Display (shows whose turn it is)
   - Timer Display (shows remaining time)
2. Create a GameObject named "GameUIManager"
3. Add the `GameUIManager` component
4. Assign the UI text elements to the appropriate fields

### 5. Setup Piece Card Data for AI
1. Create PieceCardSO assets for all chess pieces:
   - Right-click in Project → Create → Chess → PieceCard
   - Create one for each piece type (Pawn, Rook, Knight, Bishop, Queen, King)
   - Assign appropriate sprites and prefabs
2. Add these PieceCardSO assets to the AIManager's AI Deck Data list

## Game Flow

### Setup Phase (Turn-based)
- Each player takes turns placing 1 card at a time
- Total turns = setupTurnsPerPlayer × 2 (default: 6 total turns)
- White player places in bottom rows, Black (AI) in top rows
- Players can only place cards in their designated area

### Betting Phase (Timed)
- Both players can place bets on their own pieces
- Uses existing betting chip system
- Phase ends when timer expires (default: 30 seconds)

### Playing Phase (Turn-based)
- Traditional chess gameplay
- Players take turns moving pieces
- Pieces can move and attack according to chess rules
- Game continues until checkmate or king capture

### End Phase
- Win/Lose labels are displayed
- Game stops and shows restart option

## Controls

### Player Controls:
- **Setup Phase**: Drag and drop cards from hand to valid tiles
- **Betting Phase**: Drag betting chips onto your pieces
- **Playing Phase**: Click piece to select, click destination to move/attack

### AI Behavior:
- **Setup Phase**: Places pieces strategically, preferring central positions
- **Betting Phase**: Places random bets on its pieces
- **Playing Phase**: Uses basic evaluation function to choose moves (captures pieces, controls center, avoids check)

## Debugging

Enable debug logs to see:
- Turn progression: "Processing turn: Setup - White"
- AI decisions: "AI placed Pawn at (3,6)"
- Move validation: "Pawn White attacks Rook Black at (4,4)"
- Phase transitions: "Advancing to Betting Phase"

## Customization Options

### AI Difficulty:
- Increase `searchDepth` for better AI (warning: slower)
- Modify `aiThinkingTime` for faster/slower AI moves
- Edit evaluation function in `AIManager.EvaluateMove()` for smarter AI

### Game Timing:
- Adjust `_betTime` and `_playTime` in ChessGameManager
- Modify `setupTurnsPerPlayer` to change setup length

### Piece Placement:
- Modify `GetAllowedPlacementRows()` in ChessBoard for different placement rules
- Edit placement validation in `GetAvailablePlacementTiles()`

## Known Limitations

1. AI uses basic evaluation function (can be enhanced with minimax/alpha-beta)
2. Betting phase relies on timer only (could add "ready" buttons)
3. No en passant or castling special moves implemented
4. No draw conditions (stalemate, insufficient material, etc.)

## Next Steps

1. Test the complete game flow
2. Balance AI difficulty and timing
3. Add more sophisticated AI evaluation
4. Implement additional chess rules if needed
5. Polish UI and visual feedback