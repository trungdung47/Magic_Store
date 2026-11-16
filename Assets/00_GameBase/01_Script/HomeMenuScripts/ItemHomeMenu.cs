using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _00_GameBase._01_Script.LevelScript
{
    public class ItemHomeMenu : MonoBehaviour
    {
        [Header("Info")]
        public int levelIndex; // ID của level
        
        [Header("UI Components")]
        public Button btnChoose;
        public Image imgIcon;
        public Image imgLock;

        private bool isLocked;

        private void Start()
        {
            // Xóa hết listener cũ trước khi add mới để tránh bị double click
            btnChoose.onClick.RemoveAllListeners();
            btnChoose.onClick.AddListener(OnClickLevel);
        }

        public void Init(DanhSachData data, int index)
        {
            levelIndex = index; 
            
            // Hiển thị icon từ Data (nếu có)
            if(imgIcon != null && data.iconLevel != null) 
            {
                imgIcon.sprite = data.iconLevel;
            }
            
            // Cập nhật tên nút (nếu muốn debug dễ hơn)
            this.gameObject.name = "Level_Btn_" + index;
        }
        
        public void SetLockLevel(bool _isLocked)
        {
            this.isLocked = _isLocked;
            
            // Hiện ổ khóa
            if(imgLock != null) 
                imgLock.gameObject.SetActive(_isLocked);
            
            // Tùy chọn: Làm mờ nút nếu bị khóa
            // btnChoose.interactable = !_isLocked; 
            
            // Tùy chọn 2: Nếu bị khóa thì làm tối màu đi chút
            if (imgIcon != null)
            {
                imgIcon.color = _isLocked ? Color.gray : Color.white;
            }
        }

        private void OnClickLevel()
        {
            if (isLocked)
            {
                Debug.Log($"Level {levelIndex} đang bị khóa! Hãy hoàn thành level trước.");
                return;
            }

            Debug.Log("Người chơi chọn level: " + levelIndex);

            // 1. Lưu ID level muốn chơi vào PlayerPrefs
            PlayerPrefs.SetInt("CurrentLevel", levelIndex);
            PlayerPrefs.Save();

            // 2. Load Scene "GamePlay"
            // LƯU Ý: Đảm bảo bạn đã add scene "GamePlay" vào Build Settings
            SceneManager.LoadScene("GamePlay");
        }
    }
}