using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPieceCard", menuName = "Chess/PieceCard")]
public class PieceCardSO : ScriptableObject
{
    public string Name;
    public Sprite IconWhite, IconBlack;
    public ChessPiece PiecePrefab;
}