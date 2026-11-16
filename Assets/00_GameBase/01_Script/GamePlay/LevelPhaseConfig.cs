using UnityEngine;
using System.Collections.Generic;

// Class chứa dữ liệu của 1 Phase
[System.Serializable]
public class PhaseData
{
    public string phaseName; // Tên (cho dễ nhớ)
    public List<ShadowSlot> shadows; // List các bóng sẽ hiện trong phase này
}

public class LevelPhaseConfig : MonoBehaviour
{
    // List chứa các Phase theo thứ tự 0, 1, 2...
    public List<PhaseData> listPhases;

    // Hàm hỗ trợ tự động setup nếu lười kéo tay (Optional)
    public void AutoSetup()
    {
        listPhases = new List<PhaseData>();
        PhaseData p0 = new PhaseData();
        p0.phaseName = "Default Phase";
        p0.shadows = new List<ShadowSlot>(GetComponentsInChildren<ShadowSlot>(true));
        listPhases.Add(p0);
    }
}