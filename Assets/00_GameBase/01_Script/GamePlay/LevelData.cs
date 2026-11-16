using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelData : MonoBehaviour
{
    [Title("Dữ liệu Level")]
    [InfoBox("Kéo thả object vào list hoặc bấm nút 'Tự động tìm'")]
    
    // Bạn nhớ thay 'Pieces' bằng tên script thực tế của bạn (VD: PieceOneObject, DragItems...)
    public List<Pieces> lsPieces; 
    public List<ShadowSlot> lsShadow;

    // --- 1. TỰ ĐỘNG TÌM (Tiện ích phụ để đỡ phải kéo tay) ---
    [Button("1. Tự động tìm Goal & Shadow", ButtonSizes.Medium)]
    [GUIColor(1f, 1f, 0.5f)]
    private void AutoCollect()
    {
        lsPieces.Clear();
        lsShadow.Clear();

        // Tìm trong các con của object này
        lsPieces.AddRange(GetComponentsInChildren<Pieces>(true));
        lsShadow.AddRange(GetComponentsInChildren<ShadowSlot>(true));

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        Debug.Log($"Đã tìm thấy {lsPieces.Count} Pieces và {lsShadow.Count} Shadow.");
    }

    // --- 2. ĐỔI TÊN ---
    [Title("Công cụ Setup")]
    [Button("2. Đổi tên (Goal_0, Shadow_0...)", ButtonSizes.Large)]
    [GUIColor(0.4f, 0.8f, 1f)]
    private void RenameObjects()
    {
        // Đổi tên Pieces
        for (int i = 0; i < lsPieces.Count; i++)
        {
            if (lsPieces[i] == null) continue;
            
            #if UNITY_EDITOR
            Undo.RecordObject(lsPieces[i].gameObject, "Rename Piece");
            #endif
            
            lsPieces[i].gameObject.name = "Goal_" + i;
        }

        // Đổi tên Shadow
        for (int i = 0; i < lsShadow.Count; i++)
        {
            if (lsShadow[i] == null) continue;
            
            #if UNITY_EDITOR
            Undo.RecordObject(lsShadow[i].gameObject, "Rename Shadow");
            #endif
            
            lsShadow[i].gameObject.name = "Shadow_" + i;
        }
        Debug.Log("Đã đổi tên xong!");
    }

    // --- 3. GÁN ID ---
    [Button("3. Gán ID tự động (0, 1, 2...)", ButtonSizes.Large)]
    [GUIColor(0.5f, 1f, 0.5f)]
    private void AssignIDs()
    {
        // Gán ID cho Pieces
        for (int i = 0; i < lsPieces.Count; i++)
        {
            if (lsPieces[i] == null) continue;

            #if UNITY_EDITOR
            Undo.RecordObject(lsPieces[i], "Set Piece ID");
            #endif

            lsPieces[i].id = i;
        }

        // Gán ID cho Shadow
        for (int i = 0; i < lsShadow.Count; i++)
        {
            if (lsShadow[i] == null) continue;

            #if UNITY_EDITOR
            Undo.RecordObject(lsShadow[i], "Set Shadow ID");
            #endif

            lsShadow[i].id = i;
        }
        Debug.Log("Đã đồng bộ ID thành công (Piece[i].id = i).");
    }
}