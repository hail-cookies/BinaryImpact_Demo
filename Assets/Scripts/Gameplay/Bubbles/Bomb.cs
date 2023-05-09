using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Bomb : Bubble
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
        float timer = Game.Instance.gameSettings.timerBomb + 1f;

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
        
        Vector2 halfExtents = new Vector2(
            Game.Instance.gameSettings.distanceRailSpacing,
            Game.Instance.spawnSettings.bubbleRadius * 2f);
        CirclePhysics.CheckRectangle(Body.CurrentPosition, halfExtents, out var hits);

        foreach (var hit in hits)
        {
            if (hit == Body)
                continue;

            if(TryGetBubble(hit.gameObject, out var bubble))
            {
                var body = bubble.Body;
                if (body.Ownership.Claimed &&
                    !Game.Instance.supply.Contains(body))
                {
                    foreach (var rail in Game.Instance.rails)
                        if (rail.Remove(body))
                            break;
                    Score.Add(gameObject, transform.position, bubble);
                    Game.DestroyBubble(bubble);
                }
            }
        }

        foreach (var rail in Game.Instance.rails)
            if (rail.Remove(Body))
                break;
        
        Score.Add(gameObject, transform.position, this);
        Game.DestroyBubble(this);
    }
}
