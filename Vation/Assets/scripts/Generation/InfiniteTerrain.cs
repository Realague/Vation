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
	int chunkSize;
	int chunksVisibeInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionnary = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> terrainChunkVisibleLastUpdate  = new List<TerrainChunk>();

	void Start() {
		mapGenerator = FindObjectOfType<MapGenerator>();

		maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
		chunkSize = mapGenerator.mapChunkSize - 1;
		chunksVisibeInViewDst = Mathf.RoundToInt(maxViewDistance / chunkSize);

		UpdateVisibleChunks();
	}

	void Update() {
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrtViewerMoveTresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}

	void UpdateVisibleChunks() {

		for (int i = 0; i < terrainChunkVisibleLastUpdate.Count; i++) {
			terrainChunkVisibleLastUpdate[i].SetVisible(false);
		}
		terrainChunkVisibleLastUpdate.Clear();

		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

		for (int yOffset = -chunksVisibeInViewDst; yOffset <= chunksVisibeInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibeInViewDst; xOffset <= chunksVisibeInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionnary.ContainsKey(viewedChunkCoord)) {
					terrainChunkDictionnary[viewedChunkCoord].UpdateTerrainChunk();
				} else {
					terrainChunkDictionnary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
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
		MeshCollider meshCollider;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;
		LODMesh collisionLODMesh;

		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
			this.detailLevels = detailLevels;

			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			meshObject = new GameObject("TerrainChunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshCollider = meshObject.AddComponent<MeshCollider>();
			meshRenderer.material = material;
			meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
			meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
			meshObject.transform.parent = parent;
			SetVisible(false);

			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
				if (detailLevels[i].useFullCollider) {
					collisionLODMesh = lodMeshes[i];
				}
			}

			mapGenerator.RequestMapData(position, OnMapDataReceived);
		}

		void OnMapDataReceived(MapData mapData) {
			this.mapData = mapData;
			mapDataReceived = true;

			UpdateTerrainChunk();
		}

		public void UpdateTerrainChunk() {
			if (mapDataReceived) {
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDistance;

				if (visible) {
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length - 1; i++) {
						if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
							lodIndex = i + 1;
						} else {
							break;
						}
					}

					if (lodIndex != previousLODIndex) {
						LODMesh lodMesh = lodMeshes[lodIndex];
						if (lodMesh.hasMesh) {
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						} else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh(mapData);
						}
					}

					if (lodIndex == 0) {
						if (collisionLODMesh.hasMesh) {
							meshCollider.sharedMesh = collisionLODMesh.mesh;
						} else if (!collisionLODMesh.hasRequestedMesh) {
							collisionLODMesh.RequestMesh(mapData);
						}
					}

					terrainChunkVisibleLastUpdate.Add(this);
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
		public float visibleDstThreshold;
		public bool useFullCollider;
	}

}
