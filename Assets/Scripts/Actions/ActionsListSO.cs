using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionsListSO", menuName = "Scriptable Objects/ActionsListSO")]
public class ActionsListSO : ScriptableObject
{
    public List<ActionSO> actionList =  new List<ActionSO>();
}
