using UnityEngine;
using System.Collections.Generic;
using _00_GameBase._01_Script; 

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [Header("Cài đặt")]
    public Transform levelParent;
    public HomeMenuData levelData; 

    // --- THAY ĐỔI: Biến quản lý Phase thay vì List Shadow đơn lẻ ---
    private LevelPhaseConfig currentLevelConfig; 
    private int currentPhaseIndex = 0;
    private int itemsCollectedInCurrentPhase = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        foreach (Transform child in levelParent) Destroy(child.gameObject);
        
        int currentLevelID = PlayerPrefs.GetInt("CurrentLevel", 0);
        
        // Safety check
        if (levelData == null || levelData.lsData == null) return;
        if (currentLevelID >= levelData.lsData.Count) currentLevelID = 0;
        
        GameObject prefabToSpawn = levelData.lsData[currentLevelID].levelPrefab;
        
        if (prefabToSpawn != null)
        {
            GameObject newLevel = Instantiate(prefabToSpawn, levelParent);
            newLevel.transform.localPosition = Vector3.zero;
            newLevel.transform.localScale = Vector3.one;
            
            // --- THAY ĐỔI LOGIC INIT ---
            
            // 1. Tìm script cấu hình Phase trên Level vừa sinh ra
            currentLevelConfig = newLevel.GetComponent<LevelPhaseConfig>();

            if (currentLevelConfig == null)
            {
                Debug.LogError($"Lỗi: Prefab Level {currentLevelID} chưa gắn script 'LevelPhaseConfig'!");
                // Fallback: Nếu quên gắn thì chạy logic cũ hoặc báo lỗi
                return;
            }

            // 2. Nếu chưa setup phase nào thì auto setup (tránh crash game)
            if (currentLevelConfig.listPhases.Count == 0)
            {
                currentLevelConfig.AutoSetup();
            }
            
            // 3. Ẩn tất cả Shadow trước khi bắt đầu
            HideAllShadows();

            // 4. Mở Phase đầu tiên (Phase 0)
            currentPhaseIndex = 0;
            itemsCollectedInCurrentPhase = 0;
            ActivatePhase(0);
        }
        else
        {
            Debug.LogError("Chưa gắn Prefab vào Data cho Level " + currentLevelID);
        }
    }
    
    // Hàm ẩn sạch sẽ mọi shadow
    void HideAllShadows()
    {
        if (currentLevelConfig == null) return;
        foreach (var phase in currentLevelConfig.listPhases)
        {
            foreach (var shadow in phase.shadows)
            {
                if (shadow != null) shadow.gameObject.SetActive(false);
            }
        }
    }

    // Hàm bật một nhóm shadow cụ thể
    void ActivatePhase(int phaseIndex)
    {
        if (currentLevelConfig == null || phaseIndex >= currentLevelConfig.listPhases.Count) return;

        List<ShadowSlot> shadowsToOpen = currentLevelConfig.listPhases[phaseIndex].shadows;
        
        foreach (var shadow in shadowsToOpen)
        {
            if (shadow != null) 
            {
                shadow.gameObject.SetActive(true);
            }
        }
    }

    // --- THAY ĐỔI LOGIC CHECK HOÀN THÀNH ---
    public void OnStepCompleted()
    {
        if (currentLevelConfig == null) return;

        // 1. Tăng số lượng đã ghép trong Phase này
        itemsCollectedInCurrentPhase++;
        
        // 2. Lấy tổng số lượng cần ghép của Phase này
        int totalItemsInThisPhase = currentLevelConfig.listPhases[currentPhaseIndex].shadows.Count;

        // 3. Kiểm tra xong Phase chưa
        if (itemsCollectedInCurrentPhase >= totalItemsInThisPhase)
        {
            // --> XONG PHASE HIỆN TẠI
            
            // Kiểm tra xem còn Phase sau không
            if (currentPhaseIndex + 1 < currentLevelConfig.listPhases.Count)
            {
                // Còn -> Mở Phase tiếp theo
                currentPhaseIndex++;
                itemsCollectedInCurrentPhase = 0; // Reset biến đếm
                ActivatePhase(currentPhaseIndex);
            }
            else
            {
                // Hết Phase -> THẮNG GAME (Giữ nguyên logic cũ của bạn)
                HandleLevelComplete();
            }
        }
    }
    
    private void HandleLevelComplete()
    {
        if (GamePlayController.Instance != null)
            GamePlayController.Instance.OnLevelComplete();
        
        int currentLevelID = PlayerPrefs.GetInt("CurrentLevel", 0);
        int maxLevel = PlayerPrefs.GetInt("MaxLevelReached", 0);
        
        if (currentLevelID == maxLevel)
        {
            PlayerPrefs.SetInt("MaxLevelReached", maxLevel + 1);
            PlayerPrefs.Save();
        }
    }
}