using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	[SerializeField]
	private int maxNumberTime = 6;

	[SerializeField]
	private int minNumberTime = 3;

	public int recoverTime = 5;

	public Canvas uiCanvas;

	[SerializeField]
	private Canvas looseCanvas;

	public Canvas loadingCanvas;

	[SerializeField]
	private Text levelText;

	[SerializeField]
	private Text timerText;

	private float startTime;

	private bool finish = false;

	public float timer = 120f;

	[SerializeField]
	private GameObject timePrefabs;

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
		uiCanvas.enabled = false;
		spawnTime();
		level++;
		levelText.text = "Level: " + level;
		Cursor.visible = false;
		depth = mult * (level * depth) + depth;
		player = Instantiate(playerPrefabs, new Vector3(Random.Range(0f, height), depth * 5, Random.Range(0f, width)), new Quaternion(0f, 0f, 0f, 0f));
		endLevel = Instantiate(endLevelPrefabs, new Vector3(Random.Range(0f, height), depth * 5, Random.Range(0f, width)), new Quaternion(0f, 0f, 0f, 0f));
		Minimap.playerTransform = player.transform;
		startTime = Time.time;
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
		uiCanvas.enabled = false;
		looseCanvas.enabled = true;
	}

	public void loadNextLevel() {
		uiCanvas.enabled = false;
		loadingCanvas.enabled = true;
		level++;
		minNumberTime += level / 5;
		maxNumberTime += level / 5;
		recoverTime += level / 5;
		spawnTime();
		levelText.text = "Level: " + level;
		startTime = Time.time;
		timer = 120f;
		depth = mult * (level * depth) + depth;
		player.transform.position = new Vector3(Random.Range(0f, height), depth * 5, Random.Range(0f, width));
		endLevel.transform.position = new Vector3(Random.Range(0f, height), depth * 5, Random.Range(0f, width));
		TerrainGenerator.instance.generate();
		Minimap.playerTransform = player.transform;
	}

	public void restart() {
		SceneManager.LoadScene("Main");
	}

	private void spawnTime() {
		int numberTime = Random.Range(minNumberTime, maxNumberTime);

		for (int i = 0; i < numberTime; i++) {
			Instantiate(timePrefabs, new Vector3(Random.Range(0f, height), depth * 5, Random.Range(0f, width)), new Quaternion(0f, 0f, 0f, 0f));
		}
	}

}
