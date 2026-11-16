using UnityEngine;
using UnityEngine.SceneManagement;

namespace _00_GameBase._01_Script
{
    public class GameScene : MonoBehaviour
    {
        public GameObject winUI;
        public Animator animator;
        
        [Header("Thời gian chờ trước khi về Home")]
        public float delay = 3f; 

        public void Init()
        {
            if(winUI != null) winUI.SetActive(false);
        }

        public void ShowWin()
        {
            if(winUI != null) winUI.SetActive(true);

            // Chạy Animation (nếu có)
            if (animator != null)
            {
                animator.SetTrigger("Win");
            }
            
            Invoke(nameof(ReturnToMenu), delay);
        }

        void ReturnToMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("HomeScene");
        }
    }
}