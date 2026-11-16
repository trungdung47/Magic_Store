using UnityEngine;

namespace _00_GameBase._01_Script
{
    public class PlayerContaint : MonoBehaviour
    {
        public LevelController levelController; 
        public InputController inputController; 

   
        public void Init()
        {
            if (levelController != null)
            {
                levelController.Init();
            }
        
          
            if (inputController != null) inputController.Init();
        }
    }
}