using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor( typeof( Outline ) )]
public class OutlineEditor : Editor
{
	// draw lines between a chosen game object
	// and a selection of added game objects
	float radius = 1;
	
	void OnSceneGUI( )
	{
		// get the chosen game object
		Outline t = target as Outline;

		if( t == null )
			return;

		if (!t.autoOutline)
		{
			// grab the center of the parent
			Vector3 center = t.transform.position;
			for (int i = 0; i < t.outlineVertices.Count; i++)
			{
				if (i != t.outlineVertices.Count - 1)
					Handles.DrawLine(center + t.outlineVertices[i], center + t.outlineVertices[i + 1]);
				t.outlineVertices[i] = Handles.FreeMoveHandle(center + t.outlineVertices[i], Quaternion.identity, 0.01f, Vector3.zero, Handles.DotCap) - center;
			}
		}
	}
}
