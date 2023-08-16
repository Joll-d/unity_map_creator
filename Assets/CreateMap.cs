using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateMap : MonoBehaviour
{
    private (float, float) _BorderX;
    private (float, float) _BorderY;
    private (float, float) _BorderZ;

    private float _mapHorizontalSize;
    private float _mapVerticalSize;

    public TMP_InputField mapSizeXZ;
    public TMP_InputField mapSizeY;

    public void OnCreateMapButtonClicked()
    {
        // Парсинг значений из полей ввода и присвоение их переменным
        if (float.TryParse(mapSizeXZ.text, out _mapHorizontalSize) && float.TryParse(mapSizeY.text, out _mapVerticalSize))
        {
            Debug.Log("Map Horizontal Size: " + _mapHorizontalSize);
            Debug.Log("Map Vertical Size: " + _mapVerticalSize);

            this._BorderX = (-_mapHorizontalSize/2, _mapHorizontalSize/2);
            this._BorderY = (0, _mapVerticalSize);
            this._BorderZ = (-_mapHorizontalSize/2, _mapHorizontalSize/2);

            CreateGround();

        }
        else
        {
            Debug.LogError("Invalid input values");
        }
    }


    public GameObject ground;

    public void CreateGround()
    {
        (float minBorderX, float maxBorderX) = this._BorderX;
        (float minBorderZ, float maxBorderZ) = this._BorderX;

        Vector3 corner1 = new Vector3(minBorderX, -0.5f, minBorderZ);
        Vector3 corner2 = new Vector3(maxBorderX, 0.5f, maxBorderZ);

        Vector3 size = new Vector3(Mathf.Abs(corner1.x - corner2.x), 1, Mathf.Abs(corner1.z - corner2.z));

        ground.transform.localScale = size;

        _SaveGroundInfo();
    }

    private void _SaveGroundInfo(){

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
