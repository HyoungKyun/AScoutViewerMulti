using MvCamCtrl.NET;
using MvCamCtrl.NET.CameraParams;

using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using OpenCvSharp.Extensions;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using System.Text.RegularExpressions;

namespace A_Scout_Viewer
{
    public enum STATE_VAL
    {
        CAM_IDLE = 0,
        CAM_OPENED,
        CAM_PREVIEW,
        CAM_SAVE,
        CAM_SAVE_DONE,
        CAM_PLAY,
        CAM_PLAY_PAUSE,
        CAM_FILE_PLAY,
        CAM_FILE_PLAY_PAUSE,
        CAM_FILE_PLAY_PAUSE2,
    }

    public partial class A_Scout : MetroFramework.Forms.MetroForm //상속 클래스 변경 
    {

        int m_nValidCamNum = 0;
        bool m_LEFlag = false;

        private CCamera m_MyCamera1 = null;
        private CCamera m_MyCamera2 = null;
        List<CCameraInfo> m_ltDeviceList = new List<CCameraInfo>();

        bool m_bCam1Thread = false;
        Thread m_hCam1ReceiveThread = null;
        bool m_bCam2Thread = false;
        Thread m_hCam2ReceiveThread = null;

        STATE_VAL m_Cam1State = STATE_VAL.CAM_IDLE;
        STATE_VAL m_Cam2State = STATE_VAL.CAM_IDLE;

        MV_FRAME_OUT_INFO_EX m_Cam1FrameOutInfo;
        MV_FRAME_OUT_INFO_EX m_Cam2FrameOutInfo;
        MV_FRAME_OUT_INFO_EX m_stCam1ImageInfo;
        MV_FRAME_OUT_INFO_EX m_stCam2ImageInfo;

        byte[] m_pCam1DisplayBuffer = null;
        byte[] m_pCam2DisplayBuffer = null;
        byte[][] m_pSaveBuffer1 = new byte[Constants.CAM1_SAVE_COUNT][];
        byte[][] m_pSaveBuffer2 = new byte[Constants.CAM2_SAVE_COUNT][];

        bool m_bCam1NewFrame = false;
        bool m_bCam2NewFrame = false;

        int m_nSaveIndex1 = 0;
        int m_nSaveIndex2 = 0;

        int m_Cam1SaveCount = Constants.CAM1_SAVE_COUNT;
        int m_Cam2SaveCount = Constants.CAM2_SAVE_COUNT;

        bool m_bCaptureFlag1 = false;
        bool m_bCaptureFlag2 = false;

        Mat m_pOriginalImage1 = null;
        Mat m_pDisplayImage1 = null;
        Mat m_pDisplayMode1 = null;

        Mat m_pOriginalImage2 = null;
        Mat m_pDisplayImage2 = null;
        Mat m_pDisplayMode2 = null;

        VideoCapture video_file = null;

        int m_LastDisplayIndex1 = 0;
        int m_LastDisplayIndex2 = 0;

        int m_DefaultInterval = 1;
        int m_nCam1Interval = 30;
        int m_nCam2Interval = 30;

        double m_FrameRatio = 1.0;
        double m_nFrameRate = 0.0;
        int m_nSaveFrameRate = 120;
        int m_PlaySpeedValue = 0;

        bool m_bFocusMode = false;
        int m_FocusValue = 0;

        Stopwatch FPSWatch = new Stopwatch();
        long m_nFrameCount = 0;

        static System.Windows.Forms.Timer GrabInfoTimer;
        //static System.Windows.Forms.Timer PlayProgressTimer;

        private SaveFileDialog saveFileDialog1 = null;
        private OpenFileDialog openFileDialog = null;

        private static cbOutputExdelegate Cam1Callback;
        static void Cam1CallbackFunc(IntPtr pData, ref MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            if (pUser != null)
            {
                A_Scout thisClass = (A_Scout)GCHandle.FromIntPtr(pUser).Target;
                thisClass.m_nFrameCount++;
                switch (thisClass.m_Cam1State)
                {
                    case STATE_VAL.CAM_PREVIEW:
                        if (thisClass.m_bCam1NewFrame == false)
                        {
                            Marshal.Copy(pData, thisClass.m_pCam1DisplayBuffer, 0, (int)pFrameInfo.nFrameLen);
                            thisClass.m_stCam1ImageInfo = pFrameInfo;
                            thisClass.m_bCam1NewFrame = true;
                        }
                        break;

                    case STATE_VAL.CAM_SAVE:
                        Marshal.Copy(pData, thisClass.m_pSaveBuffer1[thisClass.m_nSaveIndex1], 0, (int)pFrameInfo.nFrameLen);
                        thisClass.m_stCam1ImageInfo = pFrameInfo;
                        thisClass.m_bCam1NewFrame = true;
                        thisClass.m_nSaveIndex1++;
                        if (thisClass.m_nSaveIndex1 == thisClass.m_Cam1SaveCount)
                        {
                            thisClass.m_nSaveIndex1 = 0;
                            thisClass.m_bCaptureFlag1 = true;
                            thisClass.m_Cam1State = STATE_VAL.CAM_SAVE_DONE;
                        }
                        break;

                    default:

                        break;
                }
            }
        }

        private static cbOutputExdelegate Cam2Callback;
        static void Cam2CallbackFunc(IntPtr pData, ref MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            if (pUser != null)
            {
                A_Scout thisClass = (A_Scout)GCHandle.FromIntPtr(pUser).Target;
                thisClass.m_nFrameCount++; 
                switch (thisClass.m_Cam2State)
                {
                    case STATE_VAL.CAM_PREVIEW:
                        Marshal.Copy(pData, thisClass.m_pCam2DisplayBuffer, 0, (int)pFrameInfo.nFrameLen);
                        thisClass.m_stCam2ImageInfo = pFrameInfo;
                        thisClass.m_bCam2NewFrame = true;
                        break;

                    case STATE_VAL.CAM_SAVE:
                        Marshal.Copy(pData, thisClass.m_pSaveBuffer2[thisClass.m_nSaveIndex2], 0, (int)pFrameInfo.nFrameLen);
                        thisClass.m_stCam2ImageInfo = pFrameInfo;
                        thisClass.m_bCam2NewFrame = true;
                        thisClass.m_nSaveIndex2++;
                        if (thisClass.m_nSaveIndex2 == thisClass.m_Cam2SaveCount)
                        {
                            thisClass.m_nSaveIndex2 = 0;
                            thisClass.m_bCaptureFlag2 = true;
                            thisClass.m_Cam2State = STATE_VAL.CAM_SAVE_DONE;
                        }
                        break;

                    default:

                        break;
                }
            }
        }

        public A_Scout()
        {            
            InitializeComponent();
            DeviceListAcq();
            if (m_nValidCamNum > 0)
            {
                ThreadCallbackStart();
                MemoryInitialize();
            }
            InitializeContents();
        }

        private void DeviceListAcq()
        {
            System.GC.Collect();
            m_ltDeviceList.Clear();
            int nRet = CSystem.EnumDevices(CSystem.MV_USB_DEVICE, ref m_ltDeviceList);
            if (0 != nRet)
            {
                //MessageBox.Show("Enumerate devices fail!");
                return;
            }
            TileState.Text = "Connection Check \r" +  "Connect Camera && \r" + "Press this tile";
            m_nValidCamNum = 0;
            for (int i = 0; i < m_ltDeviceList.Count; i++)
            {
                if (m_ltDeviceList[i].nTLayerType == CSystem.MV_USB_DEVICE)
                {
                    CUSBCameraInfo usbInfo = (CUSBCameraInfo)m_ltDeviceList[i];
                    if (usbInfo.chModelName == "MV-CS017-10UC")
                    {
                        m_nValidCamNum++;
                        CameraOpen(i, 0);
                    }
                    else if (usbInfo.chModelName == "MV-CS016-10UC")
                    {
                        m_nValidCamNum++;
                        m_LEFlag = true;
                        CameraOpen(i, 1);

                        int currentWidth = pictureBox1.Width;
                        int currentHeight = pictureBox1.Height;
                        int newWidth = currentWidth; // Put your desired width here
                        int newHeight = (int)((1080.0 / 1440.0) * newWidth);
                        pictureBox1.Top = pictureBox1.Top - 80;
                        pictureBox1.Size = new System.Drawing.Size(newWidth, newHeight);
                    }
                }
            }
        }

