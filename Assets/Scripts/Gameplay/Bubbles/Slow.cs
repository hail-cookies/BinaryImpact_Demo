using UnityEngine;

public class Slow : Bubble
{
    static uint eventKey = 0;

    public override void LeavePlay()
    {
        Game.SpeedRail = 
            Game.GameSettings.speedRail * Game.GameSettings.multiplierSlow;
        Game.SpeedSupply =
            Game.GameSettings.speedSupply * Game.GameSettings.multiplierSlow;

        eventKey = Game.TimedEvents.ModifyOrAdd(
            eventKey,
            Time.time,
            Game.GameSettings.timerSlow,
            ResetTimerSpawn);
    }

    static bool ResetTimerSpawn()
    {
        Game.SpeedRail = Game.GameSettings.speedRail;
        Game.SpeedSupply = Game.GameSettings.speedSupply;
        return true;
    }
}
