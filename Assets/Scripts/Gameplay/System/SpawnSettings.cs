using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "new SpawnSettings", menuName = "Game/SpawnSettings")]
public class SpawnSettings : ScriptableObject
{
    public float spawnInterval = 1f;
    public float bubbleRadius = 0.5f;
    public Color c_green, c_blue, c_red, c_yellow;
    public Material m_single, m_double, m_triple, m_quadruple;

    public WeightedList<GameObject> spawnPrefabs = new WeightedList<GameObject>();
    public WeightedList<BubbleSelect> spawnColors = new WeightedList<BubbleSelect>();
    public WeightedList<int> spawnColorCounts = new WeightedList<int>();

    public void Initialize()
    {
        spawnPrefabs.Reset();
        spawnColors.Reset();
        spawnColorCounts.Reset();
    }

    public Bubble CreateBubble(Rail supply)
    {
        if (!supply.HasSpace) 
            return null;

        var prefab = GetPrefab();
        var created = ObjectPool.Create<Bubble>(prefab).Item2;
        created.transform.localScale = Vector3.one * 2f * bubbleRadius;
        created.bubbleType = prefab.GetComponent<Bubble>().bubbleType;
        SetColor(created);

        var body = created.Body;
        body.CurrentPosition = supply.SamplePoint(0);
        body.Radius = bubbleRadius;
        supply.Add(body);

        return created;
    }

    GameObject GetPrefab()
    {
        return spawnPrefabs.GetOption();
    }

    void SetColor(Bubble bubble)
    {
        int count = (bubble.bubbleType & BubbleType.Blocked) > 0 ? 0: spawnColorCounts.GetOption("Count: ");
        bubble.Body.Renderer.material =
            count <= 1 ? m_single :
            count == 2 ? m_double :
            count == 3 ? m_triple : m_quadruple;

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                //Get color option
                BubbleSelect select = spawnColors.GetOption((BubbleSelect t) =>
                    //Cast BubblSelect to BubbleType and check if color was already selected
                    ((BubbleType)(1 << (int)t) & bubble.bubbleType) == 0);
                //Add selection
                bubble.bubbleType |= (BubbleType)(1 << (int)select);

                Color color =
                    select == BubbleSelect.Blue ? c_blue :
                    select == BubbleSelect.Green ? c_green :
                    select == BubbleSelect.Red ? c_red :
                    select == BubbleSelect.Yellow ? c_yellow :
                    Color.black;
                bubble.Body.Renderer.material.SetColor("_Color_" + i, color);
            }
        }
        else
            bubble.Body.Renderer.material.SetColor("_Color_0", Color.black);
    }
}
