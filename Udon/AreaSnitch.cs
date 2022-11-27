
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AreaSnitch : UdonSharpBehaviour
{
    public UdonBehaviour[] reportTo;
    public string behavioursPlayerVariable = "snitchedTarget";
    public string behavioursLocalPlayerVariable = "snitchedLocalTarget";

    private void Report(string playerVariableName, VRCPlayerApi player)
    {
        int nReports = reportTo.Length;
        for (int i = 0; i < nReports; i++)
        {
            UdonBehaviour behaviour = reportTo[i];
            if (behaviour == null)
            {
                continue;
            }
            behaviour.SetProgramVariable(playerVariableName, player);
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Report(behavioursPlayerVariable, player);
        if (player.isLocal)
        {
            Report(behavioursLocalPlayerVariable, player);
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        Report(behavioursPlayerVariable, null);
        if (player.isLocal)
        {
            Report(behavioursLocalPlayerVariable, null);
        }
    }

    
}
