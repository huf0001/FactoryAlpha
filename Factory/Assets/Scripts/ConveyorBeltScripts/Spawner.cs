﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class ItemIdentifierPair
    {
        [SerializeField] private GameObject item;
        [SerializeField] private Identifier identifier;

        public GameObject Item
        {
            get
            {
                return item;
            }
        }

        public Identifier Identifier
        {
            get
            {
                return identifier;
            }
        }
    }

    [System.Serializable]
    public class ItemCountPair
    {
        [SerializeField] private GameObject item;
        [SerializeField] private int count;

        public ItemCountPair(GameObject i, int c)
        {
            item = i;
            count = c;
        }

        public GameObject Item
        {
            get
            {
                return item;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                count = value;
            }
        }
    }

    [SerializeField] private float secBetweenSpawns = 2f;
    [SerializeField] ItemIdentifierPair[] easyItems;
    [SerializeField] ItemIdentifierPair[] mediumItems;
    [SerializeField] ItemIdentifierPair[] hardItems;

    private Dictionary<Identifier, ItemCountPair> allObjects = new Dictionary<Identifier, ItemCountPair>();
    private Dictionary<Identifier, ItemCountPair> restockObjects = new Dictionary<Identifier, ItemCountPair>();
    private float time = 0f;
    
    // Use this for initialization
	void Start ()
    {
        int difficulty;
        int count;

        switch (PlayerPrefs.GetString("difficulty"))
        {
            case "hard":
                difficulty = 3;
                count = 2;
                break;
            case "medium":
                difficulty = 2;
                count = 3;
                break;
            default:
                difficulty = 1;
                count = 4;
                break;
        }

        if (difficulty > 0 && easyItems.Length > 0)
        {
            foreach (ItemIdentifierPair o in easyItems)
            {
                allObjects.Add(o.Identifier, new ItemCountPair(o.Item, count));
                restockObjects.Add(o.Identifier, new ItemCountPair(o.Item, 0));
            }
        }

        if (difficulty > 1 && mediumItems.Length > 0)
        {
            foreach (ItemIdentifierPair o in mediumItems)
            {
                allObjects.Add(o.Identifier, new ItemCountPair(o.Item, count));
                restockObjects.Add(o.Identifier, new ItemCountPair(o.Item, 0));
            }
        }

        if (difficulty > 2 && hardItems.Length > 0)
        {
            foreach (ItemIdentifierPair o in hardItems)
            {
                allObjects.Add(o.Identifier, new ItemCountPair(o.Item, count));
                restockObjects.Add(o.Identifier, new ItemCountPair(o.Item, 0));
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (PlayerPrefs.GetString("active") != "false")
        {
            if (time > secBetweenSpawns)
            {
                Spawn();
                time = Time.deltaTime;
            }
            else
            {
                time += Time.deltaTime;
            }
        }
	}

    private void Spawn()
    {
        int i = 0;
        GameObject spawning;
        List<Identifier> spawnables = GetSpawnables();

        if (spawnables.Count == 0)
        {
            ReStockAll();
            spawnables = GetSpawnables();
        }

        if (spawnables.Count > 0)
        {
            i = Random.Range(0, spawnables.Count - 1);
            allObjects[spawnables[i]].Count -= 1;
            spawning = Instantiate(allObjects[spawnables[i]].Item);
            spawning.GetComponent<Movable>().Spawner = this;
            spawning.transform.position = transform.position;
            spawning.transform.rotation = transform.rotation;
        }
    }

    private List<Identifier> GetSpawnables()
    {
        List<Identifier> spawnables = new List<Identifier>();

        foreach (KeyValuePair<Identifier, ItemCountPair> p in allObjects)
        {
            if (p.Value.Count > 0)
            {
                for (int j = 0; j < p.Value.Count - 1; j++)
                {
                    spawnables.Add(p.Key);
                }
            }
        }

        return spawnables;
    }

    private void ReStockAll()
    {
        foreach (KeyValuePair<Identifier, ItemCountPair> p in allObjects)
        {
            p.Value.Count += restockObjects[p.Key].Count;
            restockObjects[p.Key].Count = 0;
        }
    }

    public void ReStockComponent(Identifier id)
    {
        restockObjects[id].Count += 1;
    }
}
