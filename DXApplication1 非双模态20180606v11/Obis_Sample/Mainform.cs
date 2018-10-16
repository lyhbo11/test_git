using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Obis_Sample.Function;
using Obis_Sample.Nets;
using Obis_New;
using Obis_Sample.Patients;
using DevExpress.XtraBars.Ribbon;
using DevExpress.Utils.Drawing;
using System.IO.Ports;
using Hardward;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;

namespace Obis_Sample
{
    public partial class Mainform : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        NetFunction NetModel = new NetFunction();
        string PatientNow_ID;
        Patient PaitentNow;
        public GalleryItemGroup group1 = new GalleryItemGroup();
        private string ms_cameraMode;
        private uint xxx;
        private bool mb_statusInit;
        private bool mb_isCameraOpen;
        private bool mb_isUartOpen;
        private Lucam lucamControl0 = new Lucam();
        private Lucam lucamControl1 = new Lucam();
        static int D_count = 0;
        private SerialPort m_serialPort = null; //声明一个串口类
        private AgDev.AgDev_CCDParam ccd = default(AgDev.AgDev_CCDParam);
        private AgDev.AgDev_BgLamp lamp = default(AgDev.AgDev_BgLamp);
        // 它的 pos_bitmap 第2位为1： Mono, auto ; 为0: color auto
        // pos_bitmap  （第1位为0：左眼；1：右眼） 
        // 它的 key_bitmap （第一位：1：照相模式； 0：摄像模式）（第3位： 1开始录像，0停止录像）
        private AgDev.AgDev_Status ss = default(AgDev.AgDev_Status);
        public string LogAddress;
        //private bool blIsDrawRectangle = false;
        //const int WM_NCLBUTTONDOWN = 0x00A1;//固定标题栏
        //const int HTCAPTION = 2;
        //protected override void WndProc(ref Message m)
        //{
        //    if (m.Msg == WM_NCLBUTTONDOWN && m.WParam.ToInt32() == HTCAPTION)
        //        return;
        //    base.WndProc(ref m);
        //}
        private unsafe AgDev.AgDevSnapRecall rc = new AgDev.AgDevSnapRecall(AgDev.AgDev_RecallProcess);
        private int DETIAL_NOW;
        public string UID_NOW;
        public int Left_count = 0;
        public int Right_count = 0;
        private int EYE_SIGHT = 1;
        private int TEMP = 0;

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private System.Windows.Forms.Timer timer4;

        private bool OpenCamera()//打开相机
        {
            try
            {
                this.xxx = AgDev.AgDev_Init(0u, 0u, 0u, 0u, 0u);
            }
            catch (Exception ex)
            {
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：打开相机失败");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
            }
            if (this.xxx == 0u)
            {
                this.mb_statusInit = true;
                IntPtr handle = this.pictureBox1.Handle;
                this.xxx = AgDev.AgDev_InitCamera(handle, IntPtr.Zero, handle, IntPtr.Zero);
                if (this.xxx == 0u)
                {
                    mb_isCameraOpen = true;
                    return true;
                }
            }
            return false;
        }

