using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hardward
{
	public class AgDev
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct AgDev_BgLamp
		{
			public int lamp_idx;

			public int min;

			public int max;

			public int step;

			public int mode;

			public int mode_param;

			public int bright;

			public int flag;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct AgDev_CCDParam
		{
			public int prop;

			public int min;

			public int max;

			public int step;

			public int def;

			public int def_flag;

			public int val;

			public int flag;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct AgDev_Status
		{
			public uint is_grabbing;

			public uint pos_bitmap;

			public uint flash_rdy;

			public uint key_bitmap;

			public uint mode_id;

			public byte gray_min_lv;

			public byte gray_max_lv;

			public byte pad0;

			public byte pad1;

			public uint rsvd0;

			public uint rsvd1;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct AgDev_ImageWH
		{
			public uint w;

			public uint h;
		}

		public unsafe delegate void AgDevSnapRecall(void* o, byte* dat_ptr, int im_width, int im_height, int im_width_bytes, int dat_type);

		public const uint AGDEV_SNAP_RECALL_IMAGE_TYPE_RGB = 0u;

		public const uint AGDEV_SNAP_RECALL_IMAGE_TYPE_RGBA = 1u;

		public const uint AGDEV_SNAP_RECALL_IMAGE_TYPE_GRAY = 2u;

		public const uint AGDEV_OPT_AutoAdjust = 1u;

		public const uint AGDEV_LAMP_IDX_InnerClose = 0u;

		public const uint AGDEV_LAMP_IDX_InnerFx0 = 1u;

		public const uint AGDEV_LAMP_IDX_InnerFx1 = 2u;

		public const uint AGDEV_LAMP_IDX_InnerFx2 = 3u;

		public const uint AGDEV_LAMP_IDX_InnerFx3 = 4u;

		public const uint AGDEV_LAMP_IDX_InnerFx4 = 5u;

		public const uint AGDEV_LAMP_IDX_InnerFx5 = 6u;

		public const uint AGDEV_LAMP_IDX_InnerFx6 = 7u;

		public const uint AGDEV_LAMP_IDX_InnerFx7 = 8u;

		public const uint AGDEV_LAMP_IDX_InnerFx8 = 9u;

		public const uint AGDEV_LAMP_IDX_OuterFx = 10u;

		public const uint AGDEV_LAMP_IDX_RedLED = 16u;

		public const uint AGDEV_LAMP_IDX_InfraredLED = 16u;

		public const uint AGDEV_LAMP_IDX_White = 16u;

		public const uint AGDEV_LAMP_IDX_BlueLED = 32u;

		public const uint AGDEV_LAMP_IDX_Flash = 48u;

		public const uint AGDEV_CCD_PROP_Brightness = 0u;

		public const uint AGDEV_CCD_PROP_Contrast = 1u;

		public const uint AGDEV_CCD_PROP_Hue = 2u;

		public const uint AGDEV_CCD_PROP_Saturation = 3u;

		public const uint AGDEV_CCD_PROP_Sharpness = 4u;

		public const uint AGDEV_CCD_PROP_Gamma = 5u;

		public const uint AGDEV_CCD_PROP_ColorEnable = 6u;

		public const uint AGDEV_CCD_PROP_WhiteBalance = 7u;

		public const uint AGDEV_CCD_PROP_BackgroundLight = 8u;

		public const uint AGDEV_CCD_PROP_Gain = 9u;

		public const uint AGDEV_CCD_PROP_ExposeTime = 10u;

		public const uint AGDEV_CCD_PROP_GainRed = 11u;

		public const uint AGDEV_CCD_PROP_GainGreen = 12u;

		public const uint AGDEV_CCD_PROP_GainBlue = 13u;

		public const uint AGDEV_CCD_PROP_ExposeTarget = 14u;

		public const uint AGDEV_CCD_PROP_WhiteBalanceRed = 15u;

		public const uint AGDEV_CCD_PROP_WhiteBalanceGreen = 16u;

		public const uint AGDEV_CCD_PROP_WhiteBalanceBlue = 17u;

		public const uint AGDEV_CCD_PROP_ImageRect = 18u;

		public const uint AGDEV_CCD_PROP_IrisRadius = 19u;

		public const uint AGDEV_CCD_PROP_MirrorV = 20u;

		public const uint AGDEV_CCD_PROP_MirrorH = 21u;

		public const uint AGDEV_CCD_PROP_CONTRAST_EX = 22u;

		public const uint AGDEV_CCD_PROP_BRIGHT_EX = 23u;

		public const uint AGDEV_CCD_PROP_RGB_B_EX = 24u;

		public const uint AGDEV_CCD_PROP_RGB_G_EX = 25u;

		public const uint AGDEV_CCD_PROP_RGB_R_EX = 26u;

		public const uint AGDEV_MODE_Infrared = 0u;

		public const uint AGDEV_MODE_NORMAL = 0u;

		public const uint AGDEV_MODE_VisibleLight = 4u;

		public const uint AGDEV_MODE_NORMALCO = 4u;

		public const uint AGDEV_MODE_FFA = 2u;

		public const uint AGDEV_MODE_FFABK = 2u;

		public const uint AGDEV_MODE_ICGA = 3u;

		public const uint AGDEV_MODE_ColorExp = 1u;

		public const uint AGDEV_MODE_EXP = 1u;

		public const int AGDEV_MODE_Test = 5;

		public const int AGDEV_MODE_IDLE = 6;

		public const int AGDEV_MODE_InfraredBK = 7;

		public const int AGDEV_MODE_NoRed_Bk = 8;

		public const int AGDEV_MODE_SelfLumin = 9;

		public const uint NW_S_OK = 0u;

		public const uint NW_S_TRUE = 1u;

		public const uint NW_S_FALSE = 0u;

		public const uint NW_E_NO_ERROR = 0u;

		public const uint NW_E_UNEXPECTED = 129u;

		public const uint NW_E_NOT_SUPPORTED = 130u;

		public const uint NW_E_OUT_OF_MEMORY = 131u;

		public const uint NW_E_OUT_OF_TIME = 132u;

		public const uint NW_E_EMPTY = 133u;

		public const uint NW_E_FULL = 134u;

		public const uint NW_E_OVER_LIMIT = 135u;

		public const uint NW_E_CONFLICT = 136u;

		public const uint NW_E_NOT_EXIST = 137u;

		public const uint NW_E_INVALIDARG = 144u;

		public const uint NW_E_INVALIDARG0 = 144u;

		public const uint NW_E_INVALIDARG1 = 145u;

		public const uint NW_E_INVALIDARG2 = 146u;

		public const uint NW_E_INVALIDARG3 = 147u;

		public const uint NW_E_INVALIDARG4 = 148u;

		public const uint NW_E_INVALIDARG5 = 149u;

		public const uint NW_E_INVALIDARG6 = 150u;

		public const uint NW_E_INVALIDARG7 = 151u;

		public const uint NW_E_INVALIDARG8 = 152u;

		public const uint NW_E_INVALIDARG9 = 153u;

		public const uint NW_E_INVALIDARG10 = 154u;

		public const uint NW_E_INVALIDARG11 = 155u;

		public const uint NW_E_INVALIDARG12 = 156u;

		public const uint NW_E_INVALIDARG13 = 157u;

		public const uint NW_E_INVALIDARG14 = 158u;

		public const uint NW_E_INVALIDARG15 = 159u;

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_Init(uint cfg_id, uint ccnd, uint esd, uint sftd, uint s);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern void AgDev_Uninit();

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_InitCamera(IntPtr bk_owner, IntPtr bk2_owner, IntPtr co_owner, IntPtr co2_owner);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern void AgDev_UninitCamera();

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern void AgDev_SetMode(uint m, uint rect_id, UIntPtr owner, UIntPtr p_owner);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern void AgDev_BeginGrab();

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern void AgDev_EndGrab();

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_SetCCDParam(uint cam_id, ref AgDev.AgDev_CCDParam p);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_GetCCDParam(uint cam_id, ref AgDev.AgDev_CCDParam p);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_GetCCDParamRang(uint cam_id, ref AgDev.AgDev_CCDParam p);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_GetBgLamp(ref AgDev.AgDev_BgLamp p);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern int AgDev_GetInnerFxLampIdx();

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_SetBgLamp(ref AgDev.AgDev_BgLamp p);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_SnapImage([MarshalAs(UnmanagedType.LPWStr)] string fname, uint quality);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_SnapAvi([MarshalAs(UnmanagedType.LPWStr)] string fname, uint c, uint quality);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern void AgDev_GetStatus(ref AgDev.AgDev_Status p);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern void AgDev_SetSnapRecall(AgDev.AgDevSnapRecall recall_func, IntPtr recall_obj);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_Enable(uint op_mask, UIntPtr p);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_Disable(uint op_mask);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern void AgDev_GetImageWH(ref AgDev.AgDev_ImageWH p);

		[DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern uint AgDev_GetCCDGroupID();

        [DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern uint AgDev_EnableWCG(int is_on);

        [DllImport("agdev.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern uint AgDev_IsSupportWCG();

		public unsafe static void AgDev_RecallProcess(void* o, byte* dat_ptr, int im_width, int im_height, int im_width_bytes, int dat_type)
		{
			for (int i = 0; i < 30; i++)
			{
				*(dat_ptr++) = 255;
			}
			MessageBox.Show("已抓取");
		}
	}
}
