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
        Transform transformObject = voxel.transform;

        (float, float) borderX = (transformObject.position.x - 1 / 2,
            transformObject.position.x + 1 / 2);

        (float, float) borderY = (transformObject.position.y - 0.5f,
            transformObject.position.y + 1);

        (float, float) borderZ = (transformObject.position.z - 1 / 2,
            transformObject.position.z + 1 / 2);

        if (!FindAllAirItemsInZone(borderX, borderY, borderZ, out List<string> items)) return;

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

        (float, float) borderY = (transformObject.position.y,
            transformObject.position.y + itemSizeInfoObject.sizeY);

        (float, float) borderZ = (transformObject.position.z - itemSizeInfoObject.sizeZ / 2,
            transformObject.position.z + itemSizeInfoObject.sizeZ / 2);

        Debug.Log(FindAllAirItemsInZone(borderX, borderY, borderZ, out _));
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
                MakeConnections(_matrixMap.MakeFromVector3ToIndex(transformObject.position), items, out HashSet<string> IndexLeft, out HashSet<string> IndexRight,
                    out HashSet<string> IndexUp, out HashSet<string> IndexDown, out HashSet<string> IndexFront,
                    out HashSet<string> IndexBottom);

                RemoveAllDictionaryItemsByKey(items);

                TerritroyReaded newItem = _matrixMap.AddVertex(new TerritroyReaded(transformObject)
                {
                    TerritoryInfo = gameObject.GetComponent<TerritoryInfo>().Type,
                    ShelterType = gameObject.GetComponent<TerritoryInfo>().ShelterType,
                    PathPrefab = gameObject.GetComponent<TerritoryInfo>().Path
                }, _matrixMap.Vertex);
                foreach (var i in IndexLeft)
                {
                    Debug.Log(i);
                }
                HashSet<string>[] connections = VerifyExistenceOfConnection(IndexLeft, IndexRight, IndexUp, IndexDown, IndexFront, IndexBottom);
                foreach (var i in IndexLeft)
                {
                    Debug.Log(i);
                }
                newItem.IndexLeft = connections[0];
                newItem.IndexRight = connections[1];
                newItem.IndexUp = connections[2];
                newItem.IndexDown = connections[3];
                newItem.IndexFront = connections[4];
                newItem.IndexBottom = connections[5];
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

    public HashSet<string>[] VerifyExistenceOfConnection(HashSet<string> IndexLeft, HashSet<string> IndexRight,
        HashSet<string> IndexUp, HashSet<string> IndexDown, HashSet<string> IndexFront, HashSet<string> IndexBottom)
    {
        foreach (var leftIndex in IndexLeft)
        {
            if (_matrixMap._vertex.ContainsKey(leftIndex)) continue;
            IndexLeft.Remove(leftIndex);
        }

        foreach (var rightIndex in IndexRight)
        {
            if (_matrixMap._vertex.ContainsKey(rightIndex)) continue;
            IndexRight.Remove(rightIndex);
        }

        foreach (var upIndex in IndexUp)
        {
            if (_matrixMap._vertex.ContainsKey(upIndex)) continue;
            IndexUp.Remove(upIndex);
        }

        foreach (var downIndex in IndexDown)
        {
            if (_matrixMap._vertex.ContainsKey(downIndex)) continue;
            IndexDown.Remove(downIndex);
        }

        foreach (var frontIndex in IndexFront)
        {
            if (_matrixMap._vertex.ContainsKey(frontIndex)) continue;
            IndexFront.Remove(frontIndex);
        }

        foreach (var bottomIndex in IndexBottom)
        {
            if (_matrixMap._vertex.ContainsKey(bottomIndex)) continue;
            IndexBottom.Remove(bottomIndex);
        }

        HashSet<string>[] resList = new HashSet<string>[] {IndexLeft, IndexRight, IndexUp, IndexDown, IndexFront, IndexBottom};
        
        return resList;
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
            for (float y = YStartPosition; y <= maxBorderY - 0.1f; y += .5f)
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
            TerritroyReaded item = _matrixMap._vertex[key];
            foreach (var leftIndex in item.IndexLeft)
            {
                if (!_matrixMap._vertex[leftIndex].IndexRight.Contains(key)) continue;
                
                _matrixMap._vertex[leftIndex].IndexRight.Remove(key);
                
            }

            foreach (var rightIndex in item.IndexRight)
            {
                if (!_matrixMap._vertex[rightIndex].IndexLeft.Contains(key)) continue;
                _matrixMap._vertex[rightIndex].IndexLeft.Remove(key);
            }

            foreach (var upIndex in item.IndexUp)
            {
                if (!_matrixMap._vertex[upIndex].IndexDown.Contains(key)) continue;
                _matrixMap._vertex[upIndex].IndexDown.Remove(key);
            }

            foreach (var downIndex in item.IndexDown)
            {
                if (!_matrixMap._vertex[downIndex].IndexUp.Contains(key)) continue;
                _matrixMap._vertex[downIndex].IndexUp.Remove(key);
            }

            foreach (var frontIndex in item.IndexFront)
            {
                if (!_matrixMap._vertex[frontIndex].IndexBottom.Contains(key)) continue;
                _matrixMap._vertex[frontIndex].IndexBottom.Remove(key);
            }

            foreach (var bottomIndex in item.IndexBottom)
            {
                if (!_matrixMap._vertex[bottomIndex].IndexFront.Contains(key)) continue;
                _matrixMap._vertex[bottomIndex].IndexFront.Remove(key);
            }
            
            _matrixMap._vertex.Remove(key);
        }
    }

    public void MakeConnections(string itemIndex, List<string> keys, out HashSet<string> IndexLeft, out HashSet<string> IndexRight,
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
                _matrixMap._vertex[leftIndex].IndexRight.Remove(leftIndex);
                _matrixMap._vertex[leftIndex].IndexRight.Add(itemIndex);
            }

            foreach (var rightIndex in item.IndexRight)
            {
                if (IndexRight.Contains(rightIndex)) continue;
                if (keys.Contains(rightIndex)) continue;

                IndexRight.Add(rightIndex);
                _matrixMap._vertex[rightIndex].IndexLeft.Remove(rightIndex);
                _matrixMap._vertex[rightIndex].IndexLeft.Add(itemIndex);
            }

            foreach (var upIndex in item.IndexUp)
            {
                if (IndexUp.Contains(upIndex)) continue;
                if (keys.Contains(upIndex)) continue;

                IndexUp.Add(upIndex);
                _matrixMap._vertex[upIndex].IndexDown.Remove(upIndex);
                _matrixMap._vertex[upIndex].IndexDown.Add(itemIndex);
            }

            foreach (var downIndex in item.IndexDown)
            {
                if (IndexDown.Contains(downIndex)) continue;
                if (keys.Contains(downIndex)) continue;

                IndexDown.Add(downIndex);
                _matrixMap._vertex[downIndex].IndexUp.Remove(downIndex);
                _matrixMap._vertex[downIndex].IndexUp.Add(itemIndex);
            }

            foreach (var frontIndex in item.IndexFront)
            {
                if (IndexFront.Contains(frontIndex)) continue;
                if (keys.Contains(frontIndex)) continue;

                IndexFront.Add(frontIndex);
                _matrixMap._vertex[frontIndex].IndexBottom.Remove(frontIndex);
                _matrixMap._vertex[frontIndex].IndexBottom.Add(itemIndex);
            }

            foreach (var bottomIndex in item.IndexBottom)
            {
                if (IndexBottom.Contains(bottomIndex)) continue;
                if (keys.Contains(bottomIndex)) continue;

                IndexBottom.Add(bottomIndex);
                _matrixMap._vertex[bottomIndex].IndexFront.Remove(bottomIndex);
                _matrixMap._vertex[bottomIndex].IndexFront.Add(itemIndex);
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