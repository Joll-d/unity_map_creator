using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject _itemParent;

    private GameObject _item;
    private Material[] _originalMaterials;
    private GameObject _itemPreview;
    public GameObject item
    {
        get => _item;
        set
        {
            if (_item == null && value != null)
            {
                _itemPreview = Instantiate(value, Vector3.zero, Quaternion.identity);
                _itemPreview.transform.SetParent(_itemParent.transform);

                Renderer renderer = _itemPreview.GetComponent<MeshRenderer>();
                _originalMaterials = renderer.materials;
                _SetMaterialTransparentProperties(renderer);

                Collider[] colliders = _itemPreview.GetComponentsInChildren<Collider>();
    
                foreach (Collider collider in colliders)
                {
                    collider.enabled = false;
                }
            }

            _item = value;
        }
    }

    private bool _isCreating = false;
    public bool isCreating { get => _isCreating; set => _isCreating = value; }

    private float _rotationSpeed = 90f;

    private static ItemCreator _instance;
    public static ItemCreator Instance => _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    void Update()
    {
        if (_isCreating)
        {
            _CreatingItem();
        }
    }

    private void _CreatingItem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isCreating = false;
            _CreateItem();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isCreating = false;
            _DeleteItemPreview();
            return;
        }

        Vector3 itemPosition = RaycastInteraction.Instance.hitPoint;

        _MoveItemPreview(_itemPreview, itemPosition);
        if (Input.GetKeyDown(KeyCode.Q)){
            _RotateItemPreview(_itemPreview, _rotationSpeed);
        }
        if (Input.GetKeyDown(KeyCode.E)){
            _RotateItemPreview(_itemPreview, -_rotationSpeed);
        }
    }

    private void _MoveItemPreview(GameObject itemPreview, Vector3 itemPosition){
        if (MapInfo.Instance.IsLocationWithinBorders(itemPosition))
        {

            float roundedX = Mathf.Round(itemPosition.x+0.5f)-0.5f;
            float roundedZ = Mathf.Round(itemPosition.z+0.5f)-0.5f;
            float roundedY = Mathf.Round(itemPosition.y);

            itemPosition = new Vector3(roundedX, roundedY, roundedZ);

            itemPreview.transform.position = itemPosition;
        }
    }

    private void _RotateItemPreview(GameObject itemPreview, float rotationAngle)
    {
        float itemRotationY = itemPreview.transform.rotation.eulerAngles.y;
        rotationAngle += itemRotationY;
        Quaternion rotation = Quaternion.Euler(0f, rotationAngle, 0f);
        itemPreview.transform.rotation = rotation;
    }

    private void _CreateItem(){
        _item = _itemPreview;
        _itemPreview = null;

        _RestoreOriginalMaterials();

        Collider[] colliders = _item.GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        _SaveItemIntoGraph();
        _item = null;
    }

    private void _SaveItemIntoGraph(){

    }

    private void _DeleteItemPreview(){
        Destroy(_itemPreview);
        _item = null;
        _itemPreview = null;
    }

    private void _RestoreOriginalMaterials(){
        _item.GetComponent<MeshRenderer>().materials =  _originalMaterials;
    }

    private void _SetMaterialTransparentProperties(Renderer renderer)
    {
        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            Material newMaterial = new Material(materials[i]);
            newMaterial.color = new Color(78 / 255f, 204 / 255f, 255 / 255f, 0.5f);
            newMaterial.SetFloat("_Mode", 2);
            newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            newMaterial.SetInt("_ZWrite", 0);
            newMaterial.DisableKeyword("_ALPHATEST_ON");
            newMaterial.EnableKeyword("_ALPHABLEND_ON");
            newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            newMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            materials[i] = newMaterial;
        }
        renderer.materials = materials;
    }

}
