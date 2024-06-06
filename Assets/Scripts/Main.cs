using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Unity.VisualScripting;

public class Main : MonoBehaviour
{
    public Material modelViewMat;
    public Button SelectFolderBtn;
    public Text PathTxt;
    public InputField SearchText;
    public Button SearchBtn;
    public Button HideBtn;
    public Button SaveBtn;
    [NonSerialized]
    public List<MeshFileItem> meshFileList = new List<MeshFileItem>();
    public GameObject meshFileItemTemplate;
    public Transform content;
    public List<HideMeshObjItem> hideMeshList = new List<HideMeshObjItem>();
    public GameObject hideMeshItemTemplate;
    public Transform hideViewContent;

    // [NonSerialized] public Dictionary<string, List<MeshReader.MeshData>> MeshDict = new Dictionary<string, List<MeshReader.MeshData>>();

    [NonSerialized] 
    public List<MeshReader.MeshData> CurrentMeshData = new List<MeshReader.MeshData>();

    public string CurrenFileName;
    public string CurrentFileFullPath;

    [NonSerialized] public GameObject ModelRoot;
    [NonSerialized] public GameObject CurSelectPartOfMesh;
    
    // Start is called before the first frame update
    void Start()
    {
        SelectFolderBtn.onClick.RemoveAllListeners();
        SelectFolderBtn.onClick.AddListener(OnSelectFolderBtnClicked);
        SearchBtn.onClick.RemoveAllListeners();
        SearchBtn.onClick.AddListener(OnSearchBtnClicked);
        HideBtn.onClick.RemoveAllListeners();
        HideBtn.onClick.AddListener(OnHideBtnClicked);
        SaveBtn.onClick.RemoveAllListeners();
        SaveBtn.onClick.AddListener(OnSaveBtnClicked);
        
        ModelRoot = new GameObject("ModelRoot");
    }

    void OnSelectFolderBtnClicked()
    {
        var path = OpenFile.OpenWinFile();
        PathTxt.text = path;
        if(!string.IsNullOrEmpty(path))
            RefreshFileListFromDirectory(path);
    }

    void OnSearchBtnClicked()
    {
        if (SearchText != null)
        {
            var searchVal = SearchText.text;
            if (string.IsNullOrEmpty(searchVal))
            {
                RefreshSearchResult(null);
            }
            else
            {
                List<MeshFileItem> resultList =
                    (from c in meshFileList where c.meshFileName.Contains(searchVal) select c).ToList();
                RefreshSearchResult(resultList);
            }
        }
    }

    void OnSaveBtnClicked()
    {
        if (CurrentMeshData != null)
        {
            var path = OpenFile.OpenWinFile();
            if (!string.IsNullOrEmpty(path))
                MeshReader.Instance.SaveMesh(path, CurrenFileName, CurrentFileFullPath, CurrentMeshData);
            // 
        }
    }

    void OnHideBtnClicked()
    {
        if (CurSelectPartOfMesh != null)
        {
            var obj = CurSelectPartOfMesh.GetComponent<MeshObj>();
            obj.Hide();
            
            var item = Instantiate(hideMeshItemTemplate, hideViewContent);
            item.SetActive(true);
            var objItem = item.GetComponent<HideMeshObjItem>();
            objItem.Initialize(obj);
            hideMeshList.Add(objItem);
            
            SaveBtn.gameObject.SetActive(true);
        }
    }
    
