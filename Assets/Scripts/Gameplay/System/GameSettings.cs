using UnityEngine;

[CreateAssetMenu(fileName = "new GameSettings", menuName = "Game/GameSettings")]
public class GameSettings : ScriptableObject
{
    public float speedSupply = 5f;
    public float speedRail = 5f;
    public float durationCombo = 1f;
    public float timerLaser = 5f;
    public float timerBomb = 5f;
    public float distanceRailSpacing = 1.5f;
}
