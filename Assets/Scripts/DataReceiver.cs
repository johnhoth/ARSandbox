using UnityEngine;
using System;
using freenect;
using System.Threading;
using System.Collections.Generic;

namespace KinectInterface
{
    public class DataReceiver
    {     
        public VideoFrameMode VideoMode {
            get 
            {
                return this.videoMode;
            }
            set
            {
                if (this.videoMode == value)
                {
                    return;
                } 
                if (this.videoDataBuffers != null)
                {
                    this.videoDataBuffers.Dispose();
                }
                this.videoDataBuffers = new SwapBufferCollection<byte>(3, 3 * value.Width * value.Height);
                this.videoMode = value;
            }
        }

        public DepthFrameMode DepthMode
        {
            get
            {
                return this.depthMode;
            }
            set
            {
                if (this.depthMode == value)
                {
                    return;
                }
                if (this.depthDataBuffers != null)
                {
                    this.depthDataBuffers.Dispose();
                }
                this.depthDataBuffers = new SwapBufferCollection<byte>(3, 3 * value.Width * value.Height);
                this.depthMode = value;
            }
        }

        public IntPtr VideoBackBuffer {
            get {
                return this.videoDataBuffers.GetHandle(2);
            }
        }

        public IntPtr DepthBackBuffer {
            get {
                return this.depthDataBuffers.GetHandle(2);
            }
        }

        public int VideoFPS { get; private set; }
        public int DepthFPS { get; private set; }


        internal VideoFrameMode videoMode = null;
        internal DepthFrameMode depthMode = null;
        private SwapBufferCollection<byte> videoDataBuffers;
        private SwapBufferCollection<byte> depthDataBuffers;
        private bool videoDataPending = false;
        private bool depthDataPending = false;
        private UInt16[] gamma = new UInt16[2048];
        private DateTime lastVideoFPSUpdate = DateTime.Now;
        private int videoFrameCount = 0;
        private DateTime lastDepthFPSUpdate = DateTime.Now;
        private int depthFrameCount = 0;

        public DataReceiver() {
            for (int i = 0; i < 2048; ++i) {
                double v = i / 2048.0;
                v = Math.Pow(v, 3.0) * 6.0;
                gamma[i] = (UInt16)(v * 6.0 * 256.0);
            }
        }

        // to check if new depth data is pending
        public bool IsDepthDataPending() {
            return this.depthDataPending;
        } 

        // to swap depth data buffers
        public void SwapDataBuffers() {
            this.depthDataBuffers.Swap(0, 1);
        }

        // to mark depth data as pending
        private void MarkDepthDataAsPending() {
            this.depthDataPending = true;
        }

        // to reset the pending flag after data was used
        public void ClearDepthDataPending() {
            this.depthDataPending = false;
        }

        public void HandleVideoBackBufferUpdate()
        {
            if (this.VideoMode.Format == VideoFormat.Infrared10Bit)
            {
                unsafe
                {
                    Int16* ptrMid = (Int16*)this.videoDataBuffers.GetHandle(1);
                    Int16* ptrBack = (Int16*)this.videoDataBuffers.GetHandle(2);
                    int dim = this.VideoMode.Width * this.VideoMode.Height;
                    Int16 mult = 50;

                    for (int i = 0; i < dim; i++)
                    {
                        *ptrMid++ = (Int16)(ptrBack[i] * mult);
                    }
                }
            }
            else
            {
                this.videoDataBuffers.Swap(1, 2);
            }

            this.videoFrameCount++;
            if ((DateTime.Now - this.lastVideoFPSUpdate).Seconds >= 1)
            {
                this.VideoFPS = this.videoFrameCount;
                this.videoFrameCount = 0;
                this.lastVideoFPSUpdate = DateTime.Now;
            }

            this.videoDataPending = true;
            // Console.WriteLine("Video buffer updated. FPS: " + this.VideoFPS);
        }

        public void HandleDepthBackBufferUpdate() { 
            unsafe {
                Int16 *ptrMid   = (Int16 *)this.depthDataBuffers.GetHandle(1);
                Int16 *ptrBack  = (Int16 *)this.depthDataBuffers.GetHandle(2);
                int dim         = this.DepthMode.Width * this.DepthMode.Height;
                Int16 pval      = 0;
                Int16 lb        = 0;
                int i           = 0;   
                for (i = 0; i < dim; ++i) {
                    *ptrMid++ = ptrBack[i];
                } 
            }

            this.depthFrameCount++;
            if ((DateTime.Now - this.lastDepthFPSUpdate).Seconds >= 1) {
                this.DepthFPS = this.depthFrameCount;
                this.depthFrameCount = 0;
                this.lastDepthFPSUpdate = DateTime.Now;
            }

            MarkDepthDataAsPending();

            // Debug.Log("Depth buffer updated. FPS: " + this.DepthFPS);
        }


        public IntPtr GetDepthDataBuffer() {
            return this.depthDataBuffers.GetHandle(0);
        }
    }
}
