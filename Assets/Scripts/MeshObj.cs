using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshObj : MonoBehaviour
{
    [NonSerialized] 
    public MeshReader.MeshData meshData;

    [NonSerialized] public MeshRenderer rdr;
    [NonSerialized] public MeshCollider collider;

    public void Initialize(MeshReader.MeshData meshData)
    {
        this.meshData = meshData;
        rdr = GetComponent<MeshRenderer>();
        collider = GetComponent<MeshCollider>();
    }

    public void Hide()
    {
        this.meshData.isHide = true;
        rdr.enabled = false;
        collider.enabled = false;
    }

    public void Show()
    {
        this.meshData.isHide = false;
        rdr.enabled = true;
        collider.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
