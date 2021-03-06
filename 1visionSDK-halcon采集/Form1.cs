﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HalconDotNet;



namespace _1visionSDK_halcon采集
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //*设置窗口颜色为黑色
        //1打开窗口(dev_open_window之后自带)
        //*设置为活跃窗口
        //2打开相机（halcon助手写的）
        //*设置采集模式为灰度图
        //*抓图一张
        //3.获取图像尺寸
        //4设置窗口宽高
        //5.显示对象到窗口上
        #region  halcon variable 
        HObject ho_Image = null;
        HTuple hv_WindowHandle = null, hv_AcqHandle = null;
        HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
        
        #endregion
        private void button1_Click(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                timer1.Enabled = true;
            }
            else
            {
                MessageBox.Show("相机已打开");
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            HOperatorSet.CloseFramegrabber(hv_AcqHandle);
            timer1.Enabled = false;         
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            HOperatorSet.CloseFramegrabber(hv_AcqHandle);
            timer1.Enabled = false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            //1设置窗口颜色为黑色
            //2打开窗口
            //3设置为活跃窗口
            //4打开相机
            //5设置采集模式为灰度图
            HOperatorSet.SetWindowAttr("background_color", "black");
            HOperatorSet.OpenWindow(0, 0, pictureBox1.Width, pictureBox1.Height, pictureBox1.Handle, "", "", out hv_WindowHandle);
            HDevWindowStack.Push(hv_WindowHandle);
            //Image Acquisition 01: Code generated by Image Acquisition 01
            HOperatorSet.OpenFramegrabber("MindVision12_X64", 1, 1, 0, 0, 0, 0, "progressive",
                8, "Gray", -1, "false", "auto", "camera1", 0, -1, out hv_AcqHandle);
            HOperatorSet.GrabImageStart(hv_AcqHandle, -1);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            HOperatorSet.GrabImageAsync(out ho_Image, hv_AcqHandle, -1);
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);          
            HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_Width, hv_Height);
            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
            ho_Image.Dispose();
        }
    }
}
