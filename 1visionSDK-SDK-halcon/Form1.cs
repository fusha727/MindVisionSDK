using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HalconDotNet;
using MVSDK;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;

using System.Runtime.InteropServices;


namespace _1visionSDK_SDK_halcon
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region camera variable
        protected CameraHandle m_hCamera = 0;             // 句柄
        protected IntPtr m_ImageBuffer;             // 预览通道RGB图像缓存
        protected IntPtr m_ImageBufferSnapshot;     // 抓拍通道RGB图像缓存
        protected tSdkCameraCapbility tCameraCapability;  // 相机特性描述
        protected int m_iDisplayedFrames = 0;    //已经显示的总帧数
        protected CAMERA_SNAP_PROC m_CaptureCallback;
        protected IntPtr m_iCaptureCallbackCtx;     //图像回调函数的上下文参数
        protected IntPtr m_iSettingPageMsgCallbackCtx; //相机配置界面消息回调函数的上下文参数   
        protected tSdkFrameHead m_tFrameHead;
        CameraSdkStatus status;
        tSdkCameraDevInfo[] tCameraDevInfoList;
        CAMERA_SNAP_PROC pCaptureCallOld = null;
        #endregion

        #region halcon variable
        HObject ho_Image = null;
        HTuple hv_WindowHandle = null, hv_AcqHandle = null;
        HObject halcon_image;
        HTuple hv_Width = null, hv_Height = null;
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            //1打开窗口 
            //2枚举设备
            //3初始化设备
            //4.获得位数，申请内存
            //5绑定回调事件  并在回调事件中加入halcon显示代码
            //6开始采集（默认连续，不设置采集模式的话）
            HOperatorSet.OpenWindow(0, 0, pictureBox1.Width, pictureBox1.Height, pictureBox1.Handle, "", "", out hv_WindowHandle);
            HDevWindowStack.Push(hv_WindowHandle);
            status = MvApi.CameraEnumerateDevice(out tCameraDevInfoList);
            if (tCameraDevInfoList != null)//此时iCameraCounts返回了实际连接的相机个数。如果大于1，则初始化第一个相机
            {
                status = MvApi.CameraInit(ref tCameraDevInfoList[0], -1, -1, ref m_hCamera);
            }
            MvApi.CameraGetCapability(m_hCamera, out tCameraCapability);

            m_ImageBuffer = Marshal.AllocHGlobal(tCameraCapability.sResolutionRange.iWidthMax * tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);

            m_CaptureCallback = new CAMERA_SNAP_PROC(ImageCaptureCallback);
            MvApi.CameraSetCallbackFunction(m_hCamera, m_CaptureCallback, m_iCaptureCallbackCtx, ref pCaptureCallOld);
            MvApi.CameraPlay(m_hCamera);
        }

        public void ImageCaptureCallback(CameraHandle hCamera, IntPtr pFrameBuffer, ref tSdkFrameHead pFrameHead, IntPtr pContext)
        {
            MvApi.CameraImageProcess(hCamera, pFrameBuffer, m_ImageBuffer, ref pFrameHead);
            int bytewidth = (pFrameHead.iWidth * 3 + 3) / 4 * 4;//要设置的RGB图 的宽度  保证其是4的倍数
            int bytewidthg = (pFrameHead.iWidth + 3) / 4 * 4;//要设置分量图
            //实例化申请内存空间
            byte[] m_pImageData = new byte[bytewidth * pFrameHead.iHeight];
            byte[] m_pImageDataR = new byte[pFrameHead.iWidth * pFrameHead.iHeight];
            byte[] m_pImageDataG = new byte[pFrameHead.iWidth * pFrameHead.iHeight];
            byte[] m_pImageDataB = new byte[pFrameHead.iWidth * pFrameHead.iHeight];
            //复制一张包含RGB的图像缓存（将RGB图像转换成Byte图像缓存）  RGB图像缓存，图像byte的图像类型，相机的索引，图像的字节数大小
            Marshal.Copy(m_ImageBuffer, m_pImageData, 0, pFrameHead.iWidth * pFrameHead.iHeight * 3);
            for (int j = 0; j < pFrameHead.iHeight; j++)
            {
                for (int i = 0; i < pFrameHead.iWidth; i++)
                {
                    //将图像的RGB分量 分别分给R分量图，G分量图，B分量图
                    m_pImageDataB[j * bytewidthg + i] = m_pImageData[j * bytewidth + i * 3 + 0];
                    m_pImageDataG[j * bytewidthg + i] = m_pImageData[j * bytewidth + i * 3 + 1];
                    m_pImageDataR[j * bytewidthg + i] = m_pImageData[j * bytewidth + i * 3 + 2];

                    // m_pImageDatagray[j * bytewidthg + i] = (byte)(0.11 * m_pImageData[j * bytewidth + i * 3 + 0] + 0.59 * m_pImageData[j * bytewidth + i * 3 + 1] + 0.30 * m_pImageData[j * bytewidth + i * 3 + 2]);
                }
            }
            unsafe
            {
                fixed (byte* pR = m_pImageDataR, pB = m_pImageDataB, pG = m_pImageDataG)
                {
                    HOperatorSet.GenImage3(out halcon_image, "byte", pFrameHead.iWidth, pFrameHead.iHeight, new IntPtr(pR), new IntPtr(pB), new IntPtr(pG));
                }
            }
            //获得图像宽高，然后设置显示图像的宽高
            HOperatorSet.GetImageSize(halcon_image, out hv_Width, out hv_Height);
            HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_Height, hv_Width);
            //如果图像反向可以用halcon镜像图像的函数 将图像镜像反转一下
            //HOperatorSet.MirrorImage(halcon_image, out halcon_image, "row");
            HOperatorSet.DispObj(halcon_image, hv_WindowHandle);
            halcon_image.Dispose();      
         }
        }
}
