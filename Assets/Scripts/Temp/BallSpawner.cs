using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float spawnRate;
    [Header("Prefab")]
    [SerializeField] private GameObject prefab;


    private float prevTime;
    private bool isSpawn;


    private void Spawn()
    {
        Instantiate(prefab, transform.position, Quaternion.identity, transform);
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            isSpawn = !isSpawn;
        }

        if (isSpawn && prevTime + 1 / spawnRate < Time.time)
        {
            Spawn();
            prevTime = Time.time;
        }
    }
}
