using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InfiniteTerrain : MonoBehaviour {

	const float viewerMoveTresholdForChunkUpdate = 25f;
	const float sqrtViewerMoveTresholdForChunkUpdate = viewerMoveTresholdForChunkUpdate * viewerMoveTresholdForChunkUpdate;

	[SerializeField]
	private LODInfo[] detailLevels;
	[SerializeField]
	private static float maxViewDistance;
	[SerializeField]
	private Transform viewer;
	[SerializeField]
	Material mapMaterial;

	public static Vector2 viewerPosition;
	Vector2 viewerPositionOld;
	static MapGenerator mapGenerator;
	int chunckSize;
	int chunckVisibeInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionnary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunkVisibleLastUpdate  = new List<TerrainChunk>();

	void Start() {
		mapGenerator = FindObjectOfType<MapGenerator>();

		maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstTreshold;
		chunckSize = MapGenerator.mapChunkSize - 1;
		chunckVisibeInViewDst = Mathf.RoundToInt(maxViewDistance / chunckSize);

		UpdateVisibleChunk();
	}

	void Update() {
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrtViewerMoveTresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunk();
		}
	}

	void UpdateVisibleChunk() {

		for (int i = 0; i < terrainChunkVisibleLastUpdate.Count; i++) {
			terrainChunkVisibleLastUpdate[i].SetVisible(false);
		}
		terrainChunkVisibleLastUpdate.Clear();

		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunckSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunckSize);

		for (int yOffset = -chunckVisibeInViewDst; yOffset <= chunckVisibeInViewDst; yOffset++) {
			for (int xOffset = -chunckVisibeInViewDst; xOffset <= chunckVisibeInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionnary.ContainsKey(viewedChunkCoord)) {
					terrainChunkDictionnary[viewedChunkCoord].UpdateTerrainChunk();
					if (terrainChunkDictionnary[viewedChunkCoord].IsVisible()) {
						terrainChunkVisibleLastUpdate.Add(terrainChunkDictionnary[viewedChunkCoord]);
					}
				} else {
					terrainChunkDictionnary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunckSize, detailLevels, transform, mapMaterial));
				}
			}
		}
	}

	public class TerrainChunk {

		GameObject meshObject;
		Vector2 position;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;

		MapData mapData;
		bool mapDataRecieved;
		int previousLODIndex = -1;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
			this.detailLevels = detailLevels;

			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			meshObject = new GameObject("TerrainChunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;
			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			SetVisible(false);

			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}
			mapGenerator.RequestMapData(position, OnMapDataReceived);
		}

		void OnMapDataReceived(MapData mapData) {
			this.mapData = mapData;
			mapDataRecieved = true;

			Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateTerrainChunk();
		}

		public void UpdateTerrainChunk() {
			if (mapDataRecieved) {
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDistance;

				if (visible) {
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length - 1; i++) {
						if (viewerDstFromNearestEdge > detailLevels[i].lod) {
							lodIndex = i + 1;
						} else {
							break;
						}
					}

					if (lodIndex != previousLODIndex) {
						LODMesh lodMesh = lodMeshes[lodIndex];
						if (lodMesh.hasMesh) {
							meshFilter.mesh = lodMesh.mesh;
						} else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh(mapData);
						}
					}
				}

				SetVisible(visible);
			}
		}

		public void SetVisible(bool visible) {
			meshObject.SetActive(visible);
		}

		public bool IsVisible() {
			return meshObject.activeSelf;
		}

	}

	class LODMesh {
		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		System.Action updateCallBack;

		public LODMesh(int lod, System.Action updateCallBack) {
			this.lod = lod;
			this.updateCallBack = updateCallBack;
		}

		void OnMeshDataReceived(MeshData meshData) {
			mesh = meshData.CreateMesh();
			hasMesh = true;

			updateCallBack();
		}

		public void RequestMesh(MapData mapData) {
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
		}

	}

	[System.Serializable]
	public struct LODInfo {
		public int lod;
		public float visibleDstTreshold;
	}

}
