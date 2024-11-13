using UnityEngine;
using System;
using freenect;
using System.Threading;
using System.Collections.Generic;

namespace KinectInterface
{
    public class KinectManager : MonoBehaviour
    {

        public static KinectManager Instance { get; private set; }

        // Awake method to initialize the Singleton instance

        // private void Awake() {
        //     if (Instance != null && Instance != this) {
        //         Destroy(gameObject);
        //         return;
        //     }
        //     Instance = this;

        //     DontDestroyOnLoad(gameObject);
        // }


        public DataReceiver dataReceiver;
        private Kinect kinect = null;
        private volatile bool isRunning = false;
        private Thread updateThread = null;

        private float updateInterval = 0.2f;
        private float timeSinceLastUpdate = 0.0f;


        private int depthDataWidth = -1;
        private int depthDataHeight = -1;

        // void Start()
        // {
            
        // }
        // public KinectManager(DataReceiver dataReceiver)
        // {
        //     this.dataReceiver = dataReceiver;
        //     this.Connect(0);
        // }

        void OnApplicationQuit()
        {
            Debug.Log("Application is quitting. Disconnecting Kinect...");
            this.Disconnect();
        }

        public void Initialize(DataReceiver dataReceiver) {
            this.dataReceiver = dataReceiver;
            this.Connect(0);
        }

        public void SetLEDColor(LEDColor color) {
            if (this.kinect != null) {
                try {
                    this.kinect.LED.Color = color;
                    Debug.Log($"LED Color was set to: {color}");
                } catch (Exception ex) {
                    Debug.LogError($"Failed to set LED color: {ex.Message}");
                }
            } else {
                Debug.LogWarning("Kinect device is not initialized, cannot set LED color.");
            }
        }

        internal void Connect(int deviceID) {
            try {
                if (this.isRunning) {
                    Debug.Log("Kinect is already running, no need to reconnect");
                    return;
                }

                this.kinect = new Kinect(deviceID);
                this.kinect.Open();
                isRunning = true;


                this.dataReceiver.DepthMode = kinect.DepthCamera.Mode;

                this.kinect.DepthCamera.DataReceived += HandleKinectDepthCameraDataReceived;
                this.kinect.VideoCamera.DataReceived += HandleKinectVideoCameraDataReceived;

                this.kinect.DepthCamera.Start();

                this.updateThread = new Thread(delegate(){
                    while (this.isRunning) {
                        try {
                            this.kinect.UpdateStatus();
                            Kinect.ProcessEvents();
                        } catch (ThreadInterruptedException e) {
                            return;
                        } catch (Exception ex) {
                            Debug.LogError($"Error during Kinect update loop: {ex.Message}");
                        }
                    }
                });

                this.updateThread.Start();

                SetLEDColor(LEDColor.Yellow);
                Debug.Log("Kinect device connected and opened successfully.");
            } catch (Exception ex) {
                Debug.LogError($"Failed to connect Kinect: {ex.Message}");
                isRunning = false;
            }
        }

        private void Disconnect() {
            if (!this.isRunning) {
                Debug.LogWarning("Disconnect called, but Kinect is not running");
                return;
            }
            if (kinect == null) {
                Debug.LogWarning("Disconnect called, but was never initialized");
                return;
            }

            Debug.Log("Disconnecting Kinect device...");
            try {
                this.isRunning = false;

                this.updateThread.Interrupt();
                this.updateThread.Join();
                this.updateThread = null;

                this.kinect.DepthCamera.Stop();
                Debug.Log("Depth camera has been stopped.");

                SetLEDColor(LEDColor.Green);

                this.kinect.Close();
                this.kinect = null;

                Debug.Log("Kinect device disconnected successfully.");
            } catch (Exception ex) {
                Debug.LogError($"Error while disconnecting Kinect: {ex.Message}");
            } finally {
                this.kinect = null;
            }
        }

        private void HandleKinectVideoCameraDataReceived(object sender, VideoCamera.DataReceivedEventArgs e) {
            Debug.Log("Video data received");
            // try {
            //     TODO
            // } catch (Exception ex) {
            //     Debug.LogError($"Error in HandleKinectDepthCameraDataReceived: {ex.Message}");
            // }
        }

        private void HandleKinectDepthCameraDataReceived(object sender, DepthCamera.DataReceivedEventArgs e) {
            try
            {
                this.dataReceiver.HandleDepthBackBufferUpdate();
                this.kinect.DepthCamera.DataBuffer = this.dataReceiver.DepthBackBuffer;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in HandleKinectDepthCameraDataReceived: {ex.Message}");
            }
        }
    }
}
