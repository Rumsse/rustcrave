using System.Collections.Generic;
using UnityEngine;

public class ObjectRegistry : MonoBehaviour
{
    public static ObjectRegistry Instance { get; private set; }

    [SerializeField] private List<GameObject> targetObjects = new();

    private readonly Dictionary<string, List<GameObject>> tagDictionary = new();

    private void Awake()
    {
        Instance = this;

        foreach (var target in targetObjects)
        {
            if (!tagDictionary.ContainsKey(target.tag))
                tagDictionary[target.tag] = new();

            tagDictionary[target.tag].Add(target);
        }
    }

    public bool TryGetObjects(string targetTag, out List<GameObject> objects) => tagDictionary.TryGetValue(targetTag, out objects);
}