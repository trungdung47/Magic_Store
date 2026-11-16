using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Data", menuName = "MenuData", order = 0)]
public class HomeMenuData : ScriptableObject
{
    public List<DanhSachData> lsData;
}

[System.Serializable]
public class DanhSachData
{
    public int idLevel;
    public GameObject levelPrefab;
    public Sprite iconLevel;
    public string sceneName;
}
