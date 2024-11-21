using UnityEngine;
using System;

namespace KinectInterface
{
    public class MainController : MonoBehaviour
    {
        private TerrainManager terrainManager;   // Manages terrain updates
        private DataProcessor dataProcessor;     // Processes depth data
        private KinectManager kinectManager;     // Manages Kinect connection and data
        private DataReceiver dataReceiver;       // Receives data from Kinect (formerly PreviewControl)

        public Terrain terrain; // Assign this in the Unity Inspector

        private float updateInterval = 0.1f; // Interval for updating the terrain in seconds
        private float timeSinceLastUpdate = 0f;

        void Start()
        {
            // Initialize components using AddComponent<T>()
            dataReceiver = new DataReceiver();

            kinectManager = gameObject.AddComponent<KinectManager>();
            kinectManager.Initialize(dataReceiver);

            dataProcessor = gameObject.AddComponent<DataProcessor>();
            dataProcessor.Initialize(kinectManager);

            terrainManager = gameObject.AddComponent<TerrainManager>();
            terrainManager.Initialize(terrain, dataProcessor);

            Debug.Log("MainController initialized all components successfully.");
        }

        void Update()
        {
            // Update terrain data periodically
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate = 0f;
                terrainManager.UpdateTerrain();
            }

            // Handle key presses
            if (Input.GetKeyDown(KeyCode.Alpha1)) // Save state
            {
                terrainManager.SaveTerrainState();
                Debug.Log("Terrain state saved.");
            }

            if (Input.GetKeyDown(KeyCode.Alpha0)) // Reset
            {
                terrainManager.ResetTerrainColor();
                Debug.Log("Terrain color reset.");
            }

            if (Input.GetKeyDown(KeyCode.C)) // Apply coloring
            {
                terrainManager.ApplyColoring();
                Debug.Log("Coloring applied to terrain.");
            }
        }
    }
}
