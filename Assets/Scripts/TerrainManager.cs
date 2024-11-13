using UnityEngine;
using System;
using freenect;
using System.Threading;

namespace KinectInterface
{
    public class TerrainManager : MonoBehaviour
    {
        internal Terrain terrain;
        private int terrainWidth;
        private int terrainHeight;
        private float[,] heightData;
        
        internal DataProcessor dataProcessor;

        public void Initialize(Terrain terrainInstance, DataProcessor processor)
        {
            terrain = terrainInstance;
            dataProcessor = processor;
        }

        private void Start() {
            if (terrain != null) {
                terrainWidth = terrain.terrainData.heightmapResolution;
                terrainHeight = terrain.terrainData.heightmapResolution;
                heightData = new float[terrainWidth, terrainHeight];
                Debug.Log("Terrain initialized with width: " + terrainWidth + " and height: " + terrainHeight);
            } else {
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
                    terrain.terrainData.SetHeights(0, 0, newHeightData);
                    Debug.Log("Terrain updated.");
                }
            }
        }
    }
}