    void RefreshSearchResult(List<MeshFileItem> resultList)
    {
        if (resultList == null)
        {
            foreach (var fileItem in meshFileList)
            {
                fileItem.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (var fileItem in meshFileList)
            {
                fileItem.gameObject.SetActive(false);
            }
            foreach (var r in resultList)
            {
                foreach (var fileItem in meshFileList)
                {
                    if (fileItem.meshFileFullPath == r.meshFileFullPath)
                    {
                        fileItem.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }
    
    void RefreshFileListFromDirectory(string dir)
    {
        foreach (var meshItem in meshFileList)
        {
            meshItem.DestroySelf();
        }
        meshFileList.Clear();
        DirectoryInfo dInfo = new DirectoryInfo(dir + "\\");
        FileInfo[] fileInfoArr = dInfo.GetFiles("*.xmesh", SearchOption.AllDirectories);
        for (int i = 0; i < fileInfoArr.Length; i++)
        {
            var item = Instantiate(meshFileItemTemplate, content);
            item.SetActive(true);
            var meshItem = item.GetComponent<MeshFileItem>();
            meshItem.Initialize(fileInfoArr[i].Name, fileInfoArr[i].FullName, this);
            meshFileList.Add(meshItem);
        }
    }

    void CreateModel(string fileName, string fileFullPath)
    {
        if (ModelRoot != null)
        {
            ModelRoot.name = "Model_" + fileName;
        }
        for (int i = 0; i < CurrentMeshData.Count; i++)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = CurrentMeshData[i].verticles;
            int[] triangles = new int[CurrentMeshData[i].triangles.Count];
            for (int j = 0; j < CurrentMeshData[i].triangles.Count; j++)
            {
                triangles[j] = CurrentMeshData[i].triangles[j];
            }
        
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            // mesh.normals = meshs[i].normals;
            
            // mesh.uv = meshs[i].uvs;

            int vertCount = CurrentMeshData[i].verticles.Length;
            int uvCount = CurrentMeshData[i].uvs.Count / vertCount;
            List<Vector2[]> uvList = new List<Vector2[]>();
            for (int j = 0; j < uvCount; j++)
            {
                Vector2[] uv = new Vector2[CurrentMeshData[i].verticles.Length];
                for (int k = j * vertCount; k < vertCount * j + vertCount; k++)
                {
                    uv[k - j * vertCount] = CurrentMeshData[i].uvs[k];
                }
                if(j == 0)
                    mesh.uv = uv;
                else if(j == 1)
                    mesh.uv2 = uv;
                else if(j == 2)
                    mesh.uv3 = uv;
                else if(j == 3)
                    mesh.uv4 = uv;
            }
        
            GameObject go = new GameObject("Mesh_" + i);
            go.transform.parent = ModelRoot.transform;
            var meshF = go.AddComponent<MeshFilter>();
            meshF.mesh = mesh;
            var meshR = go.AddComponent<MeshRenderer>();
            meshR.material = new Material(modelViewMat);
            go.AddComponent<MeshCollider>();
            var obj = go.AddComponent<MeshObj>();
            obj.Initialize(CurrentMeshData[i]);
        }
    }

    public void OpenMesh(string fileName, string fileFullPath)
    {
        CurrentMeshData.Clear();
        var meshs = MeshReader.Instance.ReadMesh(fileFullPath);
        CurrentMeshData.AddRange(meshs);

        GameObject.Destroy(ModelRoot);
        ModelRoot = new GameObject("ModelRoot");
        CreateModel(fileName, fileFullPath);
        CurrenFileName = fileName;
        CurrentFileFullPath = fileFullPath;
        
        SaveBtn.gameObject.SetActive(false);

        foreach (var hideMeshItem in hideMeshList)
        {
            hideMeshItem.meshObj.meshData.isHide = false;
            hideMeshItem.DestroySelf();
        }
        
        hideMeshList.Clear();
    }

    public void SelectPartOfMesh(GameObject mesh)
    {
        if (CurSelectPartOfMesh != null)
        {
            var preRdr = CurSelectPartOfMesh.GetComponent<MeshRenderer>();
            var obj = CurSelectPartOfMesh.GetComponent<MeshObj>();
            preRdr.material.color = Color.white;
        }
        
        var rdr = mesh.GetComponent<MeshRenderer>();
        rdr.material.color = Color.green;
        HideBtn.gameObject.SetActive(true);
        CurSelectPartOfMesh = mesh;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
