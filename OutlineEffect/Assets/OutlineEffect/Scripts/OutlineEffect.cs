using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class OutlineEffect : MonoBehaviour
{
	List<Outline> outlines = new List<Outline>();

    public Camera sourceCamera;

    Material        outlineShaderMaterial;
    Camera          outlineCamera;
    RenderTexture   renderTexture;

    int     outlineLayer;

    void Start()
    {
        outlineLayer = LayerMask.NameToLayer("Outline");

        outlineShaderMaterial = new Material(Shader.Find("Hidden/OutlineEffect"));
        outlineShaderMaterial.hideFlags = HideFlags.HideAndDontSave;

        if (sourceCamera == null)
        {
            sourceCamera = GetComponent<Camera>();

            if (sourceCamera == null)
                sourceCamera = Camera.main;
        }

        if (outlineCamera == null)
        {
            GameObject cameraGameObject = new GameObject("Outline Camera");
            cameraGameObject.transform.parent = sourceCamera.transform;
            outlineCamera = cameraGameObject.AddComponent<Camera>();
        }

		renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
		UpdateOutlineCameraFromSource();
    }

    void OnDestroy()
    {
        renderTexture.Release();
        DestroyMaterials();
    }

    void OnPreCull()
    {
		if(renderTexture.width != sourceCamera.pixelWidth || renderTexture.height != sourceCamera.pixelHeight)
		{
			renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
			outlineCamera.targetTexture = renderTexture;
		}
		UpdateOutlineCameraFromSource();

		if (outlines != null)
			for (int i = 0; i < outlines.Count; i++)
                if (outlines[i] != null)
                    outlines[i].PreRender();

        outlineCamera.Render();

        if (outlines != null)
            for (int i = 0; i < outlines.Count; i++)
                if (outlines[i] != null)
                    outlines[i].PostRender();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        outlineShaderMaterial.SetTexture("_OutlineSource", renderTexture);
        Graphics.Blit(source, destination, outlineShaderMaterial);
    }

    private void DestroyMaterials()
    {
        DestroyImmediate(outlineShaderMaterial);
        outlineShaderMaterial = null;
    }

    void UpdateOutlineCameraFromSource()
    {
        outlineCamera.CopyFrom(sourceCamera);
        outlineCamera.renderingPath = RenderingPath.Forward;
        outlineCamera.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        outlineCamera.clearFlags = CameraClearFlags.SolidColor;
        outlineCamera.cullingMask = 1 << outlineLayer;
        outlineCamera.rect = new Rect(0, 0, 1, 1);
		outlineCamera.enabled = true;
		outlineCamera.targetTexture = renderTexture;
	}

    public void AddOutline(Outline outline)
    {
        if (!outlines.Contains(outline))
        {
			outlines.Add(outline);
        }
    }
    public void RemoveOutline(Outline outline)
	{
		outlines.Remove(outline);
    }

}
