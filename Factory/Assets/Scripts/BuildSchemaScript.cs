﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSchemaScript : MonoBehaviour
{
    public enum BuildRole
    {
        BaseComponent,
        AttachableComponent
    }

    [System.Serializable]
    public class ObjectRolePair
    {
        [SerializeField] private Identifier component;
        [SerializeField] private BuildRole role;

        public Identifier Component
        {
            get
            {
                return component;
            }
        }

        public BuildRole Role
        {
            get
            {
                return role;
            }
        }
    }

    [SerializeField] private string buildPointName = "BuildPoint";
    [SerializeField] private float distanceLimit;
    [SerializeField] private ObjectRolePair[] components;
    [SerializeField] private GameObject finalObject;

    private List<Identifier> pendingComponents = new List<Identifier>();
    private Dictionary<Identifier, GameObject> loadedComponents = new Dictionary<Identifier, GameObject>();
    private Transform buildPoint = null;

    // Use this for initialization
    void Start()
    {
        buildPoint = this.transform.parent.Find(buildPointName);

        if (buildPoint == null)
        {
            Debug.Log("Warning: Couldn't find the game object '" + buildPointName + "'. " + buildPointName + " should be childed to the build" +
                "zone that this schema is attached to.");
        }

        foreach (ObjectRolePair p in components)
        {
            pendingComponents.Add(p.Component);
        }
    }

    public bool BelongsToSchema(GameObject item)
    {
        IdentifiableScript ids = null;
        ids = item.GetComponent<IdentifiableScript>();

        if (ids != null)
        {
            return BelongsToSchema(ids);
        }

        return false;
    }

    public bool BelongsToSchema(IdentifiableScript ids)
    {
        foreach (ObjectRolePair p in components)
        {
            if (ids.HasIdentifier(p.Component))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsLoaded(IdentifiableScript ids)
    {
        foreach (ObjectRolePair orp in components)
        {
            if (ids.HasIdentifier(orp.Component))
            {
                foreach (KeyValuePair<Identifier, GameObject> kvp in loadedComponents)
                {
                    if (kvp.Key == orp.Component)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void HandleValidObject(GameObject item)
    {
        LoadObject(item);

        if (pendingComponents.Count == 0)
        {
            Build();
        }
    }

    private void LoadObject(GameObject item)
    {
        IdentifiableScript itemIds = item.GetComponent<IdentifiableScript>();
        //AttachScript attachBase = null;

        if (!itemIds.HasIdentifier(Identifier.Attached))
        {
            foreach (ObjectRolePair orp in components)
            {
                if (itemIds.HasIdentifier(orp.Component))
                {
                    if (item.GetComponent<MovableScript>() != null)
                    {
                        item.GetComponent<MovableScript>().Schema = this;
                    }

                    pendingComponents.Remove(orp.Component);
                    loadedComponents.Add(orp.Component, item);
                    itemIds.RemoveIdentifier(Identifier.HasNotBeenLoadedInBuildZoneYet);

                    this.transform.parent.gameObject.GetComponent<BuildZoneScript>().PlayLoadedSound();

                    MoveTowardsBuildPoint(item);

                    /*if (itemIds.HasIdentifier(Identifier.AttachBase))
                    {
                        attachBase = itemIds.gameObject.GetComponent<AttachScript>();

                        if (attachBase != null)
                        {
                            foreach (KeyValuePair<Transform, GameObject> kvp in attachBase.Attached)
                            {
                                LoadAttachedObject(kvp.Value);
                            }
                        }
                    }*/

                    return;
                }
            }
        }
    }

    private void MoveTowardsBuildPoint(GameObject item)
    {
        float increment = 0.1f;
        float distance = 0f;

        do
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, buildPoint.transform.position, increment);
            distance = Vector3.Distance(item.transform.position, buildPoint.position);
        } while (distance > distanceLimit);
    }

    /*private void LoadAttachedObject(GameObject item)
    {
        IdentifiableScript itemIds = item.GetComponent<IdentifiableScript>();

        if (!CheckIsLoaded(item))
        {
            foreach (ObjectRolePair orp in components)
            {
                if (itemIds.HasIdentifier(orp.Component))
                {
                    pendingComponents.Remove(orp.Component);
                    loadedComponents.Add(orp.Component, item);
                    itemIds.RemoveIdentifier(Identifier.HasNotBeenLoadedInBuildZoneYet);

                    return;
                }
            }
        }
    }*/

    private bool CheckIsLoaded(GameObject item)
    {
        foreach (KeyValuePair<Identifier, GameObject> p in loadedComponents)
        {
            if (p.Value == item)
            {
                return true;
            }
        }

        return false;
    }

    public void RemoveObject(GameObject item)
    {
        //AttachableScript attached = null;
        //AttachScript attachBase = null;

        foreach (KeyValuePair<Identifier, GameObject> p in loadedComponents)
        {
            if (p.Value == item)
            {
                pendingComponents.Add(p.Key);
                loadedComponents.Remove(p.Key);
                p.Value.GetComponent<IdentifiableScript>().RemoveIdentifier(Identifier.InBuildZone);

                /*if (item.GetComponent<IdentifiableScript>().HasIdentifier(Identifier.Attached))
                {
                    attached = item.GetComponent<AttachableScript>();

                    if (attached != null)
                    {
                        RemoveAttachBaseObject(attached.AttachedTo.gameObject);
                    }
                }
                else if (item.GetComponent<IdentifiableScript>().HasIdentifier(Identifier.AttachBase))
                {
                    attachBase = item.GetComponent<AttachScript>();

                    if (attachBase != null)
                    {
                        foreach (KeyValuePair<Transform, GameObject> kvp in attachBase.Attached)
                        {
                            RemoveAttachedObject(kvp.Value);
                        }
                    }
                }*/
                
                return;
            }
        }
    }

    /*private void RemoveAttachBaseObject(GameObject item)
    {
        AttachScript attachBase = null;

        foreach (KeyValuePair<Identifier, GameObject> p in loadedComponents)
        {
            if (p.Value == item)
            {
                pendingComponents.Add(p.Key);
                loadedComponents.Remove(p.Key);
                p.Value.GetComponent<IdentifiableScript>().RemoveIdentifier(Identifier.InBuildZone);

                if (item.GetComponent<IdentifiableScript>().HasIdentifier(Identifier.AttachBase))
                {
                    attachBase = item.GetComponent<AttachScript>();

                    if (attachBase != null)
                    {
                        foreach (KeyValuePair<Transform, GameObject> kvp in attachBase.Attached)
                        {
                            RemoveAttachedObject(kvp.Value);
                        }
                    }
                }

                return;
            }
        }
    }

    private void RemoveAttachedObject(GameObject item)
    {
        if (CheckIsLoaded(item))
        {
            foreach (KeyValuePair<Identifier, GameObject> p in loadedComponents)
            {
                if (p.Value == item)
                {
                    pendingComponents.Add(p.Key);
                    loadedComponents.Remove(p.Key);
                    p.Value.GetComponent<IdentifiableScript>().RemoveIdentifier(Identifier.InBuildZone);

                    return;
                }
            }
        }
    }*/

    private void Build()
    {
        DestroyComponentObjects();
        SpawnBuiltObject();
    }

    private void DestroyComponentObjects()
    {
        List<GameObject> items = new List<GameObject>();
        List<Identifier> keys = new List<Identifier>();

        foreach (KeyValuePair<Identifier, GameObject> p in loadedComponents)
        {
            items.Add(p.Value);
            keys.Add(p.Key);
        }

        Debug.Log("Loaded objects to destroy.");

        do
        {
            Debug.Log("Destroying " + items[0]);

            loadedComponents.Remove(keys[0]);

            //Destroy(items[0].GetComponent<AttachScript>());
            //Destroy(items[0].GetComponent<AttachableScript>());
            Destroy(items[0].GetComponent<MovableScript>());
            Destroy(items[0].GetComponent<IdentifiableScript>());
            Destroy(items[0]);

            items.Remove(items[0]);
            keys.Remove(keys[0]);

            Debug.Log("Item destroyed");
        } while (items.Count > 0);
    }

    private void SpawnBuiltObject()
    {
        GameObject spawning = Instantiate(finalObject);
        CentreInBuildZone(spawning);
    }

    private void CentreInBuildZone(GameObject item)
    {
        if (buildPoint != null)
        {
            item.transform.position = buildPoint.position;
            item.transform.rotation = buildPoint.rotation;
        }
    }
}
