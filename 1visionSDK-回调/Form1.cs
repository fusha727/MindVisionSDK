using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MVSDK;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;


namespace _1visionSDK_回调
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        #region variable
        protected CameraHandle m_hCamera = 0;             // 句柄
        protected IntPtr m_ImageBuffer;             // 预览通道RGB图像缓存
        protected IntPtr m_ImageBufferSnapshot;     // 抓拍通道RGB图像缓存
        protected tSdkCameraCapbility tCameraCapability;  // 相机特性描述
        protected int m_iDisplayedFrames = 0;    //已经显示的总帧数
        protected CAMERA_SNAP_PROC m_CaptureCallback;
        protected IntPtr m_iCaptureCallbackCtx;     //图像回调函数的上下文参数
        protected tSdkFrameHead m_tFrameHead;
        //显示抓拍图像的窗口
        protected bool m_bEraseBk = false;
        protected bool m_bSaveImage = false;
        CameraSdkStatus status;     //相机SDK的状态  
        tSdkCameraDevInfo[] tCameraDevInfoList;
        CAMERA_SNAP_PROC pCaptureCallOld;
        #endregion

        //1.枚举设备
        //2.初始化相机
        //3.申请内存
        //4.抓图模式的设置-回调函数、开线程。（copy对应函数）--绑定窗口
        //5.显示模式（单次采集、实时采集、硬触发）
        //  软触发：设置按钮，按一次触发一次。（单次采集、实时采集）

        private void Form1_Load(object sender, EventArgs e)
        {
            //1.枚举设备
            status = MvApi.CameraEnumerateDevice(out tCameraDevInfoList);

            //2.初始化设备
            if (tCameraDevInfoList != null)//此时iCameraCounts返回了实际连接的相机个数。如果大于1，则初始化第一个相机
            {
                status = MvApi.CameraInit(ref tCameraDevInfoList[0], -1, -1, ref m_hCamera);
            }
            //3.申请内存
            //MvApi.CameraGetCapability(m_hCamera, out tCameraCapability);
            //m_ImageBuffer = Marshal.AllocHGlobal(tCameraCapability.sResolutionRange.iWidthMax * tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);
            //初始化显示模块，使用SDK内部封装好的显示接口
            MvApi.CameraDisplayInit(m_hCamera, pictureBox1.Handle);
            MvApi.CameraSetDisplaySize(m_hCamera, pictureBox1.Width, pictureBox1.Height);
            //4.  回调函数  #if USE_CALL_BACK
            m_CaptureCallback = new CAMERA_SNAP_PROC(ImageCaptureCallback);
            MvApi.CameraSetCallbackFunction(m_hCamera, m_CaptureCallback, m_iCaptureCallbackCtx, ref pCaptureCallOld);
            //5.显示模式（单次采集、实时采集、硬触发）
            MvApi.CameraPlay(m_hCamera);
        }

        public void ImageCaptureCallback(CameraHandle hCamera, IntPtr pFrameBuffer, ref tSdkFrameHead pFrameHead, IntPtr pContext)
        {
            MvApi.CameraDisplayRGB24(hCamera, pFrameBuffer, ref pFrameHead);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            MvApi.CameraPlay(m_hCamera);
        }


 

    }
}
