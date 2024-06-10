using UnityEngine;
public class MaterialTiling : MonoBehaviour
    {
        MeshRenderer mr;
        private Material mat;
        // Start is called before the first frame update
        void Start()
        {
            mr = GetComponent<MeshRenderer>();
            mat = mr.material;
     
            mat.mainTextureScale = new Vector2(this.transform.localScale.x * mr.material.mainTextureScale.x, this.transform.localScale.z * mr.material.mainTextureScale.y);
            mr.material = mat;
        }
     
    }
//Code Grabbed from https://forum.unity.com/threads/adjusting-material-tiling-to-each-different-object.151395/