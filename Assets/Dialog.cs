using UnityEngine;

[CreateAssetMenu(fileName = "Dialog", menuName = "LudumDare46/Dialog", order = 0)]
public class Dialog : ScriptableObject
{
    public ActorType actor;

    [TextArea]
    public string text;
    public Dialog nextDialog;
}