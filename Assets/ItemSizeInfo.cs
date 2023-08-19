using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemSizeInfo : MonoBehaviour
{
    [FormerlySerializedAs("sizeX")] [SerializeField]
    private int _sizeX;
    public int sizeX{ get => _sizeX; set => _sizeX = value; }
    
    [FormerlySerializedAs("sizeY")] [SerializeField]
    private int _sizeY;
    public int sizeY{ get => _sizeY; set => _sizeY = value; }

    [FormerlySerializedAs("sizeZ")] [SerializeField]
    private int _sizeZ;
    public int sizeZ{ get => _sizeZ; set => _sizeZ = value; }
    
    [SerializeField]
    private float offsetX = 0f;
    [SerializeField]
    private float offsetY = 0f;
    [SerializeField]
    private float offsetZ = 0f;
    [SerializeField]
    private bool[,,] boolArray;

    // Опционально, можно добавить метод для инициализации массива
    public void InitializeBoolArray()
    {
        boolArray = new bool[sizeX, sizeY, sizeZ];

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    boolArray[x, y, z] = true;
                }
            }
        }
    }

    private void Awake()
    {
        InitializeBoolArray();
    }
}
