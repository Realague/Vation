﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PerlinNoise {

	public enum NormalizedMode {Local, Global};

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed,
											int octaves, float persistance, float lacunarity, Vector2 offset, NormalizedMode normalizedMode) {
		float [,] noiseMap = new float[mapWidth, mapHeight];

		System.Random prgn = new System.Random(seed); 
		Vector2[] octaveOffsets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < octaves; i++) {
			float offsetX = prgn.Next(-100000, 100000) + offset.x;
			float offsetY = prgn.Next(-100000, 100000) - offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		if (scale <= 0) {
			scale = 0.0001f;
		}

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

				amplitude = 1;
				frequency = 1;

				float noiseHeight = 0;

				float halfWidth = mapWidth / 2f;
				float halfHeight = mapHeight / 2f;

				for (int i = 0; i < octaves; i++) {
					float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;
					
					amplitude *= persistance;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxLocalNoiseHeight) {
					maxLocalNoiseHeight = noiseHeight;
				} else if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap[x, y] = noiseHeight;
			}
		}

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				if (normalizedMode == NormalizedMode.Local) {
					noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
				} else {
					float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
					noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
				}
			}
		}
		return noiseMap;
	}
}
