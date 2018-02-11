using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColorMap, Mesh}

	[SerializeField]
	private DrawMode drawMode;

	public const int mapChunkSize = 241;
	[SerializeField]
	[Range(0,6)]
	public int levelOfDetail;
	[SerializeField]
	private float noiseScale;

	[SerializeField]
	private int octaves;
	[SerializeField]
	[Range(0, 1)]
	private float persistance;
	[SerializeField]
	private float lacunarity;
	[SerializeField]
	private int seed;
	[SerializeField]
	private float meshHeightMultiplier;
	[SerializeField]
	private Vector2 offset;
	[SerializeField]
	private AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	[SerializeField]
	private TerrainType[] regions;

	public void GenerateMap() {
		float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, seed,
                                                           octaves, persistance, lacunarity, offset);

		Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++) {
			for (int x = 0; x < mapChunkSize; x++) {
				float currentHeight = noiseMap[x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions[i].height) {
						colourMap[y * mapChunkSize + x] = regions[i].color;
						break;
					}
				}
			}
		}

		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
		} else if (drawMode == DrawMode.ColorMap) {
			display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
		}
	}

	void OnValidate() {
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}

}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public  Color color;
}
