using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Score
{
    public class Combo
    {
        List<long> values;

        public FloatingText FloatingText;
        public float LastUpdate;
        public long LastScore { get; private set; }
        public long CurrentScore { get; private set; }
        public long Multiplier { get => values.Count; }

        public Combo(FloatingText text, float time)
        {
            values = new List<long>();
            FloatingText = text;
            LastUpdate = time;
            LastScore = 0;
            CurrentScore = 0;
        }

        /// <summary>
        /// Add a score value
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Change in total score</returns>
        public long Add(long value, float time)
        {
            LastUpdate = time;
            values.Add(value);

            CurrentScore = 0;
            foreach (var v in values)
                CurrentScore += v;
            CurrentScore *= Multiplier;

            long delta = CurrentScore - LastScore;
            long last = LastScore;
            LastScore = CurrentScore;

            FloatingText.Text.text = CurrentScore + (Multiplier > 1 ? "\nCOMBO " + Multiplier + "X" : "");
            FloatingText.created = time;
            return delta;
        }
    }
    static Dictionary<GameObject, Combo> combos = new Dictionary<GameObject, Combo>();

    public static long Current { get; private set; }

    public static void Add(GameObject source, Vector2 position, Bubble bubble)
    {
        Add(source, position, 1);
    }

    public static void Add(GameObject source, Vector2 position, long value)
    {
        float time = Time.time;
        //Source has no entry
        if (!combos.ContainsKey(source))
            combos.Add(source, CreateCombo(position, time));
        //Entry is too old
        else if (time - combos[source].LastUpdate > Game.Instance.c_comboDuration)
        {
            combos[source].FloatingText.Destroy();
            combos[source] = CreateCombo(position, time);
        }
        //Update entry
        Current += combos[source].Add(value, time);
    }

    static Combo CreateCombo(Vector2 position, float time)
    {
        var dur = UI.Instance.c_scoreTextDuration;
        var text = FloatingText.Create(
            position,
            UI.Instance.c_scoreTextVelocity,
            -Vector2.one,
            false,
            dur,
            dur * 0.8f,
            dur * 0.2f,
            "",
            UI.Instance.c_scoreTextSize,
            UI.Instance.c_scoreTextColor);

        return new Combo(text, time);
    }
}
