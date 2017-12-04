using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
	public float depth;
	public int width;
	public int height;
	public float scale = 20f;
	public float offsetX;
	public float offsetY;

	public static TerrainGenerator instance = null;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (!instance.Equals(this)) {
			Destroy(this);
		}
	}

	void Start() {
		generate();
	}

	public void generate() {
		height = GameManager.instance.height;
		width = GameManager.instance.width;
		depth = GameManager.instance.depth;
		offsetX = Random.Range(0f, 9999f);
		offsetY = Random.Range(0f, 9999f);
		Terrain terrain = GetComponent<Terrain>();
		terrain.terrainData = GenerateTerrain(terrain.terrainData);
	}

	private TerrainData GenerateTerrain(TerrainData terrainData) {
		terrainData.heightmapResolution = width + 1;
		terrainData.size = new Vector3(width, depth, height);
		terrainData.SetHeights(0, 0, GenerateHeights());
		return terrainData;
	}

	private float[,] GenerateHeights() {
		float[,] heights = new float[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				heights[x, y] = CalculateHeight(x, y);
			}
		}

		return heights;
	}
	
	private float CalculateHeight(int x, int y) {
		float xCoord = (float)x / width * scale + offsetX;
		float yCoord = (float)y / height * scale + offsetY;

		return Mathf.PerlinNoise(xCoord, yCoord);
	}

}
