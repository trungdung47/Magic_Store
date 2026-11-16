using _00_GameBase._01_Script;
using UnityEngine;

public class GamePlayController : MonoBehaviour
{
    public static GamePlayController Instance; 

    [Header("References")]
    public PlayerContaint playerContain;
    public GameScene gameScene;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (gameScene != null) gameScene.Init();
        
        if (playerContain != null) playerContain.Init();
    }
    
    public void OnLevelComplete()
    {
        Debug.Log("GamePlay nhận tin: Đã thắng!");
        
        if (gameScene != null)
        {
            gameScene.ShowWin();
        }
    }
}