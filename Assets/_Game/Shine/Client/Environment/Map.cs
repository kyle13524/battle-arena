using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    private GameObject backgroundObject;

    [SerializeField]
    private GameObject boundaryObject;
    public Bounds mapBounds;

    public GameObject[] spawnSpots;

    #region Singleton definition

    public static Map Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    #endregion

    void Start()
    {
        mapBounds = boundaryObject.GetComponent<BoxCollider2D>().bounds;
    }

    void LateUpdate()
    {
        Vector3 tempPos = backgroundObject.transform.position;
        tempPos.x = Camera.main.transform.position.x;
        backgroundObject.transform.position = tempPos;
    }

}
