using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;


public class MapWritingSystem : MonoBehaviour
{
    private MatrixMap _matrixMap;

    [SerializeField] private string _fileName = "/TEST.json";

    public static MapWritingSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _matrixMap = new MatrixMap();
    }

    public void CreateAirMatrix()
    {
        (float minBorderX, float maxBorderX) = MapInfo.Instance.BorderX;
        (float minBorderZ, float maxBorderZ) = MapInfo.Instance.BorderZ;
        (float minBorderY, float maxBorderY) = MapInfo.Instance.BorderY;

        float XScale = 0.5f;
        float YScale = 0.5f;
        float ZScale = 0.5f;

        float XRotation = 0.0f;
        float YRotation = 0.0f;
        float ZRotation = 0.0f;
        float WRotation = 1.0f;

        float XStartPosition = minBorderX + 0.5f;
        float YStartPosition = 0.5f;
        float ZStartPosition = minBorderZ + 0.5f;

        GameObject voxel = new GameObject("Voxel");
        voxel.transform.localScale = new Vector3(XScale, YScale, ZScale);
        voxel.transform.rotation = new Quaternion(XRotation, YRotation, ZRotation, WRotation);

        for (float x = XStartPosition; x <= maxBorderX; x += 1.0f)
        {
            for (float y = YStartPosition; y <= maxBorderY + 1; y += 1.0f)
            {
                for (float z = ZStartPosition; z <= maxBorderZ; z += 1.0f)
                {
                    voxel.transform.position = new Vector3(x, y, z);
                    AddAirTerritory(voxel);
                }
            }
        }
    }

    public void AddAirTerritory(GameObject voxel)
    {
        if (!_matrixMap.ContainsVertexByPox(voxel.transform.position, out TerritroyReaded item))
        {
            item = _matrixMap.AddVertex(new TerritroyReaded(voxel.transform)
            {
                TerritoryInfo = TerritoryType.Air,
                ShelterType = new ShelterInfo(),
            }, _matrixMap.Vertex);
        }

        Vector3 position = voxel.transform.position;
        Vector3 possiblePosition = new Vector3(position.x, position.y, position.z - 1);
        string index = _matrixMap.MakeFromVector3ToIndex(possiblePosition);

        if (_matrixMap._vertex.ContainsKey(index))
        {
            item.IndexLeft.Add(index);
            _matrixMap._vertex[index].IndexRight.Add(item.Index);
        }

        possiblePosition = new Vector3(position.x, position.y - 1, position.z);
        index = _matrixMap.MakeFromVector3ToIndex(possiblePosition);
        if (_matrixMap._vertex.ContainsKey(index) && position.y - 1 >= 0)
        {
            item.IndexDown.Add(index);
            _matrixMap._vertex[index].IndexUp.Add(item.Index);
        }
        else if (position.y - 1 == -0.5f)
        {
            item.IndexDown.Add("0_-0.5_0");
            _matrixMap._vertex["0_-0.5_0"].IndexUp.Add(item.Index);
        }

        possiblePosition = new Vector3(position.x - 1, position.y, position.z);
        index = _matrixMap.MakeFromVector3ToIndex(possiblePosition);
        if (_matrixMap._vertex.ContainsKey(index))
        {
            item.IndexBottom.Add(index);
            _matrixMap._vertex[index].IndexFront.Add(item.Index);
        }
    }

    public void SetMapSize(int width, int height)
    {
        _matrixMap.width = width;
        _matrixMap.height = height;
    }

    public void ChangeItemScale(GameObject gameObject)
    {
        Transform transformObject = gameObject.transform;

        if (!_matrixMap.ContainsVertexByPox(transformObject.position, out var item)) return;

        Vector3 scale = transformObject.localScale;
        item.XSize = scale.x;
        item.YSize = scale.y;
        item.ZSize = scale.z;
    }

    public void AddNewItem(GameObject gameObject)
    {
        Transform transformObject = gameObject.transform;
        ItemSizeInfo itemSizeInfoObject = gameObject.GetComponent<ItemSizeInfo>();

        (float, float) borderX = (transformObject.position.x - itemSizeInfoObject.sizeX / 2,
            transformObject.position.x + itemSizeInfoObject.sizeX / 2);

        (float, float) borderY = (transformObject.position.y - itemSizeInfoObject.sizeY / 2,
            transformObject.position.y + itemSizeInfoObject.sizeY / 2);

        (float, float) borderZ = (transformObject.position.z - itemSizeInfoObject.sizeZ / 2,
            transformObject.position.z + itemSizeInfoObject.sizeZ / 2);


        if (FindAllAirItemsInZone(borderX, borderY, borderZ, out List<string> items))
        {

            if (gameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Decor)
            {
                _matrixMap.AddVertex(new TerritroyReaded(transformObject)
                {
                    TerritoryInfo = TerritoryType.Air,
                    ShelterType = new ShelterInfo(),
                }, _matrixMap.Vertex);
                var decorItem = _matrixMap.AddVertex(new TerritroyReaded(transformObject)
                {
                    TerritoryInfo = TerritoryType.Decor,
                    PathPrefab = gameObject.GetComponent<TerritoryInfo>().Path
                }, _matrixMap.Decors);
                decorItem.SetNewPosition(gameObject.transform);
            }
            else
            {
                MakeConnections(items, out HashSet<string> IndexLeft, out HashSet<string> IndexRight,
                    out HashSet<string> IndexUp, out HashSet<string> IndexDown, out HashSet<string> IndexFront,
                    out HashSet<string> IndexBottom);

                RemoveAllDictionaryItemsByKey(items);

                TerritroyReaded newItem = _matrixMap.AddVertex(new TerritroyReaded(transformObject)
                {
                    TerritoryInfo = gameObject.GetComponent<TerritoryInfo>().Type,
                    ShelterType = gameObject.GetComponent<TerritoryInfo>().ShelterType,
                    PathPrefab = gameObject.GetComponent<TerritoryInfo>().Path
                }, _matrixMap.Vertex);

                newItem.IndexLeft = IndexLeft;
                newItem.IndexRight = IndexRight;
                newItem.IndexUp = IndexUp;
                newItem.IndexDown = IndexDown;
                newItem.IndexFront = IndexFront;
                newItem.IndexBottom = IndexBottom;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGround(GameObject gameObject)
    {
        Transform transformObject = gameObject.transform;
        if (!_matrixMap.ContainsVertexByPox(transformObject.position, out _))
        {
            _matrixMap.AddVertex(new TerritroyReaded(transformObject)
            {
                TerritoryInfo = gameObject.GetComponent<TerritoryInfo>().Type,
                ShelterType = gameObject.GetComponent<TerritoryInfo>().ShelterType,
                PathPrefab = gameObject.GetComponent<TerritoryInfo>().Path
            }, _matrixMap.Vertex);
        }
    }

    public bool FindAllAirItemsInZone((float, float) BorderX, (float, float) BorderY, (float, float) BorderZ,
        out List<string> items)
    {
        (float minBorderX, float maxBorderX) = BorderX;
        (float minBorderY, float maxBorderY) = BorderY;
        (float minBorderZ, float maxBorderZ) = BorderZ;

        float XStartPosition = minBorderX;
        float YStartPosition = minBorderY;
        float ZStartPosition = minBorderZ;

        items = new List<string>();
        for (float x = XStartPosition; x <= maxBorderX; x += .5f)
        {
            for (float y = YStartPosition; y <= maxBorderY + 1; y += .5f)
            {
                for (float z = ZStartPosition; z <= maxBorderZ; z += .5f)
                {
                    Vector3 possiblePosition = new Vector3(x, y, z);
                    string possibleIndex = _matrixMap.MakeFromVector3ToIndex(possiblePosition);
                    if (_matrixMap._vertex.ContainsKey(possibleIndex))
                    {
                        if (_matrixMap._vertex[possibleIndex].TerritoryInfo != TerritoryType.Air) return false;
                        items.Add(possibleIndex);
                    }
                }
            }
        }

        return true;
    }

    public void RemoveAllDictionaryItemsByKey(List<string> keys)
    {
        foreach (var key in keys)
        {
            Debug.Log(key);
            _matrixMap._vertex.Remove(key);
        }
    }

    public void MakeConnections(List<string> keys, out HashSet<string> IndexLeft, out HashSet<string> IndexRight,
        out HashSet<string> IndexUp, out HashSet<string> IndexDown, out HashSet<string> IndexFront, out HashSet<string> IndexBottom)
    {
        IndexLeft = new HashSet<string>();
        IndexRight = new HashSet<string>();
        IndexUp = new HashSet<string>();
        IndexDown = new HashSet<string>();
        IndexFront = new HashSet<string>();
        IndexBottom = new HashSet<string>();

        foreach (var key in keys)
        {
            TerritroyReaded item = _matrixMap._vertex[key];
            foreach (var leftIndex in item.IndexLeft)
            {
                if (IndexLeft.Contains(leftIndex)) continue;
                if (keys.Contains(leftIndex)) continue;

                IndexLeft.Add(leftIndex);
            }

            foreach (var rightIndex in item.IndexRight)
            {
                if (IndexRight.Contains(rightIndex)) continue;
                if (keys.Contains(rightIndex)) continue;

                IndexRight.Add(rightIndex);
            }

            foreach (var upIndex in item.IndexUp)
            {
                if (IndexUp.Contains(upIndex)) continue;
                if (keys.Contains(upIndex)) continue;

                IndexUp.Add(upIndex);
            }

            foreach (var downIndex in item.IndexDown)
            {
                if (IndexDown.Contains(downIndex)) continue;
                if (keys.Contains(downIndex)) continue;

                IndexDown.Add(downIndex);
            }

            foreach (var frontIndex in item.IndexFront)
            {
                if (IndexFront.Contains(frontIndex)) continue;
                if (keys.Contains(frontIndex)) continue;

                IndexFront.Add(frontIndex);
            }

            foreach (var bottomIndex in item.IndexBottom)
            {
                if (IndexBottom.Contains(bottomIndex)) continue;
                if (keys.Contains(bottomIndex)) continue;

                IndexBottom.Add(bottomIndex);
            }
        }
    }

    public void SaveToJson()
    {
        // _matrixMap.DebugToConsole();
        // Debug.Log(CultureInfo.CurrentCulture.Name);
        string jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(_matrixMap, Newtonsoft.Json.Formatting.Indented);
        Debug.Log(jsonText);

        string filePath = Application.dataPath + "/Resources" + _fileName;
        System.IO.File.WriteAllText(filePath, jsonText);
    }
}