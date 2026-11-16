#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class MergeUI : Editor
{
    // =================================================
    // CẤU HÌNH LỀ AN TOÀN (PADDING)
    // Tăng số này lên nếu vẫn bị mất viền (đơn vị pixel)
    private const int PADDING = 20; 
    // =================================================

    [MenuItem("GameObject/👉 GỘP CON THÀNH 1 SPRITE (MERGE UI)", false, 0)]
    public static void MergeSelection()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null || selectedObj.GetComponent<RectTransform>() == null)
        {
             Debug.LogWarning("Vui lòng chọn một UI Object có RectTransform.");
             return;
        }

        // 1. CHUẨN BỊ
        string folderPath = "Assets/MergedSprites";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        GameObject clone = Instantiate(selectedObj);
        clone.name = selectedObj.name + "_Temp_Snapshot";
        // Đặt layer riêng để dễ render nếu cần (ở đây dùng cách đưa đi xa)
        
        // Setup Canvas tạm chế độ WorldSpace để dễ kiểm soát camera
        GameObject canvasObj = new GameObject("TempCanvas_WorldSpace");
        Canvas tempCanvas = canvasObj.AddComponent<Canvas>();
        tempCanvas.renderMode = RenderMode.WorldSpace;
        
        // Đưa ra xa tít tắp
        canvasObj.transform.position = new Vector3(50000, 50000, 0);
        
        clone.transform.SetParent(tempCanvas.transform, false);
        RectTransform cloneRect = clone.GetComponent<RectTransform>();

        // 2. TÍNH TOÁN KÍCH THƯỚC BAO QUANH (BOUNDS)
        // Buộc cập nhật layout để lấy kích thước chuẩn nhất
        LayoutRebuilder.ForceRebuildLayoutImmediate(cloneRect);
        Rect bounds = GetScreenRectWorld(cloneRect);
        
        if (bounds.width <= 1 || bounds.height <= 1) 
        {
            Debug.LogError($"Object {selectedObj.name} quá nhỏ hoặc không có hình ảnh hiển thị! (Width: {bounds.width}, Height: {bounds.height})");
            DestroyImmediate(canvasObj);
            return;
        }

        // 3. CHỤP ẢNH VỚI PADDING (LỀ AN TOÀN)
        // Cộng thêm padding vào 2 bên
        int width = Mathf.CeilToInt(bounds.width) + PADDING * 2;
        int height = Mathf.CeilToInt(bounds.height) + PADDING * 2;
        
        // Resize canvas tạm cho vừa khít với kích thước đã tính + padding
        tempCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        
        // Đưa clone về giữa canvas
        cloneRect.pivot = new Vector2(0.5f, 0.5f);
        cloneRect.anchorMin = new Vector2(0.5f, 0.5f);
        cloneRect.anchorMax = new Vector2(0.5f, 0.5f);
        cloneRect.anchoredPosition = Vector2.zero;

        // Tạo Camera
        GameObject camObj = new GameObject("TempCam_Snapshot");
        Camera cam = camObj.AddComponent<Camera>();
        cam.backgroundColor = new Color(0,0,0,0); // Nền trong suốt
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.orthographic = true;
        // Chỉnh độ zoom của camera vừa khít với chiều cao đã tính
        cam.orthographicSize = height / 2f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        
        // Đặt camera nhìn thẳng vào tâm canvas
        cam.transform.position = canvasObj.transform.position + Vector3.back * 10f;
        
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 32, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        
        cam.Render();

        // 4. LƯU ẢNH
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        // Đọc toàn bộ render texture
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        
        byte[] bytes = texture.EncodeToPNG();
        string fileName = selectedObj.name + "_Merged.png";
        string filePath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(filePath, bytes);

        // Dọn dẹp
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);
        DestroyImmediate(camObj);
        DestroyImmediate(canvasObj);

        // 5. IMPORT VÀ GÁN LẠI
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(filePath);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; // UI không cần mipmap
            // Quan trọng: Wrap Mode = Clamp để tránh bị lặp viền
            importer.wrapMode = TextureWrapMode.Clamp;
            
            // Tự động cắt bỏ phần trong suốt thừa (Trim)
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            
            importer.SaveAndReimport();
        }

        // --- THAY THẾ ---
        Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
        if (newSprite != null)
        {
            Undo.RecordObject(selectedObj, "Merge UI");

            // Xóa hết con
            var children = new List<GameObject>();
            foreach (Transform child in selectedObj.transform) children.Add(child.gameObject);
            children.ForEach(c => Undo.DestroyObjectImmediate(c));

            // Thêm Image
            Image img = selectedObj.GetComponent<Image>();
            if (img == null) img = Undo.AddComponent<Image>(selectedObj);
            
            img.sprite = newSprite;
            img.color = Color.white; // Reset màu
            img.SetNativeSize(); // Đưa về kích thước chuẩn
            
            Debug.Log($"<color=green>Xong! Đã gộp {selectedObj.name} thành công (kèm padding an toàn).</color>");
        }
    }

    // Hàm tính toán khung bao quanh trong World Space
    static Rect GetScreenRectWorld(RectTransform root)
    {
        Vector3[] corners = new Vector3[4];
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        var graphics = root.GetComponentsInChildren<Graphic>(true);
        if (graphics.Length == 0) return Rect.zero;

        foreach (var g in graphics)
        {
            g.rectTransform.GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                min = Vector3.Min(min, corner);
                max = Vector3.Max(max, corner);
            }
        }
        
        // Chuyển từ World bounds sang kích thước local để chụp
        return new Rect(0, 0, max.x - min.x, max.y - min.y);
    }
}
#endif