using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColorMap, Mesh, FallofMap}

	[SerializeField]
	private DrawMode drawMode;
	[SerializeField]
	private PerlinNoise.NormalizedMode normalizedMode;

	public const int mapChunkSize = 241;
	[SerializeField]
	[Range(0,6)]
	public int editorPreviewLOD;
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
	private bool useFalloffMap;
	[SerializeField]
	private AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	[SerializeField]
	private TerrainType[] regions;

	float[,] fallofMap;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	void Awake() {
		fallofMap = FallOffGenerator.GenerateFalloffMap(mapChunkSize);
	}

	public void DrawMapInEditor() {
		MapData mapData = GenerateMapData(Vector2.zero);

		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
		} else if (drawMode == DrawMode.ColorMap) {
			display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
		} else if (drawMode == DrawMode.FallofMap) {
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallOffGenerator.GenerateFalloffMap(mapChunkSize)));
		}
	}

	void Update() {
		if (mapDataThreadInfoQueue.Count > 0) {
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callBack(threadInfo.parameter);
			}
		}

		if (meshDataThreadInfoQueue.Count > 0) {
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callBack(threadInfo.parameter);
			}
		}
	}

	public void RequestMapData(Vector2 center, Action<MapData> callBack) {
		ThreadStart threadStart = delegate {
			MapDataThread(center, callBack);
		};

		new Thread(threadStart).Start();
	}

	public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
    	ThreadStart threadStart = delegate {
        	MeshDataThread(mapData, lod, callback);
    	};

    	new Thread(threadStart).Start();
	}

	void MeshDataThread(MapData mapData, int lod, Action<MeshData> callBack) {
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
		lock (meshDataThreadInfoQueue) {
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
		}
	}

	void MapDataThread(Vector2 center, Action<MapData> callBack) {
		MapData mapData = GenerateMapData(center);
		lock(mapDataThreadInfoQueue) {
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callBack, mapData));
		}
	}

	MapData GenerateMapData(Vector2 center) {
		float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, seed,
                                                        octaves, persistance, lacunarity, center + offset, normalizedMode);

		Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++) {
			for (int x = 0; x < mapChunkSize; x++) {
				if (useFalloffMap) {
					noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallofMap[x, y]);
				}
				float currentHeight = noiseMap[x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight >= regions[i].height) {
						colourMap[y * mapChunkSize + x] = regions[i].color;
					} else {
						break;
					}
				}
			}
		}

		return new MapData(noiseMap, colourMap);
	}

	void OnValidate() {
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}

		fallofMap = FallOffGenerator.GenerateFalloffMap(mapChunkSize);
	}

	struct MapThreadInfo<T> {
		public readonly Action<T> callBack;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callBack, T parameter) {
			this.callBack = callBack;
			this.parameter = parameter;
		}
	}

}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color color;
}

public struct MapData {
	public readonly float[,] heightMap;
	public readonly Color[] colourMap;

	public MapData(float[,] heightMap, Color[] colourMap) {
		this.colourMap = colourMap;
		this.heightMap = heightMap;
	}
}
