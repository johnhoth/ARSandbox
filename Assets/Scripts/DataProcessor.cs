using UnityEngine;
using System;
using freenect;
using System.Threading;

namespace KinectInterface
{
    public class DataProcessor : MonoBehaviour
    {
        internal KinectManager kinectManager;

        public void Initialize(KinectManager kinectManager) {
            this.kinectManager = kinectManager;
        }

        float DepthToHeight(float depth) {
            return (600 < depth && depth < 2000 ? 2000 - depth : 0);
        }
        
        public unsafe void ProcessDepthData(float[,] heights, Int16* depthPtr, int width, int height) {
            int[] depthCounts = new int[21]; // for debugging purposes

            float minDepth = depthPtr[0], maxDepth = depthPtr[0];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    // Get the depth value and normalize it for terrain heights
                    if (minDepth > depthPtr[index]) minDepth = depthPtr[index];
                    if (maxDepth < depthPtr[index]) maxDepth = depthPtr[index];

                    float depthValue = DepthToHeight(depthPtr[index]) / 1500f; // Adjust scale factor as needed
                    heights[x, y] = depthValue;

                    int depth = depthPtr[index];
                    if (depth >= 0 && depth <= 2099)
                    {
                        int rangeIndex = depth / 100;
                        depthCounts[rangeIndex]++;
                    }
                }
            }

            // Debug.Log($"Terrain updated with processed depth data. Min:{minDepth}, Max:{maxDepth}");
            // Output depth counts for debugging
            // Debug.Log("Depth counts: " + String.Join(", ", depthCounts));
        }
        

        public float[,] GetNewHeightData(int terrainWidth, int terrainHeight) {
            if (kinectManager == null) {
                return null;
            }

            if (kinectManager.dataReceiver.IsDepthDataPending()) {
                this.kinectManager.dataReceiver.SwapDataBuffers();

                IntPtr depthDataBuffer = this.kinectManager.dataReceiver.GetDepthDataBuffer();
                // Debug.Log("Accessing depth data...");
                try {
                    unsafe {
                        Int16* depthPtr = (Int16*)depthDataBuffer;//.ToPointer();
                        int dataWidth = kinectManager.dataReceiver.depthMode.Width;
                        int dataHeight = kinectManager.dataReceiver.depthMode.Height;

                        float[,] heights = new float[terrainWidth, terrainHeight];
                        unsafe {
                            ProcessDepthData(heights, depthPtr, dataWidth, dataHeight);
                        }
                        this.kinectManager.dataReceiver.ClearDepthDataPending();
                        return heights;
                    }
                } catch (Exception ex) {
                    Debug.LogError($"Error processing depth data: {ex.Message}");
                }
                return null;
                // Reset the depth data pending status after processing
                // this.kinectManager.dataReceiver.ClearDepthDataPending();
            } else {
                return null;
                // Debug.Log("No new depth data pending.");
            }
        }

    }
    
}
