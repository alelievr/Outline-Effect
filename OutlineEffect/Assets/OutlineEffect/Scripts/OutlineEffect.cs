/*
//  Copyright (c) 2015 JosÃ© Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
*/

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
        UpdateMaterialsPublicProperties();

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
		    UpdateMaterialsPublicProperties();
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

    private void UpdateMaterialsPublicProperties()
    {
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
