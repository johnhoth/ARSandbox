using UnityEngine;

namespace KinectInterface
{
    public class TerrainManager : MonoBehaviour
    {
        internal Terrain terrain;
        private int terrainWidth;
        private int terrainHeight;
        private float[,] heightData;         // Current terrain heights
        private float[,] savedHeightData;    // Saved state for comparison
        private Texture2D terrainTexture;   // Texture for coloring
        private Material defaultMaterial;   // Store the default material
        private bool isColoringEnabled = false; // Control whether coloring is applied

        internal DataProcessor dataProcessor;

        public void Initialize(Terrain terrainInstance, DataProcessor processor)
        {
            terrain = terrainInstance;
            dataProcessor = processor;
        }

        private void Start()
        {
            if (terrain != null)
            {
                terrainWidth = terrain.terrainData.heightmapResolution;
                terrainHeight = terrain.terrainData.heightmapResolution;
                heightData = new float[terrainWidth, terrainHeight];
                savedHeightData = new float[terrainWidth, terrainHeight];
                terrainTexture = new Texture2D(terrainWidth, terrainHeight);

                // Save the default material for resetting
                defaultMaterial = terrain.materialTemplate;

                Debug.Log("Terrain initialized with width: " + terrainWidth + " and height: " + terrainHeight);
            }
            else
            {
                Debug.LogError("Terrain not assigned.");
            }
        }

        internal void UpdateTerrain()
        {
            if (terrain != null)
            {
                float[,] newHeightData = dataProcessor.GetNewHeightData(terrainWidth, terrainHeight);
                if (newHeightData != null)
                {
                    heightData = newHeightData;
                    terrain.terrainData.SetHeights(0, 0, newHeightData);

                    if (isColoringEnabled)
                    {
                        ApplyColoring();
                    }
                    // Debug.Log("Terrain updated.");
                }
            }
        }

        public void SaveTerrainState()
        {
            for (int y = 0; y < terrainHeight; y++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    savedHeightData[x, y] = heightData[x, y];
                }
            }

            isColoringEnabled = true; // Enable coloring when saving state
            Debug.Log("Terrain state saved and coloring enabled.");
        }

        public void ResetTerrainColor()
        {
            isColoringEnabled = false; // Disable coloring
            terrain.materialTemplate = defaultMaterial;
            Debug.Log("Terrain color reset to default and coloring disabled.");
        }

        internal void ApplyColoring()
        {
            if (savedHeightData == null)
            {
                Debug.LogError("No saved terrain state found!");
                return;
            }

            float interval = 0.05f; // Interval for geodesic lines
            float precision = 0.0001f; // Precision for floating-point comparisons
            float intensity = 20f; // Gradient intensity multiplier

            for (int y = 0; y < terrainHeight; y++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    float diff = heightData[x, y] - savedHeightData[x, y];
                    Color color;

                    // Check if the current height is close to a geodesic level
                    float normalizedHeight = heightData[x, y] / interval;
                    bool isGeodesicLine = false;//Mathf.Abs(normalizedHeight - Mathf.Round(normalizedHeight)) < precision;

                    if (isGeodesicLine)
                    {
                        color = Color.black; // Draw geodesic line
                    }
                    else if (Mathf.Abs(diff) < precision) // Close to original
                    {
                        color = Color.white;
                    }
                    else if (diff > 0) // Higher than saved
                    {
                        color = Color.Lerp(Color.white, Color.red, Mathf.Clamp01(diff * intensity)); // Brighter red
                    }
                    else // Lower than saved
                    {
                        color = Color.Lerp(Color.white, Color.blue, Mathf.Clamp01(-diff * intensity)); // Brighter blue
                    }

                    // Swapping x and y to fix texture alignment
                    terrainTexture.SetPixel(y, x, color);
                }
            }

            terrainTexture.Apply();
            ApplyTextureToTerrain(terrainTexture);
        }



        private void ApplyTextureToTerrain(Texture2D texture)
        {
            // Create a material with the correct shader
            Material terrainMaterial = new Material(Shader.Find("Unlit/Texture"));
            terrainMaterial.mainTexture = texture;

            // Apply the material to the terrain
            terrain.materialTemplate = terrainMaterial;
        }
    }
}
