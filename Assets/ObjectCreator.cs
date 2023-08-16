using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCreator : MonoBehaviour
{

    public GameObject createdObject;
    public float iterationInterval = 0.1f;

    void Update()
    {

    }

    // public IEnumerator CreateObject(){

    //     //create preview
    //     GameObject objectPreview = CreateObjectPreview();

    //     while(true){
    //         if (Input.GetMouseButtonDown(0)){
    //             CreateObject();
    //             break;
    //         }

    //         if (Input.GetKeyDown(KeyCode.Escape)){
    //             DeleteObjectPreview();
    //             break;
    //         }

    //         //take vector
    //         Vector3 locationCreatedObject = RaycastInteraction.Instance.hitPoint;
    //         Debug.Log("Соприкосновение с платформой в точке: " + locationCreatedObject);

    //         //move preview
    //         moveObjectPreview(objectPreview, locationCreatedObject);

    //         yield return new WaitForSeconds(iterationInterval);
    //     }
    // }

    // private GameObject CreateObjectPreview(){

    // }

    // private void DeleteObjectPreview(){

    // }

    // private void moveObjectPreview(GameObject objectPreview, Vector3 locationCreatedObject){

    // }

}
