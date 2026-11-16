using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Bắt buộc Object này phải có Image (vì giờ nó là Image chính luôn)
// [RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class Pieces : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Cài đặt")]
    public int id;
    public float snapDistance = 5f; // Khoảng cách hút

    // --- BIẾN HỆ THỐNG ---
    private Transform parentAfterDrag;      // Cha cũ (Content của ScrollView)
    private int originalSiblingIndex;       // Thứ tự cũ
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private ScrollRect parentScrollRect;    // ScrollView cha
    private RectTransform scrollRectTransform; 

    // --- BIẾN KÍCH THƯỚC ---
    private Image myImage;      // Ảnh của chính tôi
    private Vector2 smallSize;  // Size bé (trong list)
    private Vector2 nativeSize; // Size gốc (lớn)
    private bool isExpanded = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        myImage = GetComponent<Image>();
        
        // Tìm Canvas tổng
        canvas = GetComponentInParent<Canvas>();
        
        // Tìm ScrollView chứa nó
        parentScrollRect = GetComponentInParent<ScrollRect>();
        if (parentScrollRect != null)
        {
            scrollRectTransform = parentScrollRect.GetComponent<RectTransform>();
        }

        // --- TÍNH TOÁN SIZE ---
        // 1. Lưu size hiện tại (đang bị Grid ép nhỏ) là smallSize
        smallSize = rectTransform.sizeDelta;

        // 2. Tính Native Size (Size thật của ảnh)
        myImage.preserveAspect = true; // Luôn giữ tỷ lệ ảnh đẹp
        myImage.SetNativeSize();       // Bung ra size thật để đo
        nativeSize = rectTransform.sizeDelta;
        
        // 3. Trả về size nhỏ để hiển thị trong list
        rectTransform.sizeDelta = smallSize;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. Lưu thông tin nhà cũ
        parentAfterDrag = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // 2. Setup để kéo
        if (parentScrollRect != null) parentScrollRect.enabled = false; // Tắt cuộn
        
        transform.SetParent(canvas.transform); // Đưa ra ngoài cùng
        transform.SetAsLastSibling();          // Nổi lên trên hết
        
        canvasGroup.blocksRaycasts = false;    // Cho phép Raycast xuyên qua để tìm Shadow

        // Reset trạng thái
        isExpanded = false;
        rectTransform.sizeDelta = smallSize; // Bắt đầu kéo với size nhỏ
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 1. Di chuyển theo chuột
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // 2. LOGIC: Trong hay Ngoài ScrollView?
        if (parentScrollRect != null)
        {
            // Kiểm tra chuột có nằm trong vùng ScrollView không
            bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
                scrollRectTransform, 
                Input.mousePosition, 
                canvas.worldCamera
            );

            if (!isInside && !isExpanded)
            {
                // -- RA NGOÀI: Phóng to (Native Size) --
                rectTransform.sizeDelta = nativeSize;
                isExpanded = true;
            }
            else if (isInside && isExpanded)
            {
                // -- VÀO TRONG: Thu nhỏ (Small Size) --
                rectTransform.sizeDelta = smallSize;
                isExpanded = false;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Bật lại tương tác
        canvasGroup.blocksRaycasts = true;
        if (parentScrollRect != null) parentScrollRect.enabled = true;

        ShadowSlot targetShadow = FindTargetShadow();

        if (targetShadow != null)
        {
            SnapToShadow(targetShadow);
        }
        else
        {
            ResetToScrollView();
        }
    }

    // --- HÀM 1: HÚT VÀO BÓNG ---
    private void SnapToShadow(ShadowSlot shadow)
    {
        // 1. Tìm Goal Container
        Transform shadowContainer = shadow.transform.parent;
        Transform levelRoot = shadowContainer.parent;
        Transform goalContainer = levelRoot.Find("Goal");
        if (goalContainer == null) goalContainer = shadowContainer;

        // 2. Chuyển nhà
        transform.SetParent(goalContainer);

        // 3. Khớp vị trí
        transform.position = shadow.transform.position;
        transform.localRotation = shadow.transform.localRotation;
        transform.localScale = Vector3.one;
        rectTransform.sizeDelta = nativeSize;

        // 4. Tắt Shadow & Báo cáo
        shadow.gameObject.SetActive(false);
        if (LevelController.Instance != null) 
            LevelController.Instance.OnStepCompleted();
        
        this.enabled = false; 

        // --- BƯỚC MỚI: SẮP XẾP LẠI THỨ TỰ HIỂN THỊ ---
        // Để đảm bảo miếng nền nằm dưới, miếng chi tiết nằm trên
        SortPiecesInGoal(goalContainer);
    }

    // --- HÀM SẮP XẾP LOGIC Z-ORDER ---
    private void SortPiecesInGoal(Transform container)
    {
        // 1. Lấy tất cả các miếng ghép đang nằm trong Goal
        List<Pieces> activePieces = new List<Pieces>();
        foreach (Transform child in container)
        {
            Pieces p = child.GetComponent<Pieces>();
            if (p != null) activePieces.Add(p);
        }

        // 2. Sắp xếp list này theo ID tăng dần
        // (ID nhỏ = Vẽ trước/Nằm dưới)
        // (ID lớn = Vẽ sau/Nằm trên)
        activePieces.Sort((a, b) => a.id.CompareTo(b.id));

        // 3. Cập nhật lại vị trí trong Hierarchy theo list đã sắp xếp
        foreach (var p in activePieces)
        {
            p.transform.SetAsLastSibling();
        }
    }

    // --- HÀM 2: TRẢ VỀ LIST ---
    private void ResetToScrollView()
    {
        transform.SetParent(parentAfterDrag);
        transform.SetSiblingIndex(originalSiblingIndex);
        
        // Reset các thông số về mặc định
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        
        // Fix lỗi biến mất (Z phải bằng 0)
        Vector3 pos = transform.localPosition;
        pos.z = 0;
        transform.localPosition = pos;

        // Trả về kích thước nhỏ
        rectTransform.sizeDelta = smallSize;

        // Refresh lại Layout của ScrollView để nó sắp xếp lại vị trí
        LayoutGroup group = parentAfterDrag.GetComponent<LayoutGroup>();
        if(group != null)
        {
            group.enabled = false;
            group.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentAfterDrag.GetComponent<RectTransform>());
        }
    }

    // --- HÀM 3: TÌM SHADOW GẦN NHẤT ---
    private ShadowSlot FindTargetShadow()
    {
        ShadowSlot[] allShadows = FindObjectsOfType<ShadowSlot>();
        ShadowSlot bestCandidate = null;
        float closestDistance = float.MaxValue;

        foreach (ShadowSlot shadow in allShadows)
        {
            // So sánh ID và xem có trống không
            if (shadow.id == this.id && !shadow.isOccupied)
            {
                float distance = Vector3.Distance(transform.position, shadow.transform.position);
                if (distance <= snapDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    bestCandidate = shadow;
                }
            }
        }
        return bestCandidate;
    }
}