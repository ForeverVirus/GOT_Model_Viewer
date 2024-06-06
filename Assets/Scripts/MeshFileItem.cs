using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeshFileItem : MonoBehaviour
{
    public Button openMeshBtn;
    public Text meshNameTxt;
    

    [NonSerialized] 
    public string meshFileFullPath;
    [NonSerialized] 
    public string meshFileName;

    [NonSerialized] 
    public Main mainMenu;

    public void Initialize(string fileName, string fileFullPath, Main mainMenu)
    {
        openMeshBtn.onClick.RemoveAllListeners();
        openMeshBtn.onClick.AddListener(OnOpenMeshBtnClicked);

        meshNameTxt.text = fileName;
        meshFileName = fileName;
        meshFileFullPath = fileFullPath;
        this.mainMenu = mainMenu;
    }

    void OnOpenMeshBtnClicked()
    {
        this.mainMenu.OpenMesh(meshFileName, meshFileFullPath);
    }
    
    public void DestroySelf()
    {
        openMeshBtn.onClick.RemoveAllListeners();
        GameObject.Destroy(gameObject);
    }
}
