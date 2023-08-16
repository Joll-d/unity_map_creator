using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject _itemParent;

    private GameObject _item;
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

                Renderer[] renderers = _itemPreview.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = renderer.materials; // Получаем массив материалов из Renderer'а
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material newMaterial = new Material(materials[i]);
                        newMaterial.color = new Color(78 / 255f, 204 / 255f, 255 / 255f, 0.5f); // Устанавливаем цвет с полупрозрачным синим
                        newMaterial.SetFloat("_Mode", 2); // Устанавливаем режим материала на Transparent
                        newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        newMaterial.SetInt("_ZWrite", 0);
                        newMaterial.DisableKeyword("_ALPHATEST_ON");
                        newMaterial.EnableKeyword("_ALPHABLEND_ON");
                        newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        newMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                        materials[i] = newMaterial; // Заменяем материал в массиве новым материалом
                    }
                    renderer.materials = materials; // Присваиваем массив материалов обратно в Renderer
                }
            }

            _item = value;
        }
    }

    private bool _isCreating = false;
    public bool isCreating { get => _isCreating; set => _isCreating = value; }

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
            _CreateItem();
            _isCreating = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _DeleteItemPreview();
            _isCreating = false;
            return;
        }

        Vector3 itemPosition = RaycastInteraction.Instance.hitPoint;

        _MoveItemPreview(_itemPreview, itemPosition);
    }

    private void _MoveItemPreview(GameObject itemPreview, Vector3 itemPosition){
        if (MapInfo.Instance.IsLocationWithinBorders(itemPosition))
        {

            float roundedX = Mathf.Round(itemPosition.x);
            float roundedZ = Mathf.Round(itemPosition.z);
            float roundedY = Mathf.Round(itemPosition.y);

            itemPosition = new Vector3(roundedX, roundedY, roundedZ);

            itemPreview.transform.position = itemPosition;
        }
    }

    private void _CreateItem(){
        _item = _itemPreview;
        _itemPreview = null;


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

}
