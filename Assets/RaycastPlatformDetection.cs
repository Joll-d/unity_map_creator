using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastInteraction : MonoBehaviour
{

    private Vector3 _hitPoint = Vector3.negativeInfinity; 

    public Vector3 hitPoint { get => _hitPoint;}

    private static RaycastInteraction _instance;
    public static RaycastInteraction Instance => _instance;

    void Update()
    {
        GetMousePositionOnGround();
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public Vector3 GetMousePositionOnGround(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Проводим рейкаст и проверяем столкновение с объектами
        if (Physics.Raycast(ray, out hit))
        {
            // Получаем координаты точки столкновения
            Vector3 hitPoint = hit.point;

            return hitPoint;
        }

        return Vector3.negativeInfinity;
    }

}
