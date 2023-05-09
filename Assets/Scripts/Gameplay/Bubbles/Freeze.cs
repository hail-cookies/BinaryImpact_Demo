using UnityEngine;

public class Freeze : Bubble
{
    static uint eventKey = 0;

    public override void LeavePlay()
    {
        Game.TimerSpawn = -1f;
        eventKey = Game.TimedEvents.ModifyOrAdd(
            eventKey,
            Time.time,
            Game.GameSettings.timerSlow,
            ResetTimerSpawn);
    }

    static bool ResetTimerSpawn()
    {
        Game.TimerSpawn = Game.SpawnSettings.spawnInterval;
        return true;
    }
}
