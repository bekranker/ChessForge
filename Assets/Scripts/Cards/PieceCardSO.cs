using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPieceCard", menuName = "Chess/PieceCard")]
public class PieceCardSO : ScriptableObject
{
    public string Name;
    //public ChessPiece Piece;
    public Sprite Icon;
    public ChessPiece WhitePiecePrefab, BlackPiecePrefab;
}