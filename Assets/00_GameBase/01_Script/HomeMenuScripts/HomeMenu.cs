using System.Collections.Generic;
using System.Linq; // Cần thêm thư viện này để sắp xếp
using _00_GameBase._01_Script.LevelScript;
using UnityEngine;

namespace _00_GameBase._01_Script
{
    public class HomeMenu : MonoBehaviour
    {
        [Header("Data Menu")]
        public HomeMenuData data; 

        [Header("UI Items")] 
        public List<ItemHomeMenu> items; 
        
        private void Start()
        {
            // --- FIX 1: Tự động sắp xếp list items theo thứ tự trong Hierarchy ---
            // Giúp tránh việc bạn kéo thả lộn xộn trong Inspector dẫn đến sai ID level
            items.Sort((x, y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));

            // 1. Lấy level cao nhất (Mặc định là 0 - Bài 1)
            int maxLevelReached = PlayerPrefs.GetInt("MaxLevelReached", 0);

            // 2. Duyệt qua từng nút để cài đặt
            for (int i = 0; i < items.Count; i++)
            {
                // Kiểm tra xem có dữ liệu cho level này không
                if (i < data.lsData.Count)
                {
                    // Bật nút lên nếu nó đang bị tắt
                    items[i].gameObject.SetActive(true);

                    // Init dữ liệu (i chính là ID Level: 0, 1, 2...)
                    items[i].Init(data.lsData[i], i);

                    // Logic Khóa:
                    bool isLocked = i > maxLevelReached;
                    items[i].SetLockLevel(isLocked);
                }
                else
                {
                    // Nếu thừa nút UI mà thiếu Data thì tắt nút đó đi
                    items[i].gameObject.SetActive(false);
                }
            }
        }
        
        // Hàm reset để test (gọi qua button hoặc inspector context menu)
        [ContextMenu("Reset Progress")]
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey("MaxLevelReached");
            PlayerPrefs.DeleteKey("CurrentLevel");
            Debug.Log("Đã reset dữ liệu người chơi!");
        }
    }
}