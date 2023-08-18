using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfo : MonoBehaviour
{

    public GameObject item;
    public Image imageComponent;

    public void OnItemButtonClicked(){
        ItemCreator.Instance.item = item;
        ItemCreator.Instance.isCreating = true;
    }

}