        private void CameraOpen(int Index, int CamNumber)
        {
            if (CamNumber == 0)
            {
                if (null == m_MyCamera1)
                {
                    m_MyCamera1 = new CCamera();
                    if (null == m_MyCamera1)
                    {
                        return;
                    }
                }

                CCameraInfo device = m_ltDeviceList[Index];
                int nRet = m_MyCamera1.CreateHandle(ref device);
                if (CErrorDefine.MV_OK != nRet)
                {
                    return;
                }

                nRet = m_MyCamera1.OpenDevice();
                if (CErrorDefine.MV_OK != nRet)
                {
                    m_MyCamera1.DestroyHandle();
                    //MessageBox.Show("Device open fail! ");
                    return;
                }

                float DigitalShift = 5.0f;
                m_MyCamera1.SetEnumValue("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                m_MyCamera1.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                m_MyCamera1.SetIntValue("Width", Constants.CAM1_WIDTH);
                m_MyCamera1.SetBoolValue("DigitalShiftEnable", true);
                m_MyCamera1.SetFloatValue("DigitalShift", DigitalShift);
                m_Cam1State = STATE_VAL.CAM_OPENED;
                TileState.Text = "Camera State : \r" + "Camera Ready";
            }
            else if (CamNumber == 1)
            {
                if (null == m_MyCamera2)
                {
                    m_MyCamera2 = new CCamera();
                    if (null == m_MyCamera2)
                    {
                        return;
                    }
                }

                CCameraInfo device = m_ltDeviceList[Index];
                int nRet = m_MyCamera2.CreateHandle(ref device);
                if (CErrorDefine.MV_OK != nRet)
                {
                    return;
                }

                nRet = m_MyCamera2.OpenDevice();
                if (CErrorDefine.MV_OK != nRet)
                {
                    m_MyCamera2.DestroyHandle();
                    //MessageBox.Show("Device open fail! ");
                    return;
                }
                //float DigitalShift = 5.0f;
                m_MyCamera2.SetEnumValue("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                m_MyCamera2.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                m_MyCamera2.SetIntValue("Width", Constants.CAM2_WIDTH);
                //m_MyCamera2.SetBoolValue("DigitalShiftEnable", true);
                //m_MyCamera2.SetFloatValue("DigitalShift", DigitalShift);
                m_Cam2State = STATE_VAL.CAM_OPENED;
                TileState.Text = "Camera State : \r" + "Camera Ready";
            }
        }

        private void ThreadCallbackStart()
        {
            int nRet;
            // this 객체를 GCHandle로 래핑합니다.
            GCHandle handle = GCHandle.Alloc(this);
            // GCHandle을 IntPtr로 변환합니다.
            IntPtr thisClassPtr = GCHandle.ToIntPtr(handle);

            if (m_MyCamera1 != null)
            {
                m_bCam1Thread = true;
                m_hCam1ReceiveThread = new Thread(Cam1ThreadProcess);
                m_hCam1ReceiveThread.Start();

                Cam1Callback = new cbOutputExdelegate(Cam1CallbackFunc);
                nRet = m_MyCamera1.RegisterImageCallBackEx(Cam1Callback, thisClassPtr);
                if (CErrorDefine.MV_OK != nRet)
                {
                    //MessageBox.Show("Register Cam1 image callback failed!");
                }
            }

            if (m_MyCamera2 != null)
            {
                m_bCam2Thread = true;
                m_hCam2ReceiveThread = new Thread(Cam2ThreadProcess);
                m_hCam2ReceiveThread.Start();

                Cam2Callback = new cbOutputExdelegate(Cam2CallbackFunc);
                nRet = m_MyCamera2.RegisterImageCallBackEx(Cam2Callback, thisClassPtr);
                if (CErrorDefine.MV_OK != nRet)
                {
                    //MessageBox.Show("Register Cam2 image callback failed!");
                }
            }
        }

        public void Cam1ThreadProcess()
        {
            while (m_bCam1Thread)
            {
                switch (m_Cam1State)
                {
                    case STATE_VAL.CAM_PREVIEW:
                        if ((m_bCam1NewFrame) && (m_pOriginalImage1 == null) && (m_pDisplayImage1 == null))
                        {
                            CreateImageBuffer1();
                        }
                        else
                        {
                            if ((m_bCam1NewFrame) && (m_pOriginalImage1 != null) && (m_pDisplayImage1 != null))
                            {
                                //ImageDisplayCam1();
                                if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                                {
                                    pictureBox1.Invoke(new MethodInvoker(delegate
                                    {
                                        ImageDisplayCam1();
                                        m_bCam1NewFrame = false;
                                    }));
                                    //return;
                                }
                                //m_bCam1NewFrame = false;
                            }
                        }
                        Thread.Sleep(30);
                        break;
                    case STATE_VAL.CAM_SAVE:
                        if ((m_bCam1NewFrame) && (m_nSaveIndex1 > 1))
                        {
                            m_bCam1NewFrame = false;
                            Buffer.BlockCopy(m_pSaveBuffer1[m_nSaveIndex1 - 1], 0, m_pCam1DisplayBuffer, 0, (int)m_stCam1ImageInfo.nFrameLen);
                            //ImageDisplayCam1();
                            if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                            {
                                pictureBox1.Invoke(new MethodInvoker(delegate
                                {
                                    ImageDisplayCam1();                                    
                                }));
                                //return;
                            }
                            Thread.Sleep(30);
                        }
                        break;
                    case STATE_VAL.CAM_SAVE_DONE:
                        if ((m_Cam2State == STATE_VAL.CAM_IDLE) || (m_Cam2State == STATE_VAL.CAM_SAVE_DONE))
                        {
                            GrabInfoTimer.Stop();

                            if (m_MyCamera1 != null)
                            {
                                m_MyCamera1.StopGrabbing();
                                m_Cam1State = STATE_VAL.CAM_OPENED;
                                TileState.Invoke(new Action(() =>
                                {
                                    TileState.Text = "Recording complete";
                                }));
                            }

                            if (m_MyCamera2 != null)
                            {
                                m_MyCamera2.StopGrabbing();
                                m_Cam2State = STATE_VAL.CAM_OPENED;
                                TileState.Invoke(new Action(() =>
                                {
                                    TileState.Text = "Recording complete";
                                }));
                            }
                        }

                        break;
                    case STATE_VAL.CAM_PLAY:
                        if ((m_pOriginalImage1 == null) && (m_pDisplayImage1 == null))
                        {
                            CreateImageBuffer1();
                        }
                        else
                        {
                            if ((m_LastDisplayIndex1 != m_nSaveIndex1) && (m_nSaveIndex1 < m_Cam1SaveCount))
                            {
                                Buffer.BlockCopy(m_pSaveBuffer1[m_nSaveIndex1], 0, m_pCam1DisplayBuffer, 0, (int)m_stCam1ImageInfo.nFrameLen);
                                //ImageDisplayCam1();
                                if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                                {
                                    pictureBox1.Invoke(new MethodInvoker(delegate
                                    {
                                        ImageDisplayCam1();
                                        m_LastDisplayIndex1 = m_nSaveIndex1;

                                        tbPlay.Value = m_nSaveIndex1;
                                    }));
                                    //return;
                                }
                                //m_LastDisplayIndex1 = m_nSaveIndex1;
                            }
                        }
                                                
                        m_nSaveIndex1+= m_DefaultInterval;

                        if (m_nSaveIndex1 >= m_Cam1SaveCount)
                        {
                            m_nSaveIndex1 = m_Cam1SaveCount-1;
                            Thread.Sleep(100);
                            if (m_MyCamera1 != null)
                            {
                                m_Cam1State = STATE_VAL.CAM_OPENED;
                            }
                            else
                            {
                                m_Cam1State = STATE_VAL.CAM_IDLE;
                            }
                           
                            m_nSaveIndex1 = 0;
                            Thread.Sleep(300);
                            //PlayProgressTimer.Stop();
                            TileState.Invoke(new Action(() =>
                            {
                                tbPlay.Value = m_Cam1SaveCount - 1;
                                TileState.Text = "Play End";
                                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
                            }));
                        }

                        Thread.Sleep(m_nCam1Interval);
                       

                        break;
                    case STATE_VAL.CAM_PLAY_PAUSE:

                        Buffer.BlockCopy(m_pSaveBuffer1[m_nSaveIndex1], 0, m_pCam1DisplayBuffer, 0, (int)m_stCam1ImageInfo.nFrameLen);
                        if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                        {
                            pictureBox1.Invoke(new MethodInvoker(delegate
                            {
                                ImageDisplayCam1();
                                tbPlay.Value = m_nSaveIndex1;
                            }));
                            //return;
                        }

                        if (m_MyCamera1 != null)
                        {
                            m_Cam1State = STATE_VAL.CAM_OPENED;
                        }
                        else
                        {
                            m_Cam1State = STATE_VAL.CAM_IDLE;
                        }

                        break;

                    case STATE_VAL.CAM_FILE_PLAY:
                        if ((m_pOriginalImage1 == null) && (m_pDisplayImage1 == null))
                        {
                            CreateImageBuffer1();
                        }
                        else
                        {
                            if(m_nSaveIndex1 < m_Cam1SaveCount)
                            {
                                video_file.PosFrames = m_nSaveIndex1;
                                video_file.Read(m_pDisplayImage1);
                                m_LastDisplayIndex1 = m_nSaveIndex1;
                                Bitmap bitmap1 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage1);
                                if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                                {
                                    pictureBox1.Invoke(new MethodInvoker(delegate
                                    {
                                        if (pictureBox1.Image != null)
                                        {
                                            pictureBox1.Image.Dispose();
                                        }
                                        pictureBox1.Image = bitmap1;
                                        tbPlay.Value = m_nSaveIndex1;
                                    }));
                                }
                                m_nSaveIndex1 += m_DefaultInterval;                               
                                Thread.Sleep(m_nCam1Interval);                               
                            }
                            else
                            {
                                m_nSaveIndex1 = m_Cam1SaveCount - 1;
                                video_file.Read(m_pDisplayImage1);
                                m_LastDisplayIndex1 = m_nSaveIndex1;
                                Bitmap bitmap2 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage1);
                                if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                                {
                                    pictureBox1.Invoke(new MethodInvoker(delegate
                                    {
                                        if (pictureBox1.Image != null)
                                        {
                                            pictureBox1.Image.Dispose();
                                        }
                                        pictureBox1.Image = bitmap2;                                        
                                    }));
                                }

                                video_file.Release();
                                video_file = null;
                                if (m_MyCamera1 != null)
                                {
                                    m_Cam1State = STATE_VAL.CAM_OPENED;
                                }
                                else
                                {
                                    m_Cam1State = STATE_VAL.CAM_IDLE;
                                }

                                TileState.Invoke(new Action(() =>
                                {
                                    tbPlay.Value = m_Cam1SaveCount - 1;
                                    TileState.Text = "Play End";
                                    btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
                                }));
                            }
                        }
                        break;

                    case STATE_VAL.CAM_FILE_PLAY_PAUSE:
                        video_file.PosFrames = m_nSaveIndex1;
                        video_file.Read(m_pDisplayImage1);
                        m_LastDisplayIndex1 = m_nSaveIndex1;
                        Bitmap bitmap3 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage1);
                        if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                        {
                            pictureBox1.Invoke(new MethodInvoker(delegate
                            {
                                if (pictureBox1.Image != null)
                                {
                                    pictureBox1.Image.Dispose();
                                }
                                pictureBox1.Image = bitmap3;
                                tbPlay.Value = m_nSaveIndex1;
                            }));
                        }
                        m_Cam1State = STATE_VAL.CAM_FILE_PLAY_PAUSE2;
                        break;

                    default:
                        Thread.Sleep(10);
                        break;
                }
                Thread.Sleep(1);
            }
        }

