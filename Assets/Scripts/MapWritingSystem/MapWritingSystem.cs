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
            Debug.Log(1);
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
        }else if (position.y - 1 == -0.5f)
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
        // if (gameObject.name == "NoParent")
        // {
        //     transforObject = gameObject.transform.parent;
        // }
        // else if (gameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Decor)
        // {
        //     transforObject = _objectDetect.transform;
        // }

        if (!_matrixMap.ContainsVertexByPox(transformObject.position, out _))
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
                _matrixMap.AddVertex(new TerritroyReaded(transformObject)
                {
                    TerritoryInfo = gameObject.GetComponent<TerritoryInfo>().Type,
                    ShelterType = gameObject.GetComponent<TerritoryInfo>().ShelterType,
                    PathPrefab = gameObject.GetComponent<TerritoryInfo>().Path
                }, _matrixMap.Vertex);
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