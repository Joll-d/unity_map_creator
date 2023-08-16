using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfo : MonoBehaviour
{

    public GameObject item;

    public void OnItemButtonClicked(){
        ItemCreator.Instance.item = item;
        ItemCreator.Instance.isCreating = true;
    }

}
