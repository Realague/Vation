using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	[SerializeField]
	private Canvas minimap;

	[SerializeField]
	private Canvas looseCanvas;

	[SerializeField]
	private Text timerText;

	private float startTime;

	private bool finish = false;

	[SerializeField]
	private float timer = 120f;

	[SerializeField]
	private GameObject playerPrefabs;

	[SerializeField]
	private GameObject endLevelPrefabs;

	private GameObject player;

	private GameObject endLevel;

	public static GameManager instance = null;

	public float depth = 10f;

	public int width = 256;
	public int height = 256;

	private int level = 0;
	private float mult = 0.1f;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (!instance.Equals(this)) {
			Destroy(this);
		}
		looseCanvas.enabled = false;
	}

	void Start() {
		startTime = Time.time;
		level++;
		Cursor.visible = false;
		depth = mult * (level * depth) + depth;
		player = Instantiate(playerPrefabs, new Vector3(Random.Range(0f, height), Random.Range(0f, width), depth * 5), new Quaternion(0f, 0f, 0f, 0f));
		endLevel = Instantiate(endLevelPrefabs, new Vector3(Random.Range(0f, height), Random.Range(0f, width), depth * 5), new Quaternion(0f, 0f, 0f, 0f));
		Minimap.playerTransform = player.transform;
	}

	void Update() {
		float t = timer - (Time.time - startTime);
		if (!finish) {
			string minutes = ((int) t / 60).ToString();
			string seconds = (t % 60).ToString("f0");

			timerText.text = "Temps restant : " + minutes + "m : " + seconds + "s";
		}
		if (t <= 0 && !finish) {
			Cursor.visible = true;
			loose();
			finish = true;
		}
	}

	public void loose() {
		minimap.enabled = false;
		looseCanvas.enabled = true;
	}

	public void loadNextLevel() {
		level++;
		depth = mult * (level * depth) + depth;
		player.transform.position = new Vector3(Random.Range(0f, height), Random.Range(0f, width), depth * 5);
		endLevel.transform.position = new Vector3(Random.Range(0f, height), Random.Range(0f, width), depth * 5);
		TerrainGenerator.instance.generate();
		Minimap.playerTransform = player.transform;
	}

	public void restart() {
		SceneManager.LoadScene("Main");
	}

}
