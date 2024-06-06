using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideMeshObjItem : MonoBehaviour
{
    public Button showMeshBtn;
    public Text meshNameTxt;

    [NonSerialized] public MeshObj meshObj;
    
    public void Initialize(MeshObj obj)
    {
        showMeshBtn.onClick.RemoveAllListeners();
        showMeshBtn.onClick.AddListener(OnShowMeshBtnClicked);

        meshNameTxt.text = "Show " + obj.gameObject.name;
        meshObj = obj;
    }

    void OnShowMeshBtnClicked()
    {
        this.meshObj.Show();
        
        GameObject.Destroy(this.gameObject);
    }
    
    public void DestroySelf()
    {
        showMeshBtn.onClick.RemoveAllListeners();
        GameObject.Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
