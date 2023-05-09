using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new GameSettings", menuName = "Game/GameSettings")]
public class GameSettings : ScriptableObject
{
    [System.Serializable]   
    public class ScoringRule
    {
        public BubbleSelect bubble;
        public long value;
    }

    public float speedSupply = 5f;
    public float speedRail = 5f;
    public float timerCombo = 1f;
    public float timerLaser = 5f;
    public float timerBomb = 5f;
    public float timerFreeze = 3f;
    public float timerSlow = 5f;
    public float distanceRailSpacing = 1.5f;
    public float multiplierSlow = 0.5f;
    public float multiplierSpeedup = 0.001f;

    public List<ScoringRule> scoringRules = new List<ScoringRule>();
    public long GetScoringValue(BubbleSelect bubble)
    {
        foreach(ScoringRule rule in scoringRules)
            if(rule.bubble == bubble)
                return rule.value;

        return 0;
    }
}
