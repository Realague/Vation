using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdatableData {

	public Color[] baseColours;
	[Range(0, 1)]
	public float[] baseStartHeights;

	float savedMinHeight;
	float savedMaxHeight;

	public void ApplyToMaterial(Material material) {

		material.SetInt("baseColourCount", baseColours.Length);
		material.SetColorArray("baseColours", baseColours);
		material.SetFloatArray("baseStartHeights", baseStartHeights);

		UpdateMeshHeight(material, savedMinHeight, savedMaxHeight);
	}

	public void UpdateMeshHeight(Material material, float minHeight, float maxHeight) {
		savedMaxHeight = maxHeight;
		savedMinHeight = minHeight;

		material.SetFloat("minHeight", minHeight);
		material.SetFloat("maxHeight", maxHeight);
	}
}
