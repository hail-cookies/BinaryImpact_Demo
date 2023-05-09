using System;
using System.Collections.Generic;

public class TimedEvents
{
    static uint currentKey = 1;

    public struct TimedEvent
    {
        public uint key;
        public float created;
        public float duration;
        public Func<bool> callBack;

        public TimedEvent(uint key, float created, float duration, Func<bool> callBack)
        {
            this.key = key;
            this.created = created;
            this.duration = duration;
            this.callBack = callBack;
        }
    }
    List<TimedEvent> events = new List<TimedEvent>();

    public bool Contains(uint key)
    {
        if(key == 0)
            return false;

        for(int i = 0; i < events.Count; i++)
            if (events[i].key == key)
                return true;

        return false;
    }

    public uint ModifyOrAdd(uint key, float time, float duration, Func<bool> callBack)
    {
        for(int i = 0; i < events.Count; ++i)
            if (events[i].key == key)
            {
                events[i] = new TimedEvent(key, time, duration, callBack);
                return key;
            }

        return Add(time, duration, callBack);
    }

    public uint Add(float time, float duration, Func<bool> callBack)
    {
        uint key = currentKey++;
        events.Add(new TimedEvent(key, time, duration, callBack));

        return key;
    }

    public bool Remove(uint key)
    {
        for(int i = 0; i < events.Count; i++)
            if (events[i].key == key)
            {
                events.RemoveAt(i);
                return true;
            }

        return false;
    }

    public bool TryGet(uint key, out TimedEvent result)
    {
        result = default;
        if (key == 0)
            return false;

        for(int i = 0; i < events.Count; i++)
            if (events[i].key == key)
            {
                result = events[i];
                return true;
            }

        return false;
    }

    public int Count => events.Count;
    public TimedEvent Get(int index) => events[index];
}
