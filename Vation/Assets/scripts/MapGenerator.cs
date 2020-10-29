using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, Mesh, FalloffMap}

	public TerrainDatas terrainData;
	public NoiseDatas noiseData;	
	public TextureData textureData;	

	[SerializeField]
	private Material terrainMaterial;

	[SerializeField]
	[Range(0,MeshGenerator.numSupportedChunkSizes - 1)]
	private int chunkSizeIndex;
	[SerializeField]
	[Range(0,MeshGenerator.numSupportedFlatshadedChunkSizes - 1)]
	private int flatshadedChunkSizeIndex;
	[SerializeField]
	[Range(0,MeshGenerator.numSupportedLODs - 1)]
	public int editorPreviewLOD;	

	[SerializeField]
	private DrawMode drawMode;


	public bool autoUpdate;

	float[,] falloffMap;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	void Awake() {
		textureData.ApplyToMaterial(terrainMaterial);
		textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
	}

	void OnValuesUpdated() {
		if (!Application.isPlaying) {
			DrawMapInEditor();
		}
	}

	void OnTextureValuesUpdated() {
		textureData.ApplyToMaterial(terrainMaterial);
	}

	public int mapChunkSize {
		get {
			if (terrainData.useFlatShading) {
				return MeshGenerator.supportedFlatshadedChunkSizes[flatshadedChunkSizeIndex] - 1;
			} else {
				return MeshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
			}
		}
	}

	public void DrawMapInEditor() {
		textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
		MapData mapData = GenerateMapData(Vector2.zero);

		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading));
		} else if (drawMode == DrawMode.FalloffMap) {
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
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier,
															terrainData.meshHeightCurve, lod, terrainData.useFlatShading);
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
		float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.noiseScale, noiseData.seed, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizedMode);

		if (terrainData.useFalloffMap) {

			if (falloffMap == null) {
				falloffMap = FallOffGenerator.GenerateFalloffMap(mapChunkSize + 2);
			}

			for (int y = 0; y < mapChunkSize + 2; y++) {
				for (int x = 0; x < mapChunkSize + 2; x++) {
					if (terrainData.useFalloffMap) {
						noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
					}
				}
			}
		}
		
		return new MapData(noiseMap);
	}

	void OnValidate() {
		if (terrainData != null) {
			terrainData.OnValuesUpdated -= OnValuesUpdated;
			terrainData.OnValuesUpdated += OnValuesUpdated;
		} 
		if (noiseData != null) {
			noiseData.OnValuesUpdated -= OnValuesUpdated;
			noiseData.OnValuesUpdated += OnValuesUpdated;
		}
		if (textureData != null) {
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}
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

public struct MapData {
	public readonly float[,] heightMap;

	public MapData(float[,] heightMap) {
		this.heightMap = heightMap;
	}
}
