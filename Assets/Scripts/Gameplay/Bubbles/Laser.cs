using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Bubble
{
    public override void AbilityTrigger()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
    }

    bool suspended = false;
    public override void AbilitySuspend(bool state)
    {
        suspended = state;
    }

    public override void AbilityReset()
    {
        StopAllCoroutines();
        suspended = false;
    }

    IEnumerator CountDown()
    {
        float timer = Game.Instance.gameSettings.timerLaser + 1f;

        if(Label)
            Label.Destroy();
        Label = Label.Create(transform, timer + "", 50, Color.white);

        while (timer >  1f)
        {
            if (suspended)
            {
                yield return null;
                continue;
            }

            timer -= Time.deltaTime;

            int count = (int)timer;
            Label.Text.text = count.ToString();

            Color c = Label.Text.color;
            c.a = 1f - (timer - count);
            Label.Text.color = c;

            yield return null;
        }

        float t = 0;
        Rail owner = null;
        foreach(var rail in Game.Instance.rails)
            if(rail.Contains(Body))
            {
                rail.TryGetBody(Body, out var result);
                t = result.t;
                owner = rail;
                break;
            }

        Score.Add(gameObject, transform.position, this);
        foreach(var rail in Game.Instance.rails)
            if(rail != owner)
                if(rail.TryGetBody(t, out var result))
                {
                    if (TryGetBubble(result.Body.gameObject, out var bubble))
                    {
                        rail.Remove(result.Body);
                        Score.Add(gameObject, transform.position, bubble);
                        Game.DestroyBubble(bubble);
                    }
                }

        owner.Remove(Body);
        Game.DestroyBubble(this);
    }
}
