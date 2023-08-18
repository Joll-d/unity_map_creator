using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float movementSpeed = 5.0f; // Скорость движения камеры
    public float rotationSpeed = 2.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update вызывается каждый кадр
    void Update()
    {
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isShiftPressed)
        {
        // Получаем ввод от клавиатуры
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Вычисляем новую позицию камеры
        Vector3 newPosition = transform.position +
            transform.forward * verticalInput * movementSpeed * Time.deltaTime +
            transform.right * horizontalInput * movementSpeed * Time.deltaTime;

        // Применяем новую позицию камеры
        transform.position = newPosition;

        // Получаем ввод от мыши
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Поворачиваем камеру в соответствии с вводом мыши
        Vector3 newRotation = transform.eulerAngles + new Vector3(-mouseY, mouseX, 0) * rotationSpeed;
        transform.eulerAngles = newRotation;
        }
    }
}