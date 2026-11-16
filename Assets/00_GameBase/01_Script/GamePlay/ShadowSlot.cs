using UnityEngine;

public class ShadowSlot : MonoBehaviour
{
    [Header("Cài đặt ID (Phải khớp Goal)")]
    public int id;

    [HideInInspector] 
    public bool isOccupied = false; // Biến này để check xem ô đã bị lấp chưa
}