using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mission/MissionObjectiveSO")]
public class MissionObjectiveSO : ScriptableObject 
{
    [TextArea(3,10)]
    [Tooltip("What player need to do in this mission")]
    public List<string> objectives;

    [Space(10)]
    [TextArea(3,10)]
    public string missionCompleteText;
}