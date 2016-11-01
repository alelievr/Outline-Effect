using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer))]
public class Outline : MonoBehaviour
{
    public bool     fullSpriteGlow = false;

    [Space]
	public Color    color = Color.white;
    public float    lineThickness = 2f;
    [RangeAttribute(0f, 1f)]
    public float    lineIntensity = 1f;
    [RangeAttribute(0f, 1f)]
    public float    alphaCutoff = .5f;
    public bool     pixelSnap = false;

    [Space]
    public bool     flipY = false;
    public bool     allowOutlineOverlap = true;
    public bool     autoColor = false;

    [Space]
    public bool     autoOutline = true;
    public bool     lastLinkedToFirst = true;
    public bool     outlineBezier = true;

    public List< OutlineVertice > outlineVertices = new List< OutlineVertice >();
    
    OutlineEffect   outlineEffect;

	int             originalLayer;
    int             outlineLayer;
	Material        originalMaterial;
    Material        outlineMaterial;

    new Renderer renderer;

    Material CreateNewOutlineMaterial(Color emissionColor)
    {
        Material m = new Material(Shader.Find("Hidden/OutlineBufferEffect"));
        m.SetColor("_Color", emissionColor);
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
        return m;
    }

	void Start()
    {
        outlineLayer = LayerMask.NameToLayer("Outline");
        renderer = GetComponent< Renderer >();
        outlineMaterial = CreateNewOutlineMaterial(color);
    }

    void OnEnable()
    {
		if(outlineEffect == null)
			outlineEffect = Camera.main.GetComponent<OutlineEffect>();
		outlineEffect.AddOutline(this);
    }

    void OnDisable()
    {
        outlineEffect.RemoveOutline(this);
    }

    public void PreRender()
    {
        //update material datas:
        originalMaterial = renderer.sharedMaterial;
        originalLayer = gameObject.layer;

        // if(eraseRenderer)
        //     renderer.material = outlineEraseMaterial;
        // else
        renderer.material = outlineMaterial;
        //to do better
        renderer.sharedMaterial.SetColor("_Color", color);
        renderer.sharedMaterial.SetFloat("_AlphaCutoff", alphaCutoff);
        renderer.sharedMaterial.SetFloat("_LineThickness", lineThickness);
        renderer.sharedMaterial.SetFloat("_LineIntensity", lineIntensity);
        renderer.sharedMaterial.SetInt("_PixelSnap", pixelSnap ? 1 : 0);
        renderer.sharedMaterial.SetInt("_FullSprite", fullSpriteGlow ? 1 : 0);
        renderer.sharedMaterial.SetInt("_FlipY", flipY ? 1 : 0);
        renderer.sharedMaterial.SetInt("_AllowOutlineOverlap", allowOutlineOverlap ? 1 : 0);
        renderer.sharedMaterial.SetInt("_AutoColor", autoColor ? 1 : 0);

        if (renderer is MeshRenderer)
            renderer.sharedMaterial.mainTexture = originalMaterial.mainTexture;

        gameObject.layer = outlineLayer;
    }

    public void PostRender()
    {
        if (renderer is MeshRenderer)
            renderer.sharedMaterial.mainTexture = null;

        renderer.material = originalMaterial;
        gameObject.layer = originalLayer;
    }

    [System.Serializable]
    public class OutlineVertice
    {
        public Vector3 position;
        public Vector3 t1;
        public Vector3 t2;

        public OutlineVertice(Vector3 pos)
        {
            position = pos;
        }
        
        public OutlineVertice(Vector3 pos, Vector3 t1, Vector3 t2)
        {
            position = pos;
            this.t1 = t1;
            this.t2 = t2;
        }
    }
}