        public void Cam2ThreadProcess()
        {
            while (m_bCam2Thread)
            {
                switch (m_Cam2State)
                {
                    case STATE_VAL.CAM_PREVIEW:
                        if ((m_bCam2NewFrame) && (m_pOriginalImage2 == null) && (m_pDisplayImage2 == null))
                        {
                            CreateImageBuffer2();
                        }
                        else
                        {
                            if ((m_bCam2NewFrame) && (m_pOriginalImage2 != null) && (m_pDisplayImage2 != null))
                            {
                                //ImageDisplayCam1();
                                if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                                {
                                    pictureBox1.Invoke(new MethodInvoker(delegate
                                    {
                                        ImageDisplayCam2();
                                        m_bCam1NewFrame = false;
                                    }));
                                    //return;
                                }
                                //m_bCam1NewFrame = false;
                            }
                        }
                        Thread.Sleep(30);
                        break;
                    case STATE_VAL.CAM_SAVE:
                        if ((m_bCam2NewFrame) && (m_nSaveIndex2 > 1))
                        {
                            m_bCam2NewFrame = false;
                            Buffer.BlockCopy(m_pSaveBuffer2[m_nSaveIndex2 - 1], 0, m_pCam2DisplayBuffer, 0, (int)m_stCam2ImageInfo.nFrameLen);
                            //ImageDisplayCam1();
                            if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                            {
                                pictureBox1.Invoke(new MethodInvoker(delegate
                                {
                                    ImageDisplayCam2();
                                }));
                                //return;
                            }
                            Thread.Sleep(30);
                        }
                        break;
                    case STATE_VAL.CAM_SAVE_DONE:
                        if ((m_Cam2State == STATE_VAL.CAM_IDLE) || (m_Cam2State == STATE_VAL.CAM_SAVE_DONE))
                        {
                            GrabInfoTimer.Stop();

                            if (m_MyCamera1 != null)
                            {
                                m_MyCamera1.StopGrabbing();
                                m_Cam1State = STATE_VAL.CAM_OPENED;
                                TileState.Invoke(new Action(() =>
                                {
                                    TileState.Text = "Recording complete";
                                }));
                            }

                            if (m_MyCamera2 != null)
                            {
                                m_MyCamera2.StopGrabbing();
                                m_Cam2State = STATE_VAL.CAM_OPENED;
                                TileState.Invoke(new Action(() =>
                                {
                                    TileState.Text = "Recording complete";
                                }));
                            }
                        }

                        break;
                    case STATE_VAL.CAM_PLAY:
                        if ((m_pOriginalImage2 == null) && (m_pDisplayImage2 == null))
                        {
                            CreateImageBuffer2();
                        }
                        else
                        {
                            if ((m_LastDisplayIndex2 != m_nSaveIndex2) && (m_nSaveIndex2 < m_Cam2SaveCount))
                            {
                                Buffer.BlockCopy(m_pSaveBuffer2[m_nSaveIndex2], 0, m_pCam2DisplayBuffer, 0, (int)m_stCam2ImageInfo.nFrameLen);
                                //ImageDisplayCam1();
                                if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                                {
                                    pictureBox1.Invoke(new MethodInvoker(delegate
                                    {
                                        ImageDisplayCam2();
                                        m_LastDisplayIndex2 = m_nSaveIndex2;

                                        tbPlay.Value = m_nSaveIndex2;
                                    }));
                                    //return;
                                }
                                //m_LastDisplayIndex1 = m_nSaveIndex1;
                            }
                        }

                        m_nSaveIndex2 += m_DefaultInterval;

                        if (m_nSaveIndex2 >= m_Cam2SaveCount)
                        {
                            m_nSaveIndex2 = m_Cam2SaveCount - 1;
                            Thread.Sleep(100);
                            if (m_MyCamera2 != null)
                            {
                                m_Cam2State = STATE_VAL.CAM_OPENED;
                            }
                            else
                            {
                                m_Cam2State = STATE_VAL.CAM_IDLE;
                            }

                            m_nSaveIndex2 = 0;
                            Thread.Sleep(300);
                            //PlayProgressTimer.Stop();
                            TileState.Invoke(new Action(() =>
                            {
                                tbPlay.Value = m_Cam2SaveCount - 1;
                                TileState.Text = "Play End";
                                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
                            }));
                        }

                        Thread.Sleep(m_nCam2Interval);


                        break;
                    case STATE_VAL.CAM_PLAY_PAUSE:

                        Buffer.BlockCopy(m_pSaveBuffer2[m_nSaveIndex2], 0, m_pCam2DisplayBuffer, 0, (int)m_stCam2ImageInfo.nFrameLen);
                        if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                        {
                            pictureBox1.Invoke(new MethodInvoker(delegate
                            {
                                ImageDisplayCam2();
                                tbPlay.Value = m_nSaveIndex2;
                            }));
                            //return;
                        }

                        if (m_MyCamera2 != null)
                        {
                            m_Cam2State = STATE_VAL.CAM_OPENED;
                        }
                        else
                        {
                            m_Cam2State = STATE_VAL.CAM_IDLE;
                        }

                        break;

                    case STATE_VAL.CAM_FILE_PLAY:
                        if ((m_pOriginalImage2 == null) && (m_pDisplayImage2 == null))
                        {
                            CreateImageBuffer2();
                        }
                        else
                        {
                            if (m_nSaveIndex2 < m_Cam2SaveCount)
                            {
                                video_file.PosFrames = m_nSaveIndex2;
                                video_file.Read(m_pDisplayImage2);
                                m_LastDisplayIndex2 = m_nSaveIndex2;
                                Bitmap bitmap1 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage2);
                                if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                                {
                                    pictureBox1.Invoke(new MethodInvoker(delegate
                                    {
                                        if (pictureBox1.Image != null)
                                        {
                                            pictureBox1.Image.Dispose();
                                        }
                                        pictureBox1.Image = bitmap1;
                                        tbPlay.Value = m_nSaveIndex2;
                                    }));
                                }
                                m_nSaveIndex2 += m_DefaultInterval;
                                Thread.Sleep(m_nCam2Interval);
                            }
                            else
                            {
                                m_nSaveIndex2 = m_Cam2SaveCount - 1;
                                video_file.Read(m_pDisplayImage2);
                                m_LastDisplayIndex2 = m_nSaveIndex2;
                                Bitmap bitmap2 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage2);
                                if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                                {
                                    pictureBox1.Invoke(new MethodInvoker(delegate
                                    {
                                        if (pictureBox1.Image != null)
                                        {
                                            pictureBox1.Image.Dispose();
                                        }
                                        pictureBox1.Image = bitmap2;
                                    }));
                                }

                                video_file.Release();
                                video_file = null;
                                if (m_MyCamera2 != null)
                                {
                                    m_Cam2State = STATE_VAL.CAM_OPENED;
                                }
                                else
                                {
                                    m_Cam2State = STATE_VAL.CAM_IDLE;
                                }

                                TileState.Invoke(new Action(() =>
                                {
                                    tbPlay.Value = m_Cam2SaveCount - 1;
                                    TileState.Text = "Play End";
                                    btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
                                }));
                            }
                        }
                        break;

                    case STATE_VAL.CAM_FILE_PLAY_PAUSE:
                        video_file.PosFrames = m_nSaveIndex2;
                        video_file.Read(m_pDisplayImage2);
                        m_LastDisplayIndex2 = m_nSaveIndex2;
                        Bitmap bitmap3 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage2);
                        if (!pictureBox1.IsDisposed && pictureBox1.InvokeRequired)
                        {
                            pictureBox1.Invoke(new MethodInvoker(delegate
                            {
                                if (pictureBox1.Image != null)
                                {
                                    pictureBox1.Image.Dispose();
                                }
                                pictureBox1.Image = bitmap3;
                                tbPlay.Value = m_nSaveIndex2;
                            }));
                        }
                        m_Cam2State = STATE_VAL.CAM_FILE_PLAY_PAUSE2;
                        break;

