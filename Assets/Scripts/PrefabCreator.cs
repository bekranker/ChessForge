using UnityEngine;

[System.Serializable]
public class PrefabCreator : MonoBehaviour
{
    [Header("Create Piece Prefabs")]
    [Tooltip("Click this button to create all chess piece prefabs")]
    public bool createPrefabs = false;
    
    void OnValidate()
    {
        if (createPrefabs)
        {
            createPrefabs = false;
            CreateAllPiecePrefabs();
        }
    }
    
    void CreateAllPiecePrefabs()
    {
        CreatePiecePrefab("Pawn", typeof(Pawn));
        CreatePiecePrefab("Rook", typeof(Rook));
        CreatePiecePrefab("Knight", typeof(Knight));
        CreatePiecePrefab("Bishop", typeof(Bishop));
        CreatePiecePrefab("Queen", typeof(Queen));
        CreatePiecePrefab("King", typeof(King));
        
        Debug.Log("All chess piece prefabs created successfully!");
    }
    
    void CreatePiecePrefab(string pieceName, System.Type pieceType)
    {
        GameObject pieceObject = new GameObject(pieceName);
        
        SpriteRenderer spriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 1;
        
        pieceObject.AddComponent(pieceType);
        
        BoxCollider2D collider = pieceObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        
#if UNITY_EDITOR
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(pieceObject, $"Assets/Prefabs/{pieceName}.prefab");
        DestroyImmediate(pieceObject);
#endif
    }
}