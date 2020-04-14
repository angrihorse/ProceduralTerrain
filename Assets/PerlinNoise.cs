using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PerlinNoise : MonoBehaviour
{
    // Width and height of the texture in pixels.
	[Range(10f, 200f)]
    public int resolution;

	// The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
	[Range(1f, 100f)]
	public float baseFrequency = 10f;

	[Range(-1f, 1f)]
	public float verticalOffset;

	public float moveSpeed;
	public float scrollSpeed;
	public TerrainType[] terrainTypes;

    private Texture2D texture;
    private Color[] colorMap;

    void Start()
    {
        texture = new Texture2D(resolution, resolution);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
        colorMap = new Color[texture.width * texture.height];
		Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
    }

	float[,] GenerateNoiseLayer(float frequency, float amplitude, int size) {
		float[,] noiseLayer = new float[size, size];
		for (int x = 0; x < size; x++) {
			for (int y = 0; y < size; y++) {
				float xCoord = ((float)(x) / size - 0.5f) * frequency - transform.position[0];
                float yCoord = ((float)(y) / size - 0.5f) * frequency - transform.position[2];
                noiseLayer[x, y] = amplitude * Mathf.PerlinNoise(xCoord, yCoord);
			}
		}

		return noiseLayer;
	}

	float[,] GenerateNoiseMap(float baseFrequency, float verticalOffset, int size) {
		float[,] noiseMap = new float[size, size];
		List<string> items = new List<string>();
		List<float[,]> noiseLayers = new List<float[,]>();

		noiseLayers.Add(GenerateNoiseLayer(baseFrequency * 0.2f, 1f, size));
		noiseLayers.Add(GenerateNoiseLayer(baseFrequency * 0.8f, 0.25f, size));
		noiseLayers.Add(GenerateNoiseLayer(baseFrequency * 1.6f, 0.1f, size));

		for (int x = 0; x < size; x++) {
			for (int y = 0; y < size; y++) {
				foreach (float[,] noiseLayer in noiseLayers) {
					noiseMap[x, y] += noiseLayer[x, y];
				}

				noiseMap[x, y] -= verticalOffset;
			}
		}

		return noiseMap;
	}

	void GenerateTextureFromNoise(float[,] noiseMap) {
		for (int y = 0; y < texture.height; y++) {
			for (int x = 0; x < texture.width; x++) {
				int index = y * texture.width + x;
				colorMap[index] = MapHeightToColor(noiseMap[x, y]);
			}
		}

		texture.SetPixels(colorMap);
        texture.Apply();
	}

	Color MapHeightToColor(float height) {
		foreach (TerrainType terrain in terrainTypes) {
			if (height < terrain.height) {
				return terrain.color;
			}
		}

		return terrainTypes[terrainTypes.Length - 1].color;
	}

    void Update()
    {
		PlayerControls();
        GenerateTextureFromNoise(GenerateNoiseMap(baseFrequency, verticalOffset, resolution));
    }

	void PlayerControls() {
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;
		Vector3 inputDir3D = new Vector3(inputDir.x, 0, inputDir.y);
		transform.Translate(inputDir3D * moveSpeed * Time.deltaTime);

		if (Input.GetAxis("Mouse ScrollWheel") != 0f ) {
		   baseFrequency -= scrollSpeed * Input.GetAxis("Mouse ScrollWheel");
		   if (baseFrequency < 0) {
			   baseFrequency = 0;
		   }
		}
	}
}

[System.Serializable]
public struct TerrainType {
	public float height;
	public Color color;
}