                    default:
                        Thread.Sleep(10);
                        break;
                }
                Thread.Sleep(1);
            }
        }

        private void MemoryInitialize()
        {
            int Cam1DisplayBufferSize = Constants.CAM1_WIDTH * Constants.CAM1_HEIGHT + Constants.MEM_BUFFER_MARGIN;
            int Cam1SaveBufferSize = Constants.CAM1_WIDTH * Constants.CAM1_HEIGHT + Constants.MEM_BUFFER_MARGIN;

            int Cam2DisplayBufferSize = Constants.CAM2_WIDTH * Constants.CAM2_HEIGHT + Constants.MEM_BUFFER_MARGIN;
            int Cam2SaveBufferSize = Constants.CAM2_WIDTH * Constants.CAM2_HEIGHT + Constants.MEM_BUFFER_MARGIN;

            // display Buffer
            m_pCam1DisplayBuffer = new byte[Cam1DisplayBufferSize];
            {
                if (m_pCam1DisplayBuffer == null)
                {
                    return;
                }
            }

            m_pCam2DisplayBuffer = new byte[Cam2DisplayBufferSize];
            {
                if (m_pCam2DisplayBuffer == null)
                {
                    return;
                }
            }

            for (int i = 0; i < Constants.CAM1_SAVE_COUNT; i++)
            {
                m_pSaveBuffer1[i] = new byte[Cam1SaveBufferSize];
            }

            for (int i = 0; i < Constants.CAM2_SAVE_COUNT; i++)
            {
                m_pSaveBuffer2[i] = new byte[Cam2SaveBufferSize];
            }
        }

        private void CreateImageBuffer1()
        {
            int width;
            int height;

            width = m_stCam1ImageInfo.nWidth;
            height = m_stCam1ImageInfo.nHeight;

            m_pOriginalImage1 = new Mat(height, width, MatType.CV_8UC1);
            m_pDisplayImage1 = new Mat(height, width, MatType.CV_8UC3);
            m_pDisplayMode1 = new Mat(height, width, MatType.CV_8UC3);
        }

        private void CreateImageBuffer2()
        {
            int width;
            int height;

            width = m_stCam2ImageInfo.nWidth;
            height = m_stCam2ImageInfo.nHeight;

            m_pOriginalImage2 = new Mat(height, width, MatType.CV_8UC1);
            m_pDisplayImage2 = new Mat(height, width, MatType.CV_8UC3);
            m_pDisplayMode2 = new Mat(height, width, MatType.CV_8UC3);
        }


        private void ImageDisplayCam1()
        {           
            m_pOriginalImage1.SetArray(m_pCam1DisplayBuffer);
            Cv2.CvtColor(m_pOriginalImage1, m_pDisplayImage1, ColorConversionCodes.BayerGR2BGR);

            if(m_bFocusMode == true)
            {
                FocusTest(m_pDisplayImage1);
            }

            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage1);
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();                
            }
            pictureBox1.Image = bitmap;            
        }


        private void ImageDisplayCam2()
        {
            m_pOriginalImage2.SetArray(m_pCam2DisplayBuffer);
            Cv2.CvtColor(m_pOriginalImage2, m_pDisplayImage2, ColorConversionCodes.BayerBG2BGR);

            if (m_bFocusMode == true)
            {
                FocusTest(m_pDisplayImage2);
            }

            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(m_pDisplayImage2);
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
            pictureBox1.Image = bitmap;
        }

        private void ReleaseImageBuffer()
        {
            if (m_pOriginalImage1 != null)
            {
                m_pOriginalImage1.Dispose();
                m_pOriginalImage1 = null;
            }

            if (m_pDisplayImage1 != null)
            {
                m_pDisplayImage1.Dispose();
                m_pDisplayImage1 = null;
            }

            if (m_pDisplayMode1 != null)
            {
                m_pDisplayMode1.Dispose();
                m_pDisplayMode1 = null;
            }

            if (m_pOriginalImage2 != null)
            {
                m_pOriginalImage2.Dispose();
                m_pOriginalImage2 = null;
            }

            if (m_pDisplayImage2 != null)
            {
                m_pDisplayImage2.Dispose();
                m_pDisplayImage2 = null;
            }

            if (m_pDisplayMode2 != null)
            {
                m_pDisplayMode2.Dispose();
                m_pDisplayMode2 = null;
            }
        }

        private void InitializeContents()
        {            
            cbPlaySpeed.SelectedIndex = m_PlaySpeedValue;

            btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;

            tbPlay.Minimum = 0;
            tbPlay.Maximum = m_Cam1SaveCount - 1;
            tbPlay.Value = 0;

            tbFrameRate.Minimum = 0;
            tbFrameRate.Maximum = 3;
            tbFrameRate.Value = 3;

            tbISO.Minimum = 0;
            tbISO.Maximum = 8;
            tbISO.Value = 4;

            tbExposure.Minimum = 0;
            tbExposure.Maximum = 6;
            tbExposure.Value = 1;

            tgFocusMode.Checked = m_bFocusMode;
            tgFocusMode.Enabled = false;

            if(m_LEFlag == false)
            {
                GetFrameRate();
                GetGain();
                GetExposureTime();
            }
            else if(m_LEFlag == true)
            {
                GetFrameRateLE();
                GetGainLE();
                GetExposureTimeLE();
            }            
        }

        private void btPreview_Click(object sender, EventArgs e)
        {
            int nRet;

            if ((m_Cam1State == STATE_VAL.CAM_OPENED)|| (m_Cam1State == STATE_VAL.CAM_FILE_PLAY_PAUSE) || (m_Cam1State == STATE_VAL.CAM_FILE_PLAY_PAUSE2))
            {
                ReleaseImageBuffer();
                tbPlay.Value = 0;
                //MakeLUT();

                // 타이머 생성 및 설정
                GrabInfoTimer = new System.Windows.Forms.Timer();
                GrabInfoTimer.Interval = 2000;
                GrabInfoTimer.Tick += Timer_Tick;
                m_nFrameCount = 0;

                tgFocusMode.Enabled = true;
                //// 타이머 시작
                GrabInfoTimer.Start();
                FPSWatch.Reset();
                FPSWatch.Start();
                if (m_MyCamera1 != null)
                {
                    nRet = m_MyCamera1.StartGrabbing();
                    if (CErrorDefine.MV_OK != nRet)
                    {
                        return;
                    }
                    m_Cam1State = STATE_VAL.CAM_PREVIEW;
                    TileState.Text = "Camera State : \r" + "Live View";
                }                                
            }
            else if ((m_Cam2State == STATE_VAL.CAM_OPENED) || (m_Cam2State == STATE_VAL.CAM_FILE_PLAY_PAUSE) || (m_Cam2State == STATE_VAL.CAM_FILE_PLAY_PAUSE2))
            {
                ReleaseImageBuffer();
                tbPlay.Value = 0;
                //MakeLUT();

                // 타이머 생성 및 설정
                GrabInfoTimer = new System.Windows.Forms.Timer();
                GrabInfoTimer.Interval = 2000;
                GrabInfoTimer.Tick += Timer_Tick;
                m_nFrameCount = 0;

                tgFocusMode.Enabled = true;
                //// 타이머 시작
                GrabInfoTimer.Start();
                FPSWatch.Reset();
                FPSWatch.Start();
                
                if (m_MyCamera2 != null)
                {
                    nRet = m_MyCamera2.StartGrabbing();
                    if (CErrorDefine.MV_OK != nRet)
                    {
                        return;
                    }
                    m_Cam2State = STATE_VAL.CAM_PREVIEW;
                    TileState.Text = "Camera State : \r" + "Live View";
                }
            }
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            int nRet;

            if (((STATE_VAL.CAM_PREVIEW != m_Cam1State) && (STATE_VAL.CAM_PREVIEW != m_Cam2State) && (STATE_VAL.CAM_SAVE_DONE != m_Cam1State) && (STATE_VAL.CAM_SAVE_DONE != m_Cam2State)))
            {
                return;
            }
            tgFocusMode.Checked = false;
            tgFocusMode.Enabled = false;
            GrabInfoTimer.Stop();

            if (m_MyCamera1 != null)
            {
                nRet = m_MyCamera1.StopGrabbing();
                if (CErrorDefine.MV_OK != nRet)
                {
                    return;
                }
                m_Cam1State = STATE_VAL.CAM_OPENED;
                TileState.Text = "Camera State : \r" + "Camera Ready";
            }

            if (m_MyCamera2 != null)
            {
                nRet = m_MyCamera2.StopGrabbing();
                if (CErrorDefine.MV_OK != nRet)
                {
                    return;
                }
                m_Cam2State = STATE_VAL.CAM_OPENED;
                TileState.Text = "Camera State : \r" + "Camera Ready";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (m_MyCamera1 != null)
            {   
                long elapsedSeconds = (long)FPSWatch.Elapsed.TotalMilliseconds;
                m_nFrameRate = (double) (m_nFrameCount) * 1000 / elapsedSeconds;
                string result = string.Format("FPS : {0:F1}", m_nFrameRate);
                lbFPS.Text = result;
                FPSWatch.Reset();
                m_nFrameCount = 0;
                FPSWatch.Start();
            }
            else if (m_MyCamera2 != null)
            {
                long elapsedSeconds = (long)FPSWatch.Elapsed.TotalMilliseconds;
                m_nFrameRate = (double)(m_nFrameCount) * 1000 / elapsedSeconds;
                string result = string.Format("FPS : {0:F1}", m_nFrameRate);
                lbFPS.Text = result;
                FPSWatch.Reset();
                m_nFrameCount = 0;
                FPSWatch.Start();
            }
        }

        //private void Timer_Tick1(object sender, EventArgs e)
        //{
        //   if ((m_Cam1State == STATE_VAL.CAM_PLAY)||(m_Cam1State == STATE_VAL.CAM_FILE_PLAY))
        //    {
        //        if (m_nSaveIndex1 < m_Cam1SaveCount)
        //        {
        //            tbPlay.Value = m_nSaveIndex1;
        //        }
        //    }            
        //}

        private void A_Scout_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (((STATE_VAL.CAM_PREVIEW == m_Cam1State) || (STATE_VAL.CAM_PREVIEW == m_Cam2State) || (STATE_VAL.CAM_SAVE_DONE == m_Cam1State) || (STATE_VAL.CAM_SAVE_DONE == m_Cam2State)))
            {
                btStop_Click(null, EventArgs.Empty);
            }            

            if (m_bCam1Thread == true)
            {
                m_bCam1Thread = false;
                Thread.Sleep(300);
                m_hCam1ReceiveThread.Join(1000);
            }

            if (m_bCam2Thread == true)
            {
                m_bCam2Thread = false;
                Thread.Sleep(300);
                m_hCam2ReceiveThread.Join(1000);
            }

            if (m_MyCamera1 != null)
            {                
                m_MyCamera1.SetEnumValue("UserSetSelector", 1); // userset 1
                Thread.Sleep(300);
                m_MyCamera1.SetEnumValue("UserSetDefault", 1); // userset 1
                Thread.Sleep(300);
                m_MyCamera1.SetCommandValue("UserSetSave"); // userset 1
                Thread.Sleep(2500);

                m_MyCamera1.CloseDevice();
                m_MyCamera1.DestroyHandle();
            }

            if (m_MyCamera2 != null)
            {
                m_MyCamera2.SetEnumValue("UserSetSelector", 1); // userset 1
                Thread.Sleep(300);
                m_MyCamera2.SetEnumValue("UserSetDefault", 1); // userset 1
                Thread.Sleep(300);
                m_MyCamera2.SetCommandValue("UserSetSave"); // userset 1
                Thread.Sleep(2500);

                m_MyCamera2.CloseDevice();
                m_MyCamera2.DestroyHandle();
            }

            ReleaseImageBuffer();
        }

        private void btCapture_Click(object sender, EventArgs e)
        {
            if ((m_Cam1State != STATE_VAL.CAM_PREVIEW) && (m_Cam2State != STATE_VAL.CAM_PREVIEW))
            {
                MessageBox.Show("Recording is possible when in Live View Mode.");
                return;
            }
            tgFocusMode.Checked = false;
            tgFocusMode.Enabled = false;
            m_bCaptureFlag1 = false;
            m_bCaptureFlag2 = false;
            m_nSaveIndex1 = 0;
            m_nSaveIndex2 = 0;
            TileState.Text = "Recording 3 sec";

            if ((m_MyCamera1 != null) && (m_MyCamera2 != null))
            {
                m_Cam1State = STATE_VAL.CAM_SAVE;
                m_Cam2State = STATE_VAL.CAM_SAVE;
            }
            else if (m_MyCamera1 != null)
            {
                m_Cam1State = STATE_VAL.CAM_SAVE;
            }
            else if (m_MyCamera2 != null)
            {
                m_Cam2State = STATE_VAL.CAM_SAVE;
            }

        }

        private void SetPlaySpeed(int Speed, int FPS)
        {
            switch(FPS)
            {
                case 250:
                    if (Speed == 0)
                    {
                        m_DefaultInterval = 8;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 1)
                    {
                        m_DefaultInterval = 4;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 2)
                    {
                        m_DefaultInterval = 2;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 3)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    break;
                case 180:
                    if(Speed == 0)
                    {
                        m_DefaultInterval = 6;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if(Speed == 1)
                    {
                        m_DefaultInterval = 3;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 2)
                    {
                        m_DefaultInterval = 2;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 3)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    break;
                case 120:
                    if (Speed == 0)
                    {
                        m_DefaultInterval = 4;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 1)
                    {
                        m_DefaultInterval = 2;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 2)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 3)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 50;
                        m_nCam2Interval = 25;
                    }
                    break;
                case 60:
                    if (Speed == 0)
                    {
                        m_DefaultInterval = 2;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 1)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 2)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 50;
                        m_nCam2Interval = 50;
                    }
                    else if (Speed == 3)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 75;
                        m_nCam2Interval = 75;
                    }
                    break;
                case 30:
                    if (Speed == 0)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 25;
                        m_nCam2Interval = 25;
                    }
                    else if (Speed == 1)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 50;
                        m_nCam2Interval = 50;
                    }
                    else if (Speed == 2)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 75;
                        m_nCam2Interval = 75;
                    }
                    else if (Speed == 3)
                    {
                        m_DefaultInterval = 1;
                        m_nCam1Interval = 100;
                        m_nCam2Interval = 100;
                    }
                    break;
                default:
                    break;
            }
        }

        private void btPlay_Click(object sender, EventArgs e)
        {
            int nSpeed;

            if ((!m_bCaptureFlag1) && (!m_bCaptureFlag2))
            {
                MessageBox.Show("There is nothing to play.");
                return;
            }
            tgFocusMode.Checked = false;
            tgFocusMode.Enabled = false;
            if ((m_Cam1State == STATE_VAL.CAM_PREVIEW) || (m_Cam2State == STATE_VAL.CAM_PREVIEW))
            {
                btStop_Click(null, EventArgs.Empty);
            }

            m_LastDisplayIndex1 = 0;
            m_LastDisplayIndex2 = 0;
            nSpeed = cbPlaySpeed.SelectedIndex;
            SetPlaySpeed(nSpeed, m_nSaveFrameRate);

            m_nSaveIndex1 = 0;
            m_nSaveIndex2 = 0;

            //PlayProgressTimer = new System.Windows.Forms.Timer();
            //PlayProgressTimer.Interval = 150; 
            //PlayProgressTimer.Tick += Timer_Tick1;

            

            //// 타이머 시작
            //PlayProgressTimer.Start();
            

            if ((m_bCaptureFlag1) && (m_bCaptureFlag2))
            {
                m_Cam1State = STATE_VAL.CAM_PLAY;
                m_Cam2State = STATE_VAL.CAM_PLAY;
                tbPlay.Maximum = m_Cam1SaveCount - 1;
            }
            else if (m_bCaptureFlag1)
            {
                m_Cam1State = STATE_VAL.CAM_PLAY;
                tbPlay.Maximum = m_Cam1SaveCount - 1;
            }
            else if (m_bCaptureFlag2)
            {
                m_Cam2State = STATE_VAL.CAM_PLAY;
                tbPlay.Maximum = m_Cam2SaveCount - 1;
            }
            TileState.Text = "Play";
            btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Stop;
        }

        private void btPlayMini_Click(object sender, EventArgs e)
        {
            int nSpeed;

            if ((!m_bCaptureFlag1) && (!m_bCaptureFlag2))
            {
                MessageBox.Show("There is nothing to play.");
                return;
            }

            if (m_Cam1State == STATE_VAL.CAM_PLAY) 
            {                
                //PlayProgressTimer.Stop();
                m_Cam1State = STATE_VAL.CAM_PLAY_PAUSE;
                TileState.Text = "Pause";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;                              
            }           
            else if((m_Cam1State == STATE_VAL.CAM_PLAY_PAUSE)|| (m_Cam1State == STATE_VAL.CAM_OPENED))
            {
                if ((m_Cam1State == STATE_VAL.CAM_PREVIEW) || (m_Cam2State == STATE_VAL.CAM_PREVIEW))
                {
                    btStop_Click(null, EventArgs.Empty);
                }
                
                nSpeed = cbPlaySpeed.SelectedIndex;
                SetPlaySpeed(nSpeed, m_nSaveFrameRate);

                if ((m_nSaveIndex1 == 0)||(m_nSaveIndex1 == m_Cam1SaveCount-1))
                {
                    m_LastDisplayIndex1 = 0;
                    m_LastDisplayIndex2 = 0;
                    m_nSaveIndex1 = 0;
                    m_nSaveIndex2 = 0;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                }                

                //PlayProgressTimer = new System.Windows.Forms.Timer();
                //PlayProgressTimer.Interval = 150; // 1초
                //PlayProgressTimer.Tick += Timer_Tick1;

                //// 타이머 시작
                //PlayProgressTimer.Start();

                if ((m_bCaptureFlag1) && (m_bCaptureFlag2))
                {
                    m_Cam1State = STATE_VAL.CAM_PLAY;
                    m_Cam2State = STATE_VAL.CAM_PLAY;
                }
                else if (m_bCaptureFlag1)
                {
                    m_Cam1State = STATE_VAL.CAM_PLAY;
                }
                else if (m_bCaptureFlag2)
                {
                    m_Cam2State = STATE_VAL.CAM_PLAY;
                }
                TileState.Text = "Play";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Stop;
            }
            else if (m_Cam1State == STATE_VAL.CAM_FILE_PLAY)
            {
                //PlayProgressTimer.Stop();
                m_Cam1State = STATE_VAL.CAM_FILE_PLAY_PAUSE;
                TileState.Text = "Pause";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
            }
            else if ((m_Cam1State == STATE_VAL.CAM_FILE_PLAY_PAUSE) || (m_Cam1State == STATE_VAL.CAM_FILE_PLAY_PAUSE2))
            {
                if (!video_file.IsOpened())
                {
                    MessageBox.Show("File Open Error");

                    if (m_MyCamera1 != null)
                    {
                        m_Cam1State = STATE_VAL.CAM_OPENED;
                    }
                    else 
                    {
                        m_Cam1State = STATE_VAL.CAM_IDLE;
                    }
                    tbPlay.Value = 0;
                    return;
                }
                                
                //PlayProgressTimer = new System.Windows.Forms.Timer();
                //PlayProgressTimer.Interval = 150; // 1초
                //PlayProgressTimer.Tick += Timer_Tick1;
                //PlayProgressTimer.Start();
               
                m_Cam1State = STATE_VAL.CAM_FILE_PLAY;
                TileState.Text = "Play ";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Stop;
            }
            else if (m_Cam2State == STATE_VAL.CAM_PLAY)
            {
                //PlayProgressTimer.Stop();
                m_Cam2State = STATE_VAL.CAM_PLAY_PAUSE;
                TileState.Text = "Pause";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
            }
            else if ((m_Cam2State == STATE_VAL.CAM_PLAY_PAUSE) || (m_Cam2State == STATE_VAL.CAM_OPENED))
            {
                if ((m_Cam2State == STATE_VAL.CAM_PREVIEW) || (m_Cam2State == STATE_VAL.CAM_PREVIEW))
                {
                    btStop_Click(null, EventArgs.Empty);
                }

                nSpeed = cbPlaySpeed.SelectedIndex;
                SetPlaySpeed(nSpeed, m_nSaveFrameRate);

                if ((m_nSaveIndex2 == 0) || (m_nSaveIndex2 == m_Cam2SaveCount - 1))
                {
                    m_LastDisplayIndex1 = 0;
                    m_LastDisplayIndex2 = 0;
                    m_nSaveIndex1 = 0;
                    m_nSaveIndex2 = 0;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                }

                //PlayProgressTimer = new System.Windows.Forms.Timer();
                //PlayProgressTimer.Interval = 150; // 1초
                //PlayProgressTimer.Tick += Timer_Tick1;

                //// 타이머 시작
                //PlayProgressTimer.Start();

                if ((m_bCaptureFlag1) && (m_bCaptureFlag2))
                {
                    m_Cam1State = STATE_VAL.CAM_PLAY;
                    m_Cam2State = STATE_VAL.CAM_PLAY;
                }
                else if (m_bCaptureFlag1)
                {
                    m_Cam1State = STATE_VAL.CAM_PLAY;
                }
                else if (m_bCaptureFlag2)
                {
                    m_Cam2State = STATE_VAL.CAM_PLAY;
                }
                TileState.Text = "Play";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Stop;
            }
            else if (m_Cam2State == STATE_VAL.CAM_FILE_PLAY)
            {
                //PlayProgressTimer.Stop();
                m_Cam2State = STATE_VAL.CAM_FILE_PLAY_PAUSE;
                TileState.Text = "Pause";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
            }
            else if ((m_Cam2State == STATE_VAL.CAM_FILE_PLAY_PAUSE) || (m_Cam2State == STATE_VAL.CAM_FILE_PLAY_PAUSE2))
            {
                if (!video_file.IsOpened())
                {
                    MessageBox.Show("File Open Error");

                    if (m_MyCamera2 != null)
                    {
                        m_Cam2State = STATE_VAL.CAM_OPENED;
                    }
                    else
                    {
                        m_Cam2State = STATE_VAL.CAM_IDLE;
                    }
                    tbPlay.Value = 0;
                    return;
                }

                //PlayProgressTimer = new System.Windows.Forms.Timer();
                //PlayProgressTimer.Interval = 150; // 1초
                //PlayProgressTimer.Tick += Timer_Tick1;
                //PlayProgressTimer.Start();

                m_Cam2State = STATE_VAL.CAM_FILE_PLAY;
                TileState.Text = "Play ";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Stop;
            }
        }

        private void tbPlay_Scroll(object sender, ScrollEventArgs e)
        {
            if (m_Cam1State == STATE_VAL.CAM_PLAY)
            {
                //PlayProgressTimer.Stop();
                m_Cam1State = STATE_VAL.CAM_OPENED;
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
            }
            else if (((m_Cam1State == STATE_VAL.CAM_OPENED) || (m_Cam1State == STATE_VAL.CAM_IDLE)) && m_bCaptureFlag1)
            {
                m_nSaveIndex1 = tbPlay.Value;
                m_Cam1State = STATE_VAL.CAM_PLAY_PAUSE;
                TileState.Text = "Pause";
            }
            else if (m_Cam1State == STATE_VAL.CAM_FILE_PLAY)
            {
                m_Cam1State = STATE_VAL.CAM_FILE_PLAY_PAUSE;
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
            }
            else if ((m_Cam1State == STATE_VAL.CAM_FILE_PLAY_PAUSE) || (m_Cam1State == STATE_VAL.CAM_FILE_PLAY_PAUSE2))
            {
                m_nSaveIndex1 = tbPlay.Value;
                m_Cam1State = STATE_VAL.CAM_FILE_PLAY_PAUSE;
                TileState.Text = "Pause";
            }
            else if (m_Cam2State == STATE_VAL.CAM_PLAY)
            {
                //PlayProgressTimer.Stop();
                m_Cam2State = STATE_VAL.CAM_OPENED;
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
            }
            else if (((m_Cam2State == STATE_VAL.CAM_OPENED) || (m_Cam2State == STATE_VAL.CAM_IDLE)) && m_bCaptureFlag2)
            {
                m_nSaveIndex2 = tbPlay.Value;
                m_Cam2State = STATE_VAL.CAM_PLAY_PAUSE;
                TileState.Text = "Pause";
            }
            else if (m_Cam2State == STATE_VAL.CAM_FILE_PLAY)
            {
                m_Cam2State = STATE_VAL.CAM_FILE_PLAY_PAUSE;
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Play;
            }
            else if ((m_Cam2State == STATE_VAL.CAM_FILE_PLAY_PAUSE) || (m_Cam2State == STATE_VAL.CAM_FILE_PLAY_PAUSE2))
            {
                m_nSaveIndex2 = tbPlay.Value;
                m_Cam2State = STATE_VAL.CAM_FILE_PLAY_PAUSE;
                TileState.Text = "Pause";
            }
        }

        private void GetFrameRate()
        {
            int nRet;
            float fps = 120.0f;
            if(m_MyCamera1 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();
                nRet = m_MyCamera1.GetFloatValue("ResultingFrameRate", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    tbFrameRate.Text = pcFloatValue.CurValue.ToString("F1");
                    fps = pcFloatValue.CurValue;
                }

                if (fps < 31)
                {
                    tbFrameRate.Value = 0;
                    lbFrameRate.Text = "Frame Rate 30";
                    m_Cam1SaveCount = 30 * 3;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                    m_DefaultInterval = 1;
                    m_nSaveFrameRate = 30;
                }
                else if (fps < 61)
                {
                    tbFrameRate.Value = 1;
                    lbFrameRate.Text = "Frame Rate 60";
                    m_Cam1SaveCount = 60 * 3;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                    m_DefaultInterval = 2;
                    m_nSaveFrameRate = 60;
                }
                else if (fps < 121)
                {
                    tbFrameRate.Value = 2;
                    lbFrameRate.Text = "Frame Rate 120";
                    m_Cam1SaveCount = 120 * 3;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                    m_DefaultInterval = 4;
                    m_nSaveFrameRate = 120;
                }
                else
                {
                    tbFrameRate.Value = 3;
                    lbFrameRate.Text = "Frame Rate 180";
                    m_Cam1SaveCount = 180 * 3;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                    m_DefaultInterval = 6;
                    m_nSaveFrameRate = 180;
                    bool bValue = true;
                    fps = 180.0f;
                    nRet = m_MyCamera1.SetBoolValue("AcquisitionFrameRateEnable", bValue);
                    nRet = m_MyCamera1.SetFloatValue("AcquisitionFrameRate", fps);
                    m_nSaveFrameRate = 180;

                }
            }            
        }

        private void SetFrameRate()
        {
            int nRet;
            float fps = 120.0f;
            if (m_MyCamera1 != null)
            {
                if (tbFrameRate.Value == 0)
                {
                    fps = 30.0f;
                    m_Cam1SaveCount = 30 * 3;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                    lbFrameRate.Text = "Frame Rate 30";
                    m_DefaultInterval = 1;
                    m_nSaveFrameRate = 30;
                }
                else if (tbFrameRate.Value == 1)
                {
                    fps = 60.0f;
                    m_Cam1SaveCount = 60 * 3;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                    lbFrameRate.Text = "Frame Rate 60";
                    m_DefaultInterval = 2;
                    m_nSaveFrameRate = 60;
                }
                else if (tbFrameRate.Value == 2)
                {
                    fps = 120.0f;
                    m_Cam1SaveCount = 120 * 3;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                    lbFrameRate.Text = "Frame Rate 120";
                    m_DefaultInterval = 4;
                    m_nSaveFrameRate = 120;
                }
                else
                {
                    fps = 180.0f;
                    m_Cam1SaveCount = 180 * 3;
                    tbPlay.Maximum = m_Cam1SaveCount - 1;
                    lbFrameRate.Text = "Frame Rate 180";
                    m_DefaultInterval = 6;
                    m_nSaveFrameRate = 180;
                }

                bool bValue = true;
                nRet = m_MyCamera1.SetBoolValue("AcquisitionFrameRateEnable", bValue);
                nRet = m_MyCamera1.SetFloatValue("AcquisitionFrameRate", fps);
            }                
        }


        private void GetFrameRateLE()
        {
            int nRet;
            float fps = 250.0f;
            if (m_MyCamera2 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();
                nRet = m_MyCamera2.GetFloatValue("ResultingFrameRate", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    tbFrameRate.Text = pcFloatValue.CurValue.ToString("F1");
                    fps = pcFloatValue.CurValue;
                }

                if (fps < 61)
                {
                    tbFrameRate.Value = 0;
                    lbFrameRate.Text = "Frame Rate 60";
                    m_Cam2SaveCount = 60 * 3;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                    m_DefaultInterval = 1;
                    m_nSaveFrameRate = 60;
                }
                else if (fps < 121)
                {
                    tbFrameRate.Value = 1;
                    lbFrameRate.Text = "Frame Rate 120";
                    m_Cam2SaveCount = 120 * 3;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                    m_DefaultInterval = 4;
                    m_nSaveFrameRate = 120;
                }
                else if (fps < 181)
                {
                    tbFrameRate.Value = 2;
                    lbFrameRate.Text = "Frame Rate 180";
                    m_Cam2SaveCount = 180 * 3;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                    m_DefaultInterval = 5;
                    m_nSaveFrameRate = 180;
                }
                else
                {
                    tbFrameRate.Value = 3;
                    lbFrameRate.Text = "Frame Rate 250";
                    m_Cam2SaveCount = 250 * 3;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                    m_DefaultInterval = 8;
                    m_nSaveFrameRate = 250;
                    bool bValue = true;
                    fps = 250.0f;
                    nRet = m_MyCamera2.SetBoolValue("AcquisitionFrameRateEnable", bValue);
                    nRet = m_MyCamera2.SetFloatValue("AcquisitionFrameRate", fps);
                    m_nSaveFrameRate = 250;

                }
            }
        }

        private void SetFrameRateLE()
        {
            int nRet;
            float fps = 120.0f;
            if (m_MyCamera2 != null)
            {
                if (tbFrameRate.Value == 0)
                {
                    fps = 60.0f;
                    m_Cam2SaveCount = 60 * 3;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                    lbFrameRate.Text = "Frame Rate 60";
                    m_DefaultInterval = 1;
                    m_nSaveFrameRate = 60;
                }
                else if (tbFrameRate.Value == 1)
                {
                    fps = 120.0f;
                    m_Cam2SaveCount = 120 * 3;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                    lbFrameRate.Text = "Frame Rate 120";
                    m_DefaultInterval = 4;
                    m_nSaveFrameRate = 120;
                }
                else if (tbFrameRate.Value == 2)
                {
                    fps = 180.0f;
                    m_Cam2SaveCount = 180 * 3;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                    lbFrameRate.Text = "Frame Rate 180";
                    m_DefaultInterval = 5;
                    m_nSaveFrameRate = 180;
                }
                else
                {
                    fps = 250.0f;
                    m_Cam2SaveCount = 250 * 3;
                    tbPlay.Maximum = m_Cam2SaveCount - 1;
                    lbFrameRate.Text = "Frame Rate 250";
                    m_DefaultInterval = 8;
                    m_nSaveFrameRate = 250;
                }

                bool bValue = true;
                nRet = m_MyCamera2.SetBoolValue("AcquisitionFrameRateEnable", bValue);
                nRet = m_MyCamera2.SetFloatValue("AcquisitionFrameRate", fps);
            }
        }
        private void tbFrameRate_Scroll(object sender, ScrollEventArgs e)
        {
            if (m_LEFlag == true)
            {
                SetFrameRateLE();
            }
            else
            {
                SetFrameRate();
            }
                
        }

        private void GetGain()
        {
            int nRet;
            float Gain = 1.0f;
            if (m_MyCamera1 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();

                nRet = m_MyCamera1.GetFloatValue("Gain", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    Gain = pcFloatValue.CurValue;
                }

                if (Gain < 0.1)
                {
                    tbISO.Value = 0;
                    lbISO.Text = "ISO 50";
                }
                else if (Gain < 10.1)
                {
                    tbISO.Value = 1;
                    lbISO.Text = "ISO 100";
                }
                else if (Gain < 15.1)
                {
                    tbISO.Value = 2;
                    lbISO.Text = "ISO 200";
                }
                else if (Gain < 20.1)
                {
                    tbISO.Value = 3;
                    lbISO.Text = "ISO 400";
                }
                else if (Gain < 25.1)
                {
                    tbISO.Value = 4;
                    lbISO.Text = "ISO 800";
                }
                else if (Gain < 30.1)
                {
                    tbISO.Value = 5;
                    lbISO.Text = "ISO 1600";
                }
                else if (Gain < 35.1)
                {
                    tbISO.Value = 6;
                    lbISO.Text = "ISO 3200";
                }
                else if (Gain < 40.1)
                {
                    tbISO.Value = 7;
                    lbISO.Text = "ISO 6400";
                }
                else
                {
                    tbISO.Value = 8;
                    lbISO.Text = "ISO 12800";
                }
            }
        }

        private void SetGain()
        {
            int nRet;
            float Gain = 20.0f;
            if (m_MyCamera1 != null)
            {
                if (tbISO.Value == 0)
                {
                    Gain = 0.0f;
                    lbISO.Text = "ISO 50";
                }
                else if (tbISO.Value == 1)
                {
                    Gain = 10.0f;
                    lbISO.Text = "ISO 100";
                }
                else if (tbISO.Value == 2)
                {
                    Gain = 15.0f;
                    lbISO.Text = "ISO 200";
                }
                else if (tbISO.Value == 3)
                {
                    Gain = 20.0f;
                    lbISO.Text = "ISO 400";
                }
                else if (tbISO.Value == 4)
                {
                    Gain = 25.0f;
                    lbISO.Text = "ISO 800";
                }
                else if (tbISO.Value == 5)
                {
                    Gain = 30.0f;
                    lbISO.Text = "ISO 1600";
                }
                else if (tbISO.Value == 6)
                {
                    Gain = 35.0f;
                    lbISO.Text = "ISO 3200";
                }
                else if (tbISO.Value == 7)
                {
                    Gain = 40.0f;
                    lbISO.Text = "ISO 6400";
                }
                else
                {
                    Gain = 45.0f;
                    lbISO.Text = "ISO 12800";
                }
                m_MyCamera1.SetEnumValue("GainAuto", 0);
                nRet = m_MyCamera1.SetFloatValue("Gain", Gain);
            }
        }


        private void GetGainLE()
        {
            int nRet;
            float Gain = 0.0f;
            if (m_MyCamera2 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();

                nRet = m_MyCamera2.GetFloatValue("Gain", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    Gain = pcFloatValue.CurValue;
                }

                if (Gain < 0.1)
                {
                    tbISO.Value = 0;
                    lbISO.Text = "ISO 50";
                }
                else if (Gain < 2.1)
                {
                    tbISO.Value = 1;
                    lbISO.Text = "ISO 100";
                }
                else if (Gain < 4.1)
                {
                    tbISO.Value = 2;
                    lbISO.Text = "ISO 200";
                }
                else if (Gain < 6.1)
                {
                    tbISO.Value = 3;
                    lbISO.Text = "ISO 400";
                }
                else if (Gain < 8.1)
                {
                    tbISO.Value = 4;
                    lbISO.Text = "ISO 800";
                }
                else if (Gain < 10.1)
                {
                    tbISO.Value = 5;
                    lbISO.Text = "ISO 1600";
                }
                else if (Gain < 12.1)
                {
                    tbISO.Value = 6;
                    lbISO.Text = "ISO 3200";
                }
                else if (Gain < 14.1)
                {
                    tbISO.Value = 7;
                    lbISO.Text = "ISO 6400";
                }
                else
                {
                    tbISO.Value = 8;
                    lbISO.Text = "ISO 12800";
                }
            }
        }

        private void SetGainLE()
        {
            int nRet;
            float Gain = 0.0f;
            if (m_MyCamera2 != null)
            {
                if (tbISO.Value == 0)
                {
                    Gain = 0.0f;
                    lbISO.Text = "ISO 50";
                }
                else if (tbISO.Value == 1)
                {
                    Gain = 2.0f;
                    lbISO.Text = "ISO 100";
                }
                else if (tbISO.Value == 2)
                {
                    Gain = 4.0f;
                    lbISO.Text = "ISO 200";
                }
                else if (tbISO.Value == 3)
                {
                    Gain = 6.0f;
                    lbISO.Text = "ISO 400";
                }
                else if (tbISO.Value == 4)
                {
                    Gain = 8.0f;
                    lbISO.Text = "ISO 800";
                }
                else if (tbISO.Value == 5)
                {
                    Gain = 10.0f;
                    lbISO.Text = "ISO 1600";
                }
                else if (tbISO.Value == 6)
                {
                    Gain = 12.0f;
                    lbISO.Text = "ISO 3200";
                }
                else if (tbISO.Value == 7)
                {
                    Gain = 14.0f;
                    lbISO.Text = "ISO 6400";
                }
                else
                {
                    Gain = 16.0f;
                    lbISO.Text = "ISO 12800";
                }
                m_MyCamera2.SetEnumValue("GainAuto", 0);
                nRet = m_MyCamera2.SetFloatValue("Gain", Gain);
            }
        }

        private void tbISO_Scroll(object sender, ScrollEventArgs e)
        {
            if (m_LEFlag == true)
            {
                SetGainLE();
            }
            else if(m_LEFlag == false)
            {
                SetGain();
            }                
        }

        private void GetExposureTime()
        {
            int nRet;
            float ExposureTime = 100.0f;
            if (m_MyCamera1 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();

                nRet = m_MyCamera1.GetFloatValue("ExposureTime", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    ExposureTime = pcFloatValue.CurValue;
                }

                if (ExposureTime < 101.0f)
                {
                    tbExposure.Value = 0;
                    lbExposure.Text = "Exposure Time 100us";
                }
                else if (ExposureTime < 501.0f)
                {
                    tbExposure.Value = 1;
                    lbExposure.Text = "Exposure Time 500us";
                }
                else if (ExposureTime < 1001.0f)
                {
                    tbExposure.Value = 2;
                    lbExposure.Text = "Exposure Time 1000us";
                }
                else if (ExposureTime < 2001.0f)
                {
                    tbExposure.Value = 3;
                    lbExposure.Text = "Exposure Time 2000us";
                }
                else if (ExposureTime < 3001.0f)
                {
                    tbExposure.Value = 4;
                    lbExposure.Text = "Exposure Time 3000us";
                }
                else if (ExposureTime < 4001.0f)
                {
                    tbExposure.Value = 5;
                    lbExposure.Text = "Exposure Time 4000us";
                }
                else
                {
                    tbExposure.Value = 6;
                    lbExposure.Text = "Exposure Time 5000us";
                }
            }
        }

        private void SetExposureTime()
        {
            int nRet;
            if (m_MyCamera1 != null)
            {
                float ExposureTime = 500.0f;
                if (tbExposure.Value == 0)
                {
                    ExposureTime = 100.0f;
                    lbExposure.Text = "Exposure Time 100us";
                }
                else if (tbExposure.Value == 1)
                {
                    ExposureTime = 500.0f;
                    lbExposure.Text = "Exposure Time 500us";
                }
                else if (tbExposure.Value == 2)
                {
                    ExposureTime = 1000.0f;
                    lbExposure.Text = "Exposure Time 1000us";
                }
                else if (tbExposure.Value == 3)
                {
                    ExposureTime = 2000.0f;
                    lbExposure.Text = "Exposure Time 2000us";
                }
                else if (tbExposure.Value == 4)
                {
                    ExposureTime = 3000.0f;
                    lbExposure.Text = "Exposure Time 3000us";
                }
                else if (tbExposure.Value == 5)
                {
                    ExposureTime = 4000.0f;
                    lbExposure.Text = "Exposure Time 4000us";
                }
                else
                {
                    ExposureTime = 5000.0f;
                    lbExposure.Text = "Exposure Time 5000us";
                }

                nRet = m_MyCamera1.SetFloatValue("ExposureTime", ExposureTime);
            }
        }

        private void GetExposureTimeLE()
        {
            int nRet;
            float ExposureTime = 100.0f;
            if (m_MyCamera2 != null)
            {
                CFloatValue pcFloatValue = new CFloatValue();

                nRet = m_MyCamera2.GetFloatValue("ExposureTime", ref pcFloatValue);
                if (CErrorDefine.MV_OK == nRet)
                {
                    ExposureTime = pcFloatValue.CurValue;
                }

                if (ExposureTime < 101.0f)
                {
                    tbExposure.Value = 0;
                    lbExposure.Text = "Exposure Time 100us";
                }
                else if (ExposureTime < 501.0f)
                {
                    tbExposure.Value = 1;
                    lbExposure.Text = "Exposure Time 500us";
                }
                else if (ExposureTime < 1001.0f)
                {
                    tbExposure.Value = 2;
                    lbExposure.Text = "Exposure Time 1000us";
                }
                else if (ExposureTime < 1501.0f)
                {
                    tbExposure.Value = 3;
                    lbExposure.Text = "Exposure Time 1500us";
                }
                else if (ExposureTime < 2001.0f)
                {
                    tbExposure.Value = 4;
                    lbExposure.Text = "Exposure Time 2000us";
                }
                else if (ExposureTime < 2501.0f)
                {
                    tbExposure.Value = 5;
                    lbExposure.Text = "Exposure Time 2500us";
                }
                else
                {
                    tbExposure.Value = 6;
                    lbExposure.Text = "Exposure Time 3000us";
                }
            }
        }

        private void SetExposureTimeLE()
        {
            int nRet;
            if (m_MyCamera2 != null)
            {
                float ExposureTime = 500.0f;
                if (tbExposure.Value == 0)
                {
                    ExposureTime = 100.0f;
                    lbExposure.Text = "Exposure Time 100us";
                }
                else if (tbExposure.Value == 1)
                {
                    ExposureTime = 500.0f;
                    lbExposure.Text = "Exposure Time 500us";
                }
                else if (tbExposure.Value == 2)
                {
                    ExposureTime = 1000.0f;
                    lbExposure.Text = "Exposure Time 1000us";
                }
                else if (tbExposure.Value == 3)
                {
                    ExposureTime = 1500.0f;
                    lbExposure.Text = "Exposure Time 1500us";
                }
                else if (tbExposure.Value == 4)
                {
                    ExposureTime = 2000.0f;
                    lbExposure.Text = "Exposure Time 2000us";
                }
                else if (tbExposure.Value == 5)
                {
                    ExposureTime = 2500.0f;
                    lbExposure.Text = "Exposure Time 2500us";
                }
                else
                {
                    ExposureTime = 3000.0f;
                    lbExposure.Text = "Exposure Time 3000us";
                }

                nRet = m_MyCamera2.SetFloatValue("ExposureTime", ExposureTime);
            }
        }

        private void tbExposure_Scroll(object sender, ScrollEventArgs e)
        {
            if(m_LEFlag == true)
            {
                SetExposureTimeLE();
            }
            else if(m_LEFlag == false)
            {
                SetExposureTime();
            }            
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if ((!m_bCaptureFlag1) && (!m_bCaptureFlag2))
            {
                MessageBox.Show("There is nothing to save.");
                return;
            }
            
            if ((m_Cam1State == STATE_VAL.CAM_PREVIEW) || (m_Cam2State == STATE_VAL.CAM_PREVIEW))
            {
                btStop_Click(null, EventArgs.Empty);
            }

            if (saveFileDialog1 == null)
            {
                saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                // 필요에 따라 대화 상자의 초기 설정을 변경할 수 있습니다.
                saveFileDialog1.Filter = "mp4 files (*.mp4)|*.mp4|All files (*.*)|*.*"; // 저장 가능한 파일 형식을 정의
                saveFileDialog1.FilterIndex = 1; // 기본 선택 파일 형식을 정의
                saveFileDialog1.RestoreDirectory = true; // 다음에 대화 상자를 열 때 마지막으로 열렸던 디렉토리를 복원
            }            

            if (saveFileDialog1.ShowDialog() == DialogResult.OK) // 사용자가 "Save" 버튼을 클릭하면
            {
                TileState.Text = "Save";

                if(m_LEFlag == false)
                {
                    int width = m_stCam1ImageInfo.nWidth;
                    int height = m_stCam1ImageInfo.nHeight;
                    int fps = m_nSaveFrameRate;
                    VideoWriter videoWriter = new VideoWriter(saveFileDialog1.FileName, VideoWriter.FourCC('m', 'p', '4', 'v'), fps, // 대소문자 구별해야함!!
                    new OpenCvSharp.Size(width, height), true);

                    if (false == videoWriter.IsOpened())
                    {
                        Console.WriteLine("Can't open VideoWriter");
                        TileState.Text = "File save Error";
                        return;
                    }
                    for (int i = 0; i < m_Cam1SaveCount; i++)
                    {
                        m_pOriginalImage1.SetArray(m_pSaveBuffer1[i]);
                        Cv2.CvtColor(m_pOriginalImage1, m_pDisplayImage1, ColorConversionCodes.BayerGR2BGR);
                        videoWriter.Write(m_pDisplayImage1);
                    }

                    videoWriter.Dispose();
                    TileState.Text = "File save complete";
                }
                else
                {
                    int width = m_stCam2ImageInfo.nWidth;
                    int height = m_stCam2ImageInfo.nHeight;
                    int fps = m_nSaveFrameRate;
                    VideoWriter videoWriter = new VideoWriter(saveFileDialog1.FileName, VideoWriter.FourCC('m', 'p', '4', 'v'), fps, // 대소문자 구별해야함!!
                    new OpenCvSharp.Size(width, height), true);

                    if (false == videoWriter.IsOpened())
                    {
                        Console.WriteLine("Can't open VideoWriter");
                        TileState.Text = "File save Error";
                        return;
                    }
                    for (int i = 0; i < m_Cam2SaveCount; i++)
                    {
                        m_pOriginalImage2.SetArray(m_pSaveBuffer2[i]);
                        Cv2.CvtColor(m_pOriginalImage2, m_pDisplayImage2, ColorConversionCodes.BayerBG2BGR);
                        videoWriter.Write(m_pDisplayImage2);
                    }
                    videoWriter.Dispose();
                    TileState.Text = "File save complete";
                }                
            }
        }

        private void tgFocusMode_CheckedChanged(object sender, EventArgs e)
        {
            m_FocusValue = 0;
            m_bFocusMode = tgFocusMode.Checked;
        }

        public int CalculateSumModifiedLaplacian(Mat mat, int nXStart, int nYStart, int nWidth, int nHeight)
        {
            int SML = 0;
            int LocalSML = 0;
            int nChannel = mat.Channels();
            int wBytes = nChannel * nWidth;

            for (int i = nYStart; i < nYStart + nHeight; i++)
            {
                for (int j = nXStart; j < nXStart + nWidth; j++)
                {
                    Vec3b pixel = mat.Get<Vec3b>(i, j);

                    LocalSML = Math.Abs((2 * pixel[0]) - (j > 0 ? mat.Get<Vec3b>(i, j - 1)[0] : pixel[0]) - (j < nWidth - 1 ? mat.Get<Vec3b>(i, j + 1)[0] : pixel[0]))
                               + Math.Abs((2 * pixel[0]) - (i > 0 ? mat.Get<Vec3b>(i - 1, j)[0] : pixel[0]) - (i < nHeight - 1 ? mat.Get<Vec3b>(i + 1, j)[0] : pixel[0]));
                    SML += LocalSML;
                }
            }

            return SML / 1000;
        }

        public void FocusTest(Mat mat)
        {
            int w = mat.Width;
            int h = mat.Height;

            int sx = w / 3;
            int sy = h / 4;
            w = sx;
            h = sy*2;

            OpenCvSharp.Point pt1 = new OpenCvSharp.Point(sx, sy);
            OpenCvSharp.Point pt2 = new OpenCvSharp.Point(sx + w, sy + h);

            // 이미지 위에 직사각형을 그립니다.
            Cv2.Rectangle(mat, pt1, pt2, Scalar.Red, 1, LineTypes.AntiAlias, 0);

            int CurrentFocus = 0;
            CurrentFocus = CalculateSumModifiedLaplacian(mat, sx, sy, w, h);
            if (CurrentFocus > m_FocusValue)
            {
                m_FocusValue = CurrentFocus;
            }

            OpenCvSharp.Point pt = new OpenCvSharp.Point(sx, sy + h + 50);
            Scalar color = Scalar.Red;
            HersheyFonts fontFace = HersheyFonts.HersheySimplex;
            double fontScale = 1.0;
            int thickness = 2;
            //Cv2.PutText(mat, $"Max Focus Value: {m_FocusValue}", pt, fontFace, fontScale, color, thickness);


            //pt = new OpenCvSharp.Point(sx, sy + h + 100);
            //color = Scalar.Green;
            Cv2.PutText(mat, $"Current Focus Value: {CurrentFocus}", pt, fontFace, fontScale, color, thickness);
            
        }

        private void TileState_Click(object sender, EventArgs e)
        {
            if ((m_nValidCamNum == 0) && (m_Cam1State == STATE_VAL.CAM_IDLE))
            {
                DeviceListAcq();
                if (m_nValidCamNum > 0)
                {
                    ThreadCallbackStart();
                    MemoryInitialize();
                }
                InitializeContents();
            }
        }

        private void cbPlaySpeed_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btFilePlay_Click(object sender, EventArgs e)
        {
            tgFocusMode.Checked = false;
            tgFocusMode.Enabled = false;
            if ((m_Cam1State == STATE_VAL.CAM_PREVIEW) || (m_Cam2State == STATE_VAL.CAM_PREVIEW))
            {
                btStop_Click(null, EventArgs.Empty);
            }

            if (openFileDialog == null)
            {
                openFileDialog = new OpenFileDialog();
                if(saveFileDialog1 != null)
                {
                    openFileDialog.InitialDirectory = saveFileDialog1.InitialDirectory;
                }
                else
                {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }

                // 필요에 따라 대화 상자의 초기 설정을 변경할 수 있습니다.
                openFileDialog.Filter = "mp4 files (*.mp4)|*.mp4|All files (*.*)|*.*"; // 저장 가능한 파일 형식을 정의
                openFileDialog.FilterIndex = 1; // 기본 선택 파일 형식을 정의
                openFileDialog.RestoreDirectory = true; // 다음에 대화 상자를 열 때 마지막으로 열렸던 디렉토리를 복원
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK) // 사용자가 "Save" 버튼을 클릭하면
            {
                double filefps;
                int nSpeed;

                video_file = new VideoCapture(openFileDialog.FileName);
                if (!video_file.IsOpened())
                {
                    MessageBox.Show("File Open Error");
                    return;
                }

                m_bCaptureFlag1 = true;
                m_bCaptureFlag2 = true;
                m_Cam1SaveCount = video_file.FrameCount;
                m_Cam2SaveCount = video_file.FrameCount;
                filefps = video_file.Fps;
                nSpeed = cbPlaySpeed.SelectedIndex;
                SetPlaySpeed(nSpeed, (int)filefps);
                tbPlay.Maximum = m_Cam1SaveCount - 1;
                tbPlay.Value = 0;
                m_nSaveIndex1 = 0;
                m_nSaveIndex2 = 0;
                //PlayProgressTimer = new System.Windows.Forms.Timer();
                //PlayProgressTimer.Interval = 150; // 1초
                //PlayProgressTimer.Tick += Timer_Tick1;
                //PlayProgressTimer.Start();
                m_LastDisplayIndex1 = 0;
                m_nSaveIndex1 = 0;
                m_LastDisplayIndex2 = 0;
                m_nSaveIndex2 = 0;

                if(m_LEFlag == false)
                {
                    m_Cam1State = STATE_VAL.CAM_FILE_PLAY;
                }
                else
                {
                    m_Cam2State = STATE_VAL.CAM_FILE_PLAY;
                }
                
                TileState.Text = "Play File";
                btPlayMini.BackgroundImage = A_Scout_Viewer.Properties.Resources.Stop;
            }
        }
    }

    public static class Constants
    {
        public const int CAM1_WIDTH = 1600;
        public const int CAM1_HEIGHT = 1100;
        public const int CAM2_WIDTH = 1440;
        public const int CAM2_HEIGHT = 1080;
        public const int MEM_BUFFER_MARGIN = 1024;

        public const int CAM1_SAVE_COUNT = 185 * 3 ;
        public const int CAM2_SAVE_COUNT = 250 * 3;
        public const double FRAME_RATIO = 1;
        public const int MAX_SAVE_FOLDER = 100;
        public const int CAM1_FPS = 120;
        public const int CAM2_FPS = 250;

        public const int CAM1_DEFAULT_INTERVAL = 1;
        public const int CAM2_DEFAULT_INTERVAL = 1;
      
    }    
}