        private void SetPortProperty()//设置串口的属性
        {
            //if (toolStripComboBox2.Text.ToString().Trim() != "COM1")
            //{
            m_serialPort = new SerialPort();
            m_serialPort.PortName = this.barEditItem2.EditValue.ToString().Trim();//设置串口名
            m_serialPort.BaudRate = Convert.ToInt32("9600");//设置串口的波特率
            float f = Convert.ToSingle("1");//设置停止位
            if (f == 0)
            {
                m_serialPort.StopBits = StopBits.None;
            }
            else if (f == 1.5)
            {
                m_serialPort.StopBits = StopBits.OnePointFive;
            }
            else if (f == 1)
            {
                m_serialPort.StopBits = StopBits.One;
            }
            else if (f == 2)
            {
                m_serialPort.StopBits = StopBits.Two;
            }
            else
            {
                m_serialPort.StopBits = StopBits.One;
            }
            m_serialPort.DataBits = Convert.ToInt16("8");//设置数据位
            string s = "无"; //设置奇偶校验位
            if (s.CompareTo("无") == 0)
            {
                m_serialPort.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                m_serialPort.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                m_serialPort.Parity = Parity.Even;
            }
            else
            {
                m_serialPort.Parity = Parity.None;
            }
            m_serialPort.ReadTimeout = -1;//设置超时读取时间
            m_serialPort.RtsEnable = true;
            //定义DataReceived 事件，当串口收到数据后触发事件
            m_serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
        }
        private void DeviceUpdate(string fileName)
        {
            int[,] array = new int[32, 2];
            string section = "Mono";
            StringBuilder stringBuilder = new StringBuilder(255);
            for (int i = 0; i < 26; i++)
            {
                this.ccd.prop = i;
                AgDev.AgDev_GetCCDParamRang(1u, ref this.ccd);
                AgDev.AgDev_GetCCDParam(1u, ref this.ccd);
                array[i, 0] = this.ccd.val;
                array[i, 1] = this.ccd.flag;
            }

            if (array[(int)((UIntPtr)6), (int)((UIntPtr)1)] != 3)
            {
                string key = "Color";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 6;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)20), (int)((UIntPtr)1)] != 3)
            {
                string key = "vFlip";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 20;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)21), (int)((UIntPtr)1)] != 3)
            {
                string key = "hFlip";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 21;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)5), (int)((UIntPtr)1)] != 3)
            {
                string key = "Gamma";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 5;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)1), (int)((UIntPtr)1)] != 3)
            {
                string key = "Contrat";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 1;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)3), (int)((UIntPtr)1)] != 3)
            {
                string key = "Saturation";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 3;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)2), (int)((UIntPtr)1)] != 3)
            {
                string key = "Tonal";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 2;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)4), (int)((UIntPtr)1)] != 3)
            {
                string key = "Sharpen";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 4;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)0), (int)((UIntPtr)1)] != 3)
            {
                string key = "Brightness";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 0;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)10), (int)((UIntPtr)1)] != 3)
            {
                string key = "ExpoTime";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 10;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)9), (int)((UIntPtr)1)] != 3)
            {
                string key = "Gain";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 9;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)11), (int)((UIntPtr)1)] != 3)
            {
                string key = "rGain";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 11;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)12), (int)((UIntPtr)1)] != 3)
            {
                string key = "gGain";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 12;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)13), (int)((UIntPtr)1)] != 3)
            {
                string key = "bGain";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 13;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)10), (int)((UIntPtr)1)] != 3)
            {
                string key = "ExpoTime";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 10;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)17), (int)((UIntPtr)1)] != 3)
            {
                string key = "bWBalance";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 17;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)16), (int)((UIntPtr)1)] != 3)
            {
                string key = "gWBalance";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 16;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)15), (int)((UIntPtr)1)] != 3)
            {
                string key = "rWBalance";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 15;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)19), (int)((UIntPtr)1)] != 3)
            {
                string key = "SoftAperture";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 19;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)22), (int)((UIntPtr)1)] != 3)
            {
                string key = "ContrastExEnable";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 22;
                this.ccd.flag = int.Parse(stringBuilder.ToString());
                key = "ContrastEx";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)23), (int)((UIntPtr)1)] != 3)
            {
                string key = "BrightExEnable";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 23;
                this.ccd.flag = int.Parse(stringBuilder.ToString());
                key = "BrightEx";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)24), (int)((UIntPtr)1)] != 3)
            {
                string key = "RGBExEnable";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.flag = int.Parse(stringBuilder.ToString());
                key = "rEx";
                this.ccd.prop = 26;
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
                key = "gEx";
                this.ccd.prop = 25;
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
                key = "bEx";
                this.ccd.prop = 24;
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(1u, ref this.ccd);
            }
            section = "Color";
            for (int j = 0; j < 26; j++)
            {
                this.ccd.prop = j;
                AgDev.AgDev_GetCCDParamRang(2u, ref this.ccd);
                AgDev.AgDev_GetCCDParam(2u, ref this.ccd);
                array[j, 0] = this.ccd.val;
                array[j, 1] = this.ccd.flag;
            }
            if (array[(int)((UIntPtr)6), (int)((UIntPtr)1)] != 3)
            {
                string key = "Color";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 6;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)20), (int)((UIntPtr)1)] != 3)
            {
                string key = "vFlip";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 20;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)21), (int)((UIntPtr)1)] != 3)
            {
                string key = "hFlip";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 21;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)5), (int)((UIntPtr)1)] != 3)
            {
                string key = "Gamma";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 5;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)1), (int)((UIntPtr)1)] != 3)
            {
                string key = "Contrat";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 1;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)3), (int)((UIntPtr)1)] != 3)
            {
                string key = "Saturation";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 3;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)2), (int)((UIntPtr)1)] != 3)
            {
                string key = "Tonal";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 2;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)4), (int)((UIntPtr)1)] != 3)
            {
                string key = "Sharpen";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 4;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)0), (int)((UIntPtr)1)] != 3)
            {
                string key = "Brightness";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 0;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)10), (int)((UIntPtr)1)] != 3)
            {
                string key = "ExpoTime";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 10;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)9), (int)((UIntPtr)1)] != 3)
            {
                string key = "Gain";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 9;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)11), (int)((UIntPtr)1)] != 3)
            {
                string key = "rGain";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 11;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)12), (int)((UIntPtr)1)] != 3)
            {
                string key = "gGain";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 12;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)13), (int)((UIntPtr)1)] != 3)
            {
                string key = "bGain";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 13;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)10), (int)((UIntPtr)1)] != 3)
            {
                string key = "ExpoTime";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 10;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            Console.WriteLine(this.ccd.val.ToString());
            if (array[(int)((UIntPtr)17), (int)((UIntPtr)1)] != 3)
            {
                string key = "bWBalance";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 17;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)16), (int)((UIntPtr)1)] != 3)
            {
                string key = "gWBalance";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 16;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)15), (int)((UIntPtr)1)] != 3)
            {
                string key = "rWBalance";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 15;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)19), (int)((UIntPtr)1)] != 3)
            {
                string key = "SoftAperture";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 19;
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)22), (int)((UIntPtr)1)] != 3)
            {
                string key = "ContrastExEnable";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 22;
                this.ccd.flag = int.Parse(stringBuilder.ToString());
                key = "ContrastEx";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)23), (int)((UIntPtr)1)] != 3)
            {
                string key = "BrightExEnable";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.prop = 23;
                this.ccd.flag = int.Parse(stringBuilder.ToString());
                key = "BrightEx";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            if (array[(int)((UIntPtr)24), (int)((UIntPtr)1)] != 3)
            {
                string key = "RGBExEnable";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.flag = int.Parse(stringBuilder.ToString());
                key = "rEx";
                this.ccd.prop = 26;
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
                key = "gEx";
                this.ccd.prop = 25;
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
                key = "bEx";
                this.ccd.prop = 24;
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.ccd.val = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetCCDParam(2u, ref this.ccd);
            }
            section = "Lamp";
            this.lamp.lamp_idx = 48;
            AgDev.AgDev_GetBgLamp(ref this.lamp);
            array[31, 0] = this.lamp.bright;
            array[31, 1] = this.lamp.flag;
            if (array[31, 1] != 3)
            {
                string key = "Flash"; // 设置闪光灯
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.lamp.bright = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetBgLamp(ref this.lamp);
            }
            this.lamp.lamp_idx = 16;
            AgDev.AgDev_GetBgLamp(ref this.lamp);
            array[29, 0] = this.lamp.bright;
            array[29, 1] = this.lamp.flag;
            if (array[29, 1] != 3)
            {
                string key = "rLight";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.lamp.bright = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetBgLamp(ref this.lamp);
            }
            this.lamp.lamp_idx = 32;
            AgDev.AgDev_GetBgLamp(ref this.lamp);
            array[30, 0] = this.lamp.bright;
            array[30, 1] = this.lamp.flag;
            if (array[30, 1] != 3)
            {
                string key = "bLight";
                GetPrivateProfileString(section, key, "", stringBuilder, 255, fileName);
                this.lamp.bright = int.Parse(stringBuilder.ToString());
                AgDev.AgDev_SetBgLamp(ref this.lamp);
            }
        }

        private void timer4_Tick(object sender, EventArgs e)//获取硬件信息
        {
            AgDev.AgDev_GetStatus(ref this.ss);
            if ((this.ss.pos_bitmap & 1u) == 0u)//通过硬件信息判断左右眼
            {
                this.barButtonItem7.Down = true;
                this.barButtonItem8.Down = false;
                EYE_SIGHT = 1;
            }
            else
            {
                this.barButtonItem8.Down = true;
                this.barButtonItem7.Down = false;
                EYE_SIGHT = 0;
            }
            bool flag = (this.ss.key_bitmap & 1u) > 0u;
            bool flag2 = (this.ss.key_bitmap & 4u) > 0u;
            if (flag)//判断是否通过硬件触发拍照
            {
                if (mb_isCameraOpen)
                {
                    AgDev.AgDev_GetStatus(ref this.ss);
                    this.Take("Color");
                }
                else if (!mb_isCameraOpen)
                {
                    MessageBox.Show("请先连接到相机,或打开正确的串口,");
                    return;
                }
            }
        }

        private void Take(string mode)//拍照
        {
            string type = "";
            string eye = "";
            if (this.PatientNow_ID == "")
            {
                MessageBox.Show("请选择患者");
            }
            else
            {
                uint num;
                if (EYE_SIGHT == 1)
                {
                    eye = "Left";
                    Left_count++;
                    num = (uint)Left_count;
                }
                else
                {
                    eye = "Right";
                    Right_count++;
                    num = (uint)Right_count;
                }
                string name = UID_NOW + "_" + type + "_" + eye + "_";
                string result = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\";
                string patient_path = result + "Data\\";//检查data文件夹
                string ori_name = name + num + "_origin";
                name = ori_name + ".jpg";
                string path = result + "Data\\" + UID_NOW + "\\" + name;
                string doc_path = result + "Doctor\\" + UID_NOW + "\\" + name;
                //Thread t1 = new Thread(new ParameterizedThreadStart(lucamControl0.TakingPhoto));
                //Thread t2 = new Thread(new ParameterizedThreadStart(lucamControl1.TakingPhoto));
                //name = ori_name + "_hcam0" + ".jpg";
                //path = result + "Data\\" + UID_NOW + "\\" + name;
                //t1.Start(path);
                //name = ori_name + "_hcam1" + ".jpg";
                //path = result + "Data\\" + UID_NOW + "\\" + name;
                //t2.Start(path);
                //name = ori_name + ".jpg";
                //path = result + "Data\\" + UID_NOW + "\\" + name;
                doc_path = result + "Doctor\\" + UID_NOW + "\\" + name;
                uint retnum = AgDev.AgDev_SnapImage(path, 100u);
                if (retnum == 0u)
                {
                    FileStream fileStream = new FileStream(path, FileMode.Open);
                    //FileStream fileStream2 = new FileStream(".\\Data\\" + UID_NOW + "\\" + te[te.Length - 1], FileMode.Open);
                    Image temp = Image.FromStream(fileStream);
                    temp.Save(doc_path);
                    fileStream.Close();
                    GalleryItem item = new GalleryItem(temp, eye, num.ToString());
                    item.Tag = path;
                    group1.Items.Add(item);
                }
                //try
                //{
                //    name = ori_name + "_hcam0" + ".jpg";
                //    path = result + "Data\\" + UID_NOW + "\\" + name;
                //    t1.Join();
                //    FileStream fileStream1 = new FileStream(path, FileMode.Open);
                //    Image temp1 = Image.FromStream(fileStream1);
                //    fileStream1.Close();
                //    GalleryItem item = new GalleryItem(temp1, eye, num.ToString() + "hcam0");
                //    item.Tag = path;
                //    group1.Items.Add(item);
                //    t1.Abort();
                //}
                //catch(Exception ex)
                //{
                //    MessageBox.Show("拍摄异常，请联系技术人员检查相关日志内容");
                //    using (StreamWriter fs = new StreamWriter(LogAddress, true))
                //    {
                //        fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                //        fs.WriteLine("异常类型：双模态相机1拍摄异常");
                //        fs.WriteLine("异常信息:" + ex.Message);
                //        fs.WriteLine("异常对象：" + ex.Source);
                //        fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                //        fs.WriteLine("触发方法：" + ex.TargetSite);
                //        fs.WriteLine();
                //        fs.Close();
                //    }
                //}
                //try
                //{
                //    name = ori_name + "_hcam1" + ".jpg";
                //    path = result + "Data\\" + UID_NOW + "\\" + name;
                //    t2.Join();
                //    FileStream fileStream1 = new FileStream(path, FileMode.Open);
                //    Image temp1 = Image.FromStream(fileStream1);
                //    fileStream1.Close();
                //    GalleryItem item = new GalleryItem(temp1, eye, num.ToString() + "hcam1");
                //    item.Tag = path;
                //    group1.Items.Add(item);
                //    t2.Abort();
                //}
                //catch(Exception ex)
                //{
                //    MessageBox.Show("拍摄异常，请联系技术人员检查相关日志内容");
                //    using (StreamWriter fs = new StreamWriter(LogAddress, true))
                //    {
                //        fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                //        fs.WriteLine("异常类型：双模态相机2拍摄异常");
                //        fs.WriteLine("异常信息:" + ex.Message);
                //        fs.WriteLine("异常对象：" + ex.Source);
                //        fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                //        fs.WriteLine("触发方法：" + ex.TargetSite);
                //        fs.WriteLine();
                //        fs.Close();
                //    }
                //}
            }
        }

        private void Start_InnerLamp()
        {
            AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
            agDev_BgLamp.lamp_idx = 10;
            agDev_BgLamp.bright = 0;
            AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            //MessageBox.Show("打开内固视灯");
            this.Lamp5.Checked = true;
            return;
        }

        private void ListView_Refresh()
        {
            NetModel.List_Refresh();
            listView1.Items.Clear();
            for (int i = 0; i < NetModel.Patient_List.Count(); i++)
            {
                ListViewItem l = new ListViewItem(NetModel.Patient_List[i].Patient_name);
                l.Tag = NetModel.Patient_List[i].Patient_ID;
                l.SubItems.Add(NetModel.Patient_List[i].Patient_sex);
                l.SubItems.Add(NetModel.Patient_List[i].Patient_age);
                listView1.Items.Add(l);
            }
        }

        public Mainform()
        {
            InitializeComponent();
        }

        private void Mainform_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            Log fl = new Log();
            while (true)
            {
                fl.ShowDialog();
                if (fl.DialogResult == DialogResult.OK)
                {
                    break;
                }
                else if (fl.DialogResult == DialogResult.No)
                {
                    break;
                }
            }
            if (fl.DialogResult == DialogResult.OK)
            {
                NetModel.Net_info = fl.Netinfo;
                this.Visible = true;
                NetModel.Init();
                fl.Dispose();
                TEMP = 1;
            }
            else
            {
                Application.Exit();
                return;
            }
            LogAddress = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\Log\\" + DateTime.Now.Month + '_' + DateTime.Now.Day + "_OrbisLog.log";
            using (StreamWriter fs = new StreamWriter(LogAddress, true))
            {
                fs.WriteLine("----------------------------------------------------------------------------------------------------------------");
                fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                fs.WriteLine("程序开始运行");
                fs.Close();
            }
            try
            {
                NetModel.List_Refresh();
            }
            catch (Exception ex)
            {
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：列表获取失败");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
            }
            this.listView1.Columns.Add("姓名", 120, HorizontalAlignment.Left);
            this.listView1.Columns.Add("性别", 120, HorizontalAlignment.Left);
            this.listView1.Columns.Add("年龄", 120, HorizontalAlignment.Left);
            try
            {
                ListView_Refresh();
            }
            catch (Exception ex)
            {
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：刷新列表失败");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
            }
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)barEditItem2.Edit).Items.Clear(); //清除当前串口号中的所有串口名称
            this.barButtonItem7.Down = true;
            string[] names = SerialPort.GetPortNames();
            bool comExistence = false;
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)barEditItem2.Edit).Items.Add("COM" + (i + 1).ToString());
                    comExistence = true;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (comExistence)
            {
                //MessageBox.Show("找到可用串口！", "提示");
                barEditItem2.EditValue = ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)barEditItem2.Edit).Items[0];
            }
            else
            {
                //MessageBox.Show("没有找到可用串口！，自动按默认启动");
            }
            ms_cameraMode = "default";
            mb_statusInit = false;
            mb_isCameraOpen = false;
            mb_isUartOpen = false;
            AgDev.AgDev_GetStatus(ref this.ss);
            barEditItem1.EditValue = ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)barEditItem1.Edit).Items[0];
            //m_serialPort.Open();
            //mb_isUartOpen = true;

            this.timer4 = new System.Windows.Forms.Timer(this.components);//初始化硬件触发
            this.timer4.Tick += new EventHandler(this.timer4_Tick);
            this.timer4.Enabled = true;
            galleryControl1.Gallery.ItemImageLayout = ImageLayoutMode.ZoomInside;
            galleryControl1.Gallery.ImageSize = new Size(120, 90);
            galleryControl1.Gallery.ShowItemText = true;
            group1.Caption = "眼底照片";
            galleryControl1.Gallery.Groups.Add(group1);
            System.Threading.Thread thread_fresh = new System.Threading.Thread(NetModel.Uploading);
            thread_fresh.IsBackground = true;
            thread_fresh.Start();
            //NetModel.Upload = 1;
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ListView_Refresh();
            }
            catch (Exception ex)
            {
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：刷新失败");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
            }
        }

        public void galleryInit()
        {
            string result = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\";
            string path = result + "Data\\" + PatientNow_ID;
            if (Directory.Exists(path))
            {
                group1.Items.Clear();
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] fil = dir.GetFiles();
                DirectoryInfo[] dii = dir.GetDirectories();
                int i = 0;
                foreach (FileInfo f in fil)
                {
                    i++;
                    try
                    {
                        FileStream fileStream = new FileStream(f.FullName, FileMode.Open);
                        Image temp = Image.FromStream(fileStream);
                        fileStream.Close();
                        string[] name = f.FullName.Split('_');
                        if (name[2] == "Left")
                        {
                            Left_count++;
                        }
                        else
                        {
                            Right_count++;
                        }
                        GalleryItem item = new GalleryItem(temp, name[2] + name[3], name[4]);
                        item.Tag = f.FullName;
                        group1.Items.Add(item);
                    }
                    catch (Exception ex)
                    {
                        using (StreamWriter fs = new StreamWriter(LogAddress, true))
                        {
                            fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                            fs.WriteLine("异常类型：载入图片预览失败");
                            fs.WriteLine("异常信息:" + ex.Message);
                            fs.WriteLine("异常对象：" + ex.Source);
                            fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                            fs.WriteLine("触发方法：" + ex.TargetSite);
                            fs.WriteLine();
                            fs.Close();
                        }
                    }
                }
                Left_count = Left_count / 3;
                Right_count = Right_count / 3;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            PatientNow_ID = listView1.SelectedItems[0].Tag.ToString();
            try
            {
                Patient Patient_Now = NetModel.Patient_List.Find((Patient s) => s.Patient_ID == PatientNow_ID);
                this.PaitentNow = Patient_Now;
                //PatientsDetial Pdetails = NetModel.getPatientsDetial(Patient_Now);
                textPatient_name.Text = Patient_Now.Patient_name;
                textPatient_age.Text = Patient_Now.Patient_age;
                textPatient_sex.Text = Patient_Now.Patient_sex;
            }
            catch (Exception ex)
            {
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：获取详细内容失败");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
            }
            string result = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\";
            string patient_path = result + "Data\\" + PatientNow_ID;//创建对应目录
            if (!Directory.Exists(patient_path))
            {
                Directory.CreateDirectory(patient_path);
            }
            string doc_path = result + "Doctor\\" + PatientNow_ID;
            if (!Directory.Exists(doc_path))
            {
                Directory.CreateDirectory(doc_path);
            }
            UID_NOW = PatientNow_ID;
            DETIAL_NOW = 1;
            this.galleryInit();
            panelControl1.Visible = true;
            listView1.Visible = false;
        }

        private void galleryControl1_Click(object sender, EventArgs e)
        {

        }

        private void barEditItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string selectMode = this.barEditItem1.EditValue.ToString();
            if (selectMode == "自动模式")
            {
                AgDev.AgDev_Enable(1u, (UIntPtr)0u);
            }
            else if (selectMode == "默认模式")
            {
                AgDev.AgDev_Disable(1u);
                this.DeviceUpdate(".\\default.ir");
            }
            else if (selectMode == "小瞳模式")
            {
                AgDev.AgDev_Disable(1u);
                this.DeviceUpdate(".\\small.ir");
            }

        }


        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)//启动相机按钮
        {
            if (!this.mb_statusInit)
            {
                if (this.OpenCamera())
                {
                    if (this.ms_cameraMode == "Auto")
                    {
                        AgDev.AgDev_Enable(1u, (UIntPtr)0u);
                        return;
                    }
                    AgDev.AgDev_Disable(1u);
                    // 正常打开了相机
                    AgDev.AgDev_GetStatus(ref this.ss);
                    //AgDev.AgDev_GetInnerFxLampIdx(); // 内固视灯                  

                    int num = (int)(this.ss.pos_bitmap >> 1 & 1u);
                    if (num == 1)
                    {
                        this.DeviceUpdate(".\\default.ir");
                        AgDev.AgDev_SetMode(0u, 0u, UIntPtr.Zero, UIntPtr.Zero);
                        AgDev.AgDev_BeginGrab();
                        AgDev.AgDev_Enable(1u, (UIntPtr)0u);
                    }

                    // 无赤光
                    if (AgDev.AgDev_IsSupportWCG() != 0u)
                        AgDev.AgDev_EnableWCG(0);
                }
                else
                {
                    MessageBox.Show("请返回并重新连接相机");
                }
            }
            //if ((textEdit1.Text=="") || (textEdit2.Text=="") || (textEdit3.Text=="") || (textEdit4.Text==""))
            //{
            //    MessageBox.Show("请输入相机参数");
            //}
            //else
            //{
            //    lucamControl0.InitCamera(1,textEdit1.Text,textEdit2.Text,textEdit5.Text);
            //    lucamControl1.InitCamera(2,textEdit3.Text, textEdit4.Text,textEdit6.Text);
            //}
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)//拍摄图片按钮
        {

            if (mb_isCameraOpen && DETIAL_NOW == 1)
            {
                AgDev.AgDev_GetStatus(ref this.ss);
                this.Take("Color");
            }
            else if (!mb_isCameraOpen)
            {
                MessageBox.Show("请先连接到相机,或打开正确的串口,");
                return;
            }
            else
            {
                MessageBox.Show("请先选择患者");
                return;
            }
        }

        private void barToggleSwitchItem1_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)//有无赤光按钮
        {
            if (AgDev.AgDev_IsSupportWCG() != 0u)
            {
                if (barToggleSwitchItem1.Checked == true)
                {
                    AgDev.AgDev_EnableWCG(1);
                    return;
                }
                AgDev.AgDev_EnableWCG(0);
            }
            else
            {
                MessageBox.Show("请先连接到相机,或打开正确的串口,");
                barToggleSwitchItem1.Checked = false;
            }
        }

        private void barButtonItem10_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.panelControl3.Visible)
            {
                this.panelControl3.Visible = false;
                return;
            }
            else
            {
                this.panelControl3.Visible = true;
                try
                {
                    Start_InnerLamp();
                }
                catch (Exception ex)
                {
                    using (StreamWriter fs = new StreamWriter(LogAddress, true))
                    {
                        fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                        fs.WriteLine("异常类型：内固视灯初始化失败");
                        fs.WriteLine("异常信息:" + ex.Message);
                        fs.WriteLine("异常对象：" + ex.Source);
                        fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                        fs.WriteLine("触发方法：" + ex.TargetSite);
                        fs.WriteLine();
                        fs.Close();
                    }
                }
            }
        }

        private void Lamp1_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp1.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 3;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            }
            return;
        }

        private void Lamp2_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp2.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 2;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);

            }
            return;
        }

        private void Lamp3_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp3.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 1;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            }
            return;
        }

        private void Lamp4_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp4.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 4;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            }
            return;
        }

        private void Lamp5_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp5.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 9;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            }
            return;
        }

        private void Lamp6_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp6.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 8;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            }
            return;
        }

        private void Lamp7_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp7.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 5;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            }
            return;
        }

        private void Lamp8_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp8.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 6;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            }
            return;
        }

        private void Lamp9_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp9.Checked == true)
            {
                AgDev.AgDev_BgLamp agDev_BgLamp = default(AgDev.AgDev_BgLamp);
                agDev_BgLamp.lamp_idx = 10;
                agDev_BgLamp.bright = 0;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
                agDev_BgLamp.lamp_idx = 7;
                agDev_BgLamp.bright = 255;
                AgDev.AgDev_SetBgLamp(ref agDev_BgLamp);
            }
            return;
        }

        private void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barButtonItem7.Down == false)
            {
                barButtonItem7.Down = true;
            }
            else
            {
                barButtonItem7.Down = false;
            }
        }

        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barButtonItem8.Down == false)
            {
                barButtonItem8.Down = true;
            }
            else
            {
                barButtonItem8.Down = false;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            string result = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\";
            string dir = result + "Data\\" + PaitentNow.Patient_ID;
            string[] files = Directory.GetFiles(dir, "*.jpg");
            if (files.Length == 0)
            {
                DialogResult dr1 = MessageBox.Show("未拍摄图片是否上传？", "", MessageBoxButtons.YesNo);
                if (dr1 == DialogResult.Yes)
                {
                    DialogResult dr2 = MessageBox.Show("确认上传图片？", "", MessageBoxButtons.YesNo);
                    if (dr2 == DialogResult.Yes)
                    {
                        this.panelControl1.Visible = false;
                        this.listView1.Visible = true;
                        NetModel.Patiented_List.Add(PaitentNow);
                        NetModel.UploadList.Enqueue(PaitentNow);
                        NetModel.Upload_Signal = 1;
                        try
                        {
                            this.ListView_Refresh();
                        }
                        catch (Exception ex)
                        {
                            using (StreamWriter fs = new StreamWriter(LogAddress, true))
                            {
                                fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                                fs.WriteLine("异常类型：列表刷新失败");
                                fs.WriteLine("异常信息:" + ex.Message);
                                fs.WriteLine("异常对象：" + ex.Source);
                                fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                                fs.WriteLine("触发方法：" + ex.TargetSite);
                                fs.WriteLine();
                                fs.Close();
                            }
                        }
                        using (StreamWriter fs = new StreamWriter(LogAddress, true))
                        {
                            fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                            fs.WriteLine("完成用户拍摄，ID" + PatientNow_ID);
                            fs.WriteLine();
                            fs.Close();
                        }
                        group1.Items.Clear();
                        DETIAL_NOW = 0;
                        PatientNow_ID = "";
                        Left_count = 0;
                        Right_count = 0;
                    }
                }
            }
            else
            {
                DialogResult dr = MessageBox.Show("确认上传图片？", "", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {



                    this.panelControl1.Visible = false;
                    this.listView1.Visible = true;
                    NetModel.Patiented_List.Add(PaitentNow);
                    NetModel.UploadList.Enqueue(PaitentNow);
                    NetModel.Upload_Signal = 1;
                    try
                    {
                        this.ListView_Refresh();
                    }
                    catch (Exception ex)
                    {
                        using (StreamWriter fs = new StreamWriter(LogAddress, true))
                        {
                            fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                            fs.WriteLine("异常类型：列表刷新失败");
                            fs.WriteLine("异常信息:" + ex.Message);
                            fs.WriteLine("异常对象：" + ex.Source);
                            fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                            fs.WriteLine("触发方法：" + ex.TargetSite);
                            fs.WriteLine();
                            fs.Close();
                        }
                    }
                    using (StreamWriter fs = new StreamWriter(LogAddress, true))
                    {
                        fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                        fs.WriteLine("完成用户拍摄，ID" + PatientNow_ID);
                        fs.WriteLine();
                        fs.Close();
                    }
                    group1.Items.Clear();
                    DETIAL_NOW = 0;
                    PatientNow_ID = "";
                    Left_count = 0;
                    Right_count = 0;
                }
            }
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确认不再拍摄并返回？照片将会保存", "", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                this.panelControl1.Visible = false;
                this.listView1.Visible = true;
                this.ListView_Refresh();
                group1.Items.Clear();
                PatientNow_ID = "";
                Left_count = 0;
                Right_count = 0;
            }
        }

        private void galleryControl1_Gallery_ItemClick(object sender, GalleryItemClickEventArgs e)
        {

        }

        private void galleryControl1_Gallery_ItemRightClick(object sender, GalleryItemClickEventArgs e)
        {
            List<GalleryItem> LstArray = galleryControl1.Gallery.GetCheckedItems();
            if (LstArray.Count == 0)
            {
                MessageBox.Show("请先点击图片再进行操作");
            }
            else
            {
                DialogResult dr = MessageBox.Show("确认删除", "", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    
                    try
                    {
                        File.Delete(LstArray[0].Tag.ToString());
                        group1.Items.Remove(LstArray[0]);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("只有关闭预览图片才可以删除");
                        using (StreamWriter fs = new StreamWriter(LogAddress, true))
                        {
                            fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                            fs.WriteLine("异常类型：删除图片失败");
                            fs.WriteLine("异常信息:" + ex.Message);
                            fs.WriteLine("异常对象：" + ex.Source);
                            fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                            fs.WriteLine("触发方法：" + ex.TargetSite);
                            fs.WriteLine();
                            fs.Close();
                        }
                    }
                }
            }
        }

        private void galleryControl1_Gallery_ItemDoubleClick(object sender, GalleryItemClickEventArgs e)
        {
            List<GalleryItem> LstArray = galleryControl1.Gallery.GetCheckedItems();
            if (LstArray.Count == 0)
            {
                MessageBox.Show("请先点击图片再进行操作");
            }
            else
            {
                Picture_Show show = new Picture_Show();
                show.Image_Path = LstArray[0].Tag.ToString();
                show.Picture_ShowInit();
                show.Visible = true;
            }
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Check newcheck = new Check(NetModel.Patiented_List);
            newcheck.userid = NetModel.Net_info.user_id;
            newcheck.password = NetModel.Net_info.password;
            newcheck.Visible = true;
        }

        private void Mainform_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (NetModel.Upload == 1)
            {
                MessageBox.Show("未上传完成无法关闭");
                e.Cancel = true;
            }
            else
            {
                if (TEMP == 1)
                {
                    DialogResult dr1 = MessageBox.Show("是否确定关闭软件？", "", MessageBoxButtons.YesNo);
                    if (dr1 == DialogResult.Yes)
                    {

                        AgDev.AgDev_EndGrab();
                        AgDev.AgDev_UninitCamera();
                        AgDev.AgDev_Uninit();
                        if (mb_isUartOpen)
                        {
                            m_serialPort.WriteLine("2");
                            m_serialPort.Close();
                        }
                        this.timer4.Enabled = false;
                        using (StreamWriter fs = new StreamWriter(LogAddress, true))
                        {
                            fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                            fs.WriteLine("顺利关闭程序");
                            fs.WriteLine("---------------------------------------------------------------------------------------------------");
                            fs.WriteLine();
                            fs.Close();
                        }
                        TEMP = 0;
                        Application.Exit();
                    }
                    else
                        e.Cancel = true;
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void barEditItem1_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                string selectMode = this.barEditItem1.EditValue.ToString();
                if (selectMode == "自动模式")
                {
                    AgDev.AgDev_Enable(1u, (UIntPtr)0u);
                }
                else if (selectMode == "默认模式")
                {
                    AgDev.AgDev_Disable(1u);
                    this.DeviceUpdate(".\\default.ir");
                }
                else if (selectMode == "小瞳模式")
                {
                    AgDev.AgDev_Disable(1u);
                    this.DeviceUpdate(".\\small.ir");
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：属性下拉项获取失败");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
            }

        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (MessageBox.Show("是否删除该用户", "删除确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ListViewItem item = listView1.SelectedItems[0];
                    string DeleteIDNow = listView1.SelectedItems[0].Tag.ToString();
                    listView1.Items.Remove(item);
                    try
                    {
                        NetModel.DeleteList(DeleteIDNow);
                    }
                    catch(Exception ex)
                    {
                        using (StreamWriter fs = new StreamWriter(LogAddress, true))
                        {
                            fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                            fs.WriteLine("异常类型：删除列表失败");
                            fs.WriteLine("异常信息:" + ex.Message);
                            fs.WriteLine("异常对象：" + ex.Source);
                            fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                            fs.WriteLine("触发方法：" + ex.TargetSite);
                            fs.WriteLine();
                            fs.Close();
                        }
                    }
                }
            }
        }
    }
}
