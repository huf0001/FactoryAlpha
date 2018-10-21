﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildZoneScript : MonoBehaviour
{
    [SerializeField] private GameObject buildPoint;
    [SerializeField] private int buildZoneNumber;
    [SerializeField] private GameObject[] buildSchemaObjects;

    private List<BuildSchemaScript> schemas = new List<BuildSchemaScript>();

    [SerializeField] private AudioClip loadedSound;
    [SerializeField] private AudioClip builtSound;

    private BuildSchemaScript currentSchema;

    private AudioSource audioSource;

    // Use this for initialization
    void Start ()
    {
        audioSource = GetComponent<AudioSource>();

        int difficulty;

        switch (PlayerPrefs.GetString("difficulty"))
        {
            case "hard":
                difficulty = 1;
                break;
            case "medium":
                difficulty = 2;
                break;
            default:
                difficulty = 1;
                break;
        }

        for (int i = 0; i < buildSchemaObjects.Length; i++)
        {
            if (difficulty > i)
            {
                schemas.Add(buildSchemaObjects[i].GetComponent<BuildSchemaScript>());
            }
        }

        currentSchema = schemas[0];
        currentSchema.ActivateGhost();
	}

    public int BuildZoneNumber
    {
        get
        {
            return buildZoneNumber;
        }
    }

    private void OnTriggerStay(Collider other)
    {                
        if (other.gameObject.GetComponent<IdentifiableScript>() != null)
        {
            IdentifiableScript ids = other.gameObject.GetComponent<IdentifiableScript>();

            if (currentSchema != null && !ids.HasIdentifier(Identifier.PlayerMoving) && other.gameObject.tag != "Player")
            {
                if (currentSchema.BelongsToSchema(ids) && !currentSchema.IsLoaded(ids))
                {
                    other.gameObject.GetComponent<Rigidbody>().useGravity = false;
                    other.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    currentSchema.HandleValidObject(other.gameObject);
                    return;
                }

                EjectFromBuildPoint(other.gameObject);
            }
        }
    }

    private void EjectFromBuildPoint(GameObject item)
    {
        float increment = -0.1f;

        item.transform.position = Vector3.MoveTowards(item.transform.position, buildPoint.transform.position, increment);
    }

    public void ChangeCurrentSchema(BuildSchemaScript schema, GameObject builtObject, BuiltScript script)
    {
        DeleteSchema(schema);
        DestroyBuiltObject(builtObject, script);
        NextSchema();
    }

    private void DeleteSchema(BuildSchemaScript schema)
    {
        schemas.Remove(schema);
        Destroy(schema.gameObject);
        Destroy(schema);
    }

    private void DestroyBuiltObject(GameObject builtObject, BuiltScript script)
    {
        Destroy(script);
        Destroy(builtObject);
    }

    private void NextSchema()
    {
        if (schemas.Count > 0)
        {
            currentSchema = schemas[0];
            currentSchema.ActivateGhost();
        }
        else
        {
            currentSchema = null;
        }
    }

    public void PlayLoadedSound()
    {
        PlaySoundEffect(loadedSound);
    }

    public void PlayBuiltSound()
    {
        PlaySoundEffect(builtSound);
    }

    private void PlaySoundEffect(AudioClip sound)
    {
        audioSource.clip = sound;
        audioSource.Play();
    }
}