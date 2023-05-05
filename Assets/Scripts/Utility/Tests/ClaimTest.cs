using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClaimTest : MonoBehaviour
{
    public CircleBody body;
    public int claim = 0;
    public bool allow = true;
    public uint key;

    public string owned;

    private void Update()
    {
        owned = body.Ownership.HasAccess(key) + "";

        if (claim != 0)
        {
            if (claim > 0)
            {
                if (body.Ownership.HasAccess(key) || body.Ownership.Claim(BodyClaimed, out key))
                    Debug.Log(name + " owns " + body.name);
            }
            else
            {
                if (body.Ownership.Release(key))
                    Debug.Log(name + " released " + body.name);
            }

            claim = 0;
        }
    }

    bool BodyClaimed(CircleBody body)
    {
        Debug.Log(name + " " + (allow ? "lost " : "kept ") + body.name);
        return allow;
    }
}
