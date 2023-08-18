using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapInfo : MonoBehaviour
{
    private (float, float) _BorderX;

    public (float, float) BorderX
    {
        get => _BorderX;
        set => _BorderX = value;
    }

    private (float, float) _BorderY;

    public (float, float) BorderY
    {
        get => _BorderY;
        set => _BorderY = value;
    }

    private (float, float) _BorderZ;

    public (float, float) BorderZ
    {
        get => _BorderZ;
        set => _BorderZ = value;
    }

    public bool IsLocationWithinBorders(Vector3 point)
    {
        (float minBorderX, float maxBorderX) = _BorderX;
        (float minBorderZ, float maxBorderZ) = _BorderZ;
        (float minBorderY, float maxBorderY) = _BorderY;

        if (point.x >= minBorderX && point.x <= maxBorderX &&
            point.z >= minBorderZ && point.z <= maxBorderZ &&
            point.y >= minBorderY && point.y <= maxBorderY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private float _mapHorizontalSize;
    private float _mapVerticalSize;

    public GameObject ground;

    public TMP_InputField mapSizeXZ;
    public TMP_InputField mapSizeY;

    private static MapInfo _instance;
    public static MapInfo Instance => _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    void Start()
    {
        MapWritingSystem.Instance.AddNewItem(ground);
        OnCreateMapButtonClicked();
    }

    public void OnCreateMapButtonClicked()
    {
        if (float.TryParse(mapSizeXZ.text, out _mapHorizontalSize) &&
            float.TryParse(mapSizeY.text, out _mapVerticalSize))
        {
            Debug.Log("Map Horizontal Size: " + _mapHorizontalSize);
            Debug.Log("Map Vertical Size: " + _mapVerticalSize);

            _BorderX = (-_mapHorizontalSize / 2, _mapHorizontalSize / 2);
            _BorderY = (0, _mapVerticalSize - 1);
            _BorderZ = (-_mapHorizontalSize / 2, _mapHorizontalSize / 2);

            MapWritingSystem.Instance.SetMapSize((int)_mapHorizontalSize, (int)_mapVerticalSize);
            CreateGround();
            MapWritingSystem.Instance.CreateAirMatrix();
        }
        else
        {
            Debug.LogError("Invalid input values");
        }
    }


    public void CreateGround()
    {
        (float minBorderX, float maxBorderX) = _BorderX;
        (float minBorderZ, float maxBorderZ) = _BorderX;

        Vector3 corner1 = new Vector3(minBorderX, -0.5f, minBorderZ);
        Vector3 corner2 = new Vector3(maxBorderX, 0.5f, maxBorderZ);

        Vector3 size = new Vector3(Mathf.Abs(corner1.x - corner2.x), 1, Mathf.Abs(corner1.z - corner2.z));

        ground.transform.localScale = size;

        _SaveGroundInfo();
    }

    private void _SaveGroundInfo()
    {
        MapWritingSystem.Instance.ChangeItemScale(ground);
        MapWritingSystem.Instance.SaveToJson();
    }
}