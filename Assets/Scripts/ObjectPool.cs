using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    static Dictionary<GameObject, ObjectPool> activePools = new Dictionary<GameObject, ObjectPool>();
    static Dictionary<GameObject, (ObjectPool, Object)> assignments = new Dictionary<GameObject, (ObjectPool, Object)>();

    public GameObject prefab;
    Queue<(GameObject, Object)> available = new Queue<(GameObject, Object)>();

    static ObjectPool GetPool(GameObject prefab)
    {
        if (activePools.ContainsKey(prefab))
            return activePools[prefab];
        else
        {
            ObjectPool pool = new ObjectPool();
            pool.prefab = prefab;
            activePools.Add(prefab, pool);

            return pool;
        }
    }

    #region Create<T>
    public static (GameObject, T) Create<T>(GameObject prefab) where T : Object
    {
        return Create<T>(prefab, Vector3.zero, Quaternion.identity, null);
    }

    public static (GameObject, T) Create<T>(GameObject prefab, Transform parent) where T : Object
    {
        return Create<T>(prefab, Vector3.zero, Quaternion.identity, parent);
    }

    public static (GameObject, T) Create<T>(GameObject prefab, Quaternion rot, Transform parent) where T : Object
    {
        return Create<T>(prefab, Vector3.zero, rot, parent);
    }

    public static (GameObject, T) Create<T>(GameObject prefab, Vector3 pos, Transform parent) where T : Object
    {
        return Create<T>(prefab, pos, Quaternion.identity, parent);
    }

    public static (GameObject, T) Create<T>(GameObject prefab,Vector3 pos, Quaternion rot, Transform parent) where T : Object
    {
        ObjectPool pool = GetPool(prefab);
        //Debug.Log("Pools: " + activePools.Count);
        //Instantiate new object
        if (pool.available.Count < 1)
        {
            GameObject go = Object.Instantiate(prefab, pos, rot, parent);
            go.SetActive(true);

            var component = go.GetComponent<T>();
            assignments.Add(go, (pool, component));
            return (go, component);
        }
        //Get existing object
        else
        {
            var template = pool.available.Dequeue();
            GameObject gameObject = template.Item1;
            //Object is invalid
            if(!gameObject)
            {
                GameObject go = Object.Instantiate(prefab, pos, rot, parent);
                go.SetActive(true);

                var component = go.GetComponent<T>();
                assignments.Add(go, (pool, component));
                return (go, component);
            }

            Transform t = gameObject.transform;
            t.position = pos;
            t.rotation = rot;
            t.SetParent(parent);

            gameObject.SetActive(true);

            return (gameObject, (T)template.Item2);
        }
    }
    #endregion

    #region Create
    public static GameObject Create(GameObject prefab)
    {
        return Create(prefab, Vector3.zero, Quaternion.identity, null);
    }

    public static GameObject Create(GameObject prefab, Transform parent)
    {
        return Create(prefab, Vector3.zero, Quaternion.identity, parent);
    }

    public static GameObject Create(GameObject prefab, Quaternion rot, Transform parent)
    {
        return Create(prefab, Vector3.zero, rot, parent);
    }

    public static GameObject Create(GameObject prefab, Vector3 pos, Transform parent)
    {
        return Create(prefab, pos, Quaternion.identity, parent);
    }

    public static GameObject Create(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        ObjectPool pool = GetPool(prefab);
        //Debug.Log("Pools: " + activePools.Count);
        //Instantiate new object
        if (pool.available.Count < 1)
        {
            GameObject go = Object.Instantiate(prefab, pos, rot, parent);
            go.SetActive(true);

            assignments.Add(go, (pool, null));
            return go;
        }
        //Get existing object
        else
        {
            GameObject gameObject = pool.available.Dequeue().Item1;

            Transform t = gameObject.transform;
            t.position = pos;
            t.rotation = rot;
            t.SetParent(parent);

            gameObject.SetActive(true);

            return gameObject;
        }
    }
    #endregion

    public static bool Destroy(GameObject target)
    {
        if(target == null)
            return false;

        target.SetActive(false);

        if (!assignments.ContainsKey(target))
            return false;

        var assignment = assignments[target];
        if(!assignment.Item1.available.Contains((target, assignment.Item2)))
            assignment.Item1.available.Enqueue((target, assignment.Item2));

        return true;
    }
}
