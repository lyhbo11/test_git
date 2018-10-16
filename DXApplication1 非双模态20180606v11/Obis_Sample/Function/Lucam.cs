using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumenera;
using Lumenera.USB;

namespace Obis_Sample.Function
{
    public class Lucam
    {
        public int captureCounter = 0;
        public int snapshotCounter = 0;
        public bool isMono = false;
        public IntPtr hcam = IntPtr.Zero;
        public dll.LucamConversion conversion = new dll.LucamConversion();     // Create a conversion buffer.
        public dll.LucamFrameFormat frameFormat = new dll.LucamFrameFormat();  // Create Video frame format buffer.
        dll.LucamSnapshot snap = new dll.LucamSnapshot();               // Create snapshot buffer
        public int nbCameras = 0;
        public dll.LucamVersion[] lumVersion;
        string strWork;                                 // String to work with.
        string strSerial = string.Empty;                // Serial number working string.
        string errorString;
  
        public string PatientNow_ID;
        
        public void InitCamera(int Num,string Exposure_TB, string Gain_TB,string VideoExposure)
        {
            nbCameras = Lumenera.USB.dll.LucamNumCameras();
            string[] strWorks = new string[nbCameras];
            lumVersion = new dll.LucamVersion[nbCameras];                               // Adjust array according to number of cameras founded..
            lumVersion = api.EnumCameras();                                              // Get all cameras information.
            byte[] camMac = new byte[6];
            byte[] hostMac = new byte[6];
            dll.LgcamIPConfiguration camConfig = new dll.LgcamIPConfiguration();
            dll.LgcamIPConfiguration hostConfig = new dll.LgcamIPConfiguration();
            // Fill list with the camera found.
            for (int i = 0; i < nbCameras; i++)
            {
                strSerial = lumVersion[i].SerialNumber.ToString();
                switch (lumVersion[i].CameraId)
                {
                    #region "LU050"
                    case 0x091:
                    case 0x095:
                        strWork = "LU050 Series --> S/N: ";
                        break;
                    #endregion

                    #region "Lu056"
                    case 0x090:
                    case 0x093:
                        strWork = "LU056 Series --> S/N: ";
                        break;
                    #endregion


                    #region "lu070"
                    case 0x06c:
                    case 0x07c:
                    case 0x08c:
                        strWork = "LU070 Series --> S/N: ";
                        break;
                    #endregion

                    #region "Lu080"
                    case 0x085:
                        strWork = "LU080 Series --> S/N: ";
                        break;
                    #endregion

                    #region "Lu100 series"
                    case 0x12:
                    case 0x052:
                    case 0x092:
                    case 0x09d:
                        strWork = "LU100 Sereis --> S/N: ";
                        break;
                    case 0x094:
                        strWork = "LU110 Series --> S/N: ";
                        break;
                    case 0x096:
                        strWork = "LU120 Series --> S/N: ";
                        break;
                    case 0x07a:
                    case 0x09a:
                        strWork = "LU130 Series --> S/N: ";
                        break;
                    case 0x098:
                        strWork = "LU150 Series --> S/N: ";
                        break;
                    case 0x08a:
                        strWork = "LU160 Series --> S/N: ";
                        break;
                    case 0x04E:
                    case 0x05e:
                    case 0x06e:
                    case 0x099:
                    case 0x09e:
                        strWork = "LU170 Series --> S/N: ";
                        break;
                    case 0x062:
                    case 0x072:
                    case 0x082:
                        strWork = "LU176 Series --> S/N: ";
                        break;
                    #endregion

                    #region lu200
                    case 0x17:
                    case 0x097:
                    case 0x9c:
                        strWork = "LU200 Series --> S/N: ";
                        break;
                    case 0x08d:
                        strWork = "LU270 Series --> S/N: ";
                        break;
                    #endregion

                    #region Lu300 series
                    case 0x087:
                        strWork = "LU300 Series --> S/N: ";
                        break;

                    case 0x09b:
                        strWork = "LU330 Series --> S/N: ";
                        break;

                    case 0x05b:
                    case 0x07b:
                    case 0x08b:
                        strWork = "LU370 Series --> S/N: ";
                        break;
                    #endregion

                    #region Lu5xx
                    case 0x81:
                        strWork = "LU500 Series --> S/N: ";
                        break;
                    #endregion

                    #region Lu6xxx
                    case 0x086:
                        strWork = "LU620 Series --> S/N: ";
                        break;
                    #endregion

                    #region Lw0xx
                    case 0x189:
                        strWork = "Lw015 Series --> S/N: ";
                        break;
                    case 0x18c:
                        strWork = "Lw070 Series --> S/N: ";
                        break;

                    case 0x184:
                        strWork = "Lw080 Series --> S/N: ";
                        break;
                    #endregion

                    #region lw1xx
                    case 0x196:
                        strWork = "Lw120 Series --> S/N: ";
                        break;

                    case 0x11a:
                    case 0x19a:
                        strWork = "Lw130 Series --> S/N: ";
                        break;

                    case 0x10a:
                    case 0x18a:
                        strWork = "Lw160 Series --> S/N: ";
                        break;
                    case 0x19e:
                        strWork = "Lw170 Series --> S/N: ";
                        break;

                    case 0x15e:
                        strWork = "Lw175 Series --> S/N: ";
                        break;
                    #endregion

                    #region Lw2xx
                    case 0x110:
                    case 0x180:
                    case 0x1c0:
                        strWork = "Lw230 Series --> S/N: ";
                        break;
                    case 0x1c6:
                        strWork = "Lw250 Series --> S/N: ";
                        break;

                    case 0x16d:
                    case 0x1cd:
                        strWork = "Lw290 Series --> S/N: ";
                        break;
                    #endregion

                    #region lw3xx
                    case 0x19b:
                        strWork = "Lw330 Series --> S/N: ";
                        break;
                    #endregion

                    #region lw4xx
                    case 0x1c7:
                        strWork = "Lw450 Series --> S/N: ";
                        break;
                    #endregion

                    #region lw5xx
                    case 0x1ce:
                        strWork = "Lw530 Series --> S/N: ";
                        break;


                    case 0x115:
                    case 0x1c5:
                        strWork = "Lw570 Series --> S/N: ";
                        break;
                    #endregion

                    #region lw6xx
                    case 0x186:
                        strWork = "Lw620 Series --> S/N: ";
                        break;
                    case 0x1c8:
                    #endregion

                    #region lw11xxx
                    case 0x1ca:
                        strWork = "Lw11050 Series --> S/N: ";
                        break;
                    case 0x1c9:
                        strWork = "Lw16050 Series --> S/N: ";
                        break;
                    #endregion

                    #region INFINITY
                    case 0x01e:
                    case 0x031:
                    case 0x0A1:
                    case 0x0b1:
                    case 0x0e1:
                    case 0x1a6:
                    case 0x1ac:
                    case 0x1e5:
                        strWork = "INFINITY1 Series --> S/N: ";
                        break;
                    case 0x0A2:
                    case 0x0b2:
                    case 0x132:
                    case 0x144:
                    case 0x159:
                    case 0x1a7:
                    case 0x1b2:
                    case 0x1b7:
                        strWork = "INFINITY2 Series --> S/N: ";
                        break;
                    case 0x01b:
                    case 0x033:
                    case 0x0a3:
                    case 0x0b3:
                    case 0x0e3:
                    case 0x135:
                    case 0x15A:
                    case 0x1af:
                    case 0x1b5:
                        strWork = "INFINITY3 Series --> S/N: ";
                        break;
                    case 0x044:
                    case 0x0a4:
                    case 0x0b4:
                    case 0x168:
                    case 0x1a8:
                    case 0x1ab:
                    case 0x1aa:
                    case 0x1f9:
                        strWork = "INFINITY4 Series --> S/N: ";
                        break;
                    case 0x0a5:
                    case 0x0b5:
                    case 0x0ba:
                        strWork = "INFINITY5 Series --> S/N: ";
                        break;
                    case 0x020:
                    case 0x040:
                    case 0x0a0:
                    case 0X0B0:
                    case 0x0e0:
                    case 0x129:
                    case 0x1a9:
                    case 0x1b9:
                        strWork = "INFINITYX Series --> S/N: ";
                        break;

                    #endregion

                    #region LV series
                    case 0x46c:
                        strWork = "Lu070 --> S/N: ";
                        break;
                    case 0x49a:
                        strWork = "Lv130 --> S/N: ";
                        break;

                    case 0x02E:
                    case 0x49e:
                        strWork = "Lv170 --> S/N: ";
                        break;
                    case 0x480:
                        strWork = "Lv230 --> S/N: ";
                        break;
                    case 0x487:
                        strWork = "Lv900 --> S/N: ";
                        break;
                    case 0x49f:
                        strWork = "Lw110 --> S/N: ";
                        break;
                    case 0x48f:
                        strWork = "Lw310 --> S/N: ";
                        break;
                    case 0x4ce:
                        strWork = "Lw560 --> S/N: ";
                        break;


                    case 0x432:
                    case 0x4a2:
                    case 0x4b1:
                        strWork = "INFINITY1 --> S/N: ";
                        break;
                    case 0x460:
                    case 0x462:
                    case 0x4ae:
                        strWork = "INFINITY2 --> S/N: ";
                        break;
                    case 0x464:
                    case 0x465:
                        strWork = "INFINITY3 --> S/N: ";
                        break;

                    case 0x461:
                        strWork = "INFINITY Lite --> S/N: ";
                        break;
                    case 0x463:
                        strWork = "INFINITYX --> S/N: ";
                        break;



                    #endregion

                    #region PhotoID
                    case 0x083:
                        strWork = "Photoid Series --> S/N: ";
                        break;
                    #endregion

                    #region SKYnyx

                    case 0x1dc:
                        strWork = "SKYnyx 2-0 Series --> S/N: ";
                        break;
                    case 0x1d0:
                        strWork = "SKYnyx 2-2 Series --> S/N: ";
                        break;
                    case 0x1dA:
                        strWork = "SKYnyx 2-1 Series --> S/N: ";
                        break;
                    #endregion

                    #region Mini LM
                    case 0x27c:
                    case 0x28c:
                        strWork = "LM070 --> S/N: ";
                        break;

                    case 0x264:
                    case 0x284:
                        strWork = "LM080 --> S/N: ";
                        break;
                    case 0x279:
                    case 0x27a:
                    case 0x29a:
                        strWork = "LM130 --> S/N: ";
                        break;

                    case 0x269:
                    case 0x26a:
                    case 0x28a:
                        strWork = "LM160 --> S/N: ";
                        break;
                    case 0x2c5:
                        strWork = "LM570 --> S/N: ";
                        break;
                    case 0x2c8:
                        strWork = "LM11050 --> S/N: ";
                        break;


                    #endregion

                    #region LC
                    case 0x384:
                        strWork = "LC080 --> S/N: ";
                        break;
                    case 0x36e:
                    case 0x39e:
                        strWork = "LC170 --> S/N: ";
                        break;
                    case 0x38d:
                        strWork = "LC270 --> S/N: ";
                        break;
                    case 0x38b:
                        strWork = "LC370 --> S/N: ";
                        break;
                    case 0x3ad:
                        strWork = "INFINITY Lite --> S/N: ";
                        break;
                    #endregion



                    #region Giga ethernet
                    case 0x40000:
                        strWork = "LG unprogrammed id --> S/N: ";
                        break;
                    case 0x40080:
                        strWork = "Lg230ii --> S/N: ";
                        break;
                    case 0x400c8:
                        strWork = "Lg11050 --> S/N: ";
                        break;
                    case 0x400ca:
                        strWork = "Lg11050ii --> S/N: ";
                        break;
                    case 0x401ce:
                        strWork = "Lg565 --> S/N: ";
                        break;
                    case 0x602:
                        strWork = "Lt220 --> S/N: ";
                        break;
                    case 0x604:
                        strWork = "Lt420 --> S/N: ";
                        break;

                    #endregion

                    default:
                        strWork = "Unknown --> S/N: ";
                        break;
                }
                strWork += strSerial;
                strWorks[i] = strWork;
            }
            dll.LucamColorFormat colorFormat;
            hcam = api.CameraOpen(Num);
            api.SetProperty(hcam, dll.LucamProperty.EXPOSURE, Convert.ToSingle(VideoExposure));
            colorFormat = (dll.LucamColorFormat)api.GetProperty(hcam, dll.LucamProperty.COLOR_FORMAT);
            isMono = false;
            if (colorFormat == dll.LucamColorFormat.MONO)
            {
                isMono = true;
                api.SetProperty(hcam, dll.LucamProperty.FLIPPING, (float)dll.LucamFlippingFormat.Y);
            }
            try
            {
                frameFormat = api.GetFormat(hcam);                              // Get frame format.
            }
            catch
            {
                return;
            }
            // Initialize snapshot.
            snap.Exposure = Convert.ToSingle(Exposure_TB);                  // Set exposure.
            snap.ExposureDelay = 0;                                             // Set exposure delay.
            snap.Format.BinningX = frameFormat.BinningX;                        // Set Binning x as video format.
            snap.Format.BinningY = frameFormat.BinningY;                        // Set Binning y as video format.
            snap.Format.FlagsX = frameFormat.FlagsX;
            snap.Format.FlagsY = frameFormat.FlagsY;
            snap.Format.Height = frameFormat.Height;
            snap.Format.PixelFormat = frameFormat.PixelFormat;
            snap.Format.SubSampleX = frameFormat.SubSampleX;
            snap.Format.SubSampleY = frameFormat.SubSampleY;
            snap.Format.Width = frameFormat.Width;
            snap.Format.X = frameFormat.X;
            snap.Format.Y = frameFormat.Y;
            snap.Gain = Convert.ToSingle(Gain_TB);
            snap.ShutterType = dll.LucamShutterType.GlobalShutter;
            snap.StrobeDelay = 0;
            snap.StrobeFlags = 0;
            snap.Timeout = 5000;    // 5 sec timeout.
            snap.UseHwTrigger = true;
            if (!isMono)
            {
                try
                {
                    snap.GainBlue = api.GetProperty(hcam, dll.LucamProperty.GAIN_BLUE);
                    snap.GainGrn1 = api.GetProperty(hcam, dll.LucamProperty.GAIN_GREEN1);
                    snap.GainGrn2 = api.GetProperty(hcam, dll.LucamProperty.GAIN_GREEN2);
                    snap.GainRed = api.GetProperty(hcam, dll.LucamProperty.GAIN_RED);
                }
                catch (Exception)
                {

                }
            }
            else
            {
                snap.GainBlue = 1;
                snap.GainGrn1 = 1;
                snap.GainGrn2 = 1;
                snap.GainRed = 1;
            }
        }

        public void TakingPhoto(Object path1)
        {
            string path = path1 as string;
            byte[] rawImage;
            int SizeImage = (snap.Format.Width / snap.Format.SubSampleX) * (snap.Format.Height / snap.Format.SubSampleY); // Initialize the value of buffer size;
            if (frameFormat.PixelFormat == dll.LucamPixelFormat.PF_16)      // Check if in 16 bits mode.
                SizeImage *= 2;                                             // Double size of buffer in 16bits mode.
            rawImage = new byte[SizeImage];                                 // Allocate memory to hold raw data.
            if (!dll.LucamTakeSnapshot(hcam, ref snap, rawImage))                      // Grab video frame.
            {
                return;
            }
            // Create a picture windows.
            conversion.CorrectionMatrix = dll.LucamCorrectionMatrix.FLUORESCENT; // Color correction matrix initialization.
            conversion.DemosaicMethod = dll.LucamDemosaicMethod.HIGHER_QUALITY; // Demosaicing method selection.
            Bitmap myImage;
            byte[] pdata;                           // Raw Image Buffer.
            byte[] rgbImage;
            BitmapData MyImageData;                 // Bitmap header information.
            Rectangle lockdata;
            int imageSize = SizeImage;

            if (snap.Format.PixelFormat == dll.LucamPixelFormat.PF_16)
                imageSize *= 2;                     // double the size if in 16 bits;

            pdata = new byte[imageSize];            // Allocate memory for RAW Image;
            pdata = rawImage;                       // Save the given image in memory;
            rgbImage = new byte[imageSize * 3];     // Allocate memory for the RGB converstion.
            int width = snap.Format.Width / snap.Format.SubSampleX;
            int height = snap.Format.Height / snap.Format.SubSampleY;
            dll.LucamConvertFrameToRgb24(hcam, rgbImage, pdata, width, height, snap.Format.PixelFormat, ref conversion);


            // Create bitmap with RGB
            if (snap.Format.PixelFormat == dll.LucamPixelFormat.PF_8)
                myImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);    // create bitmap header
            else
            {
                myImage = new Bitmap(width, height, PixelFormat.Format48bppRgb); // Create RGB 48 header if in 16 bit mode.
            }
            MyImageData = new BitmapData();                                                                  // Create bitmap data.                
            lockdata = new Rectangle(0, 0, width, height);                          // Work with full pictures.
            if (snap.Format.PixelFormat == dll.LucamPixelFormat.PF_8)
            {
                MyImageData = myImage.LockBits(lockdata, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb, MyImageData); // Prepare for data copy.
                System.Runtime.InteropServices.Marshal.Copy(rgbImage, 0, MyImageData.Scan0, (width * height * 3)); // copy RGB Date.
                myImage.UnlockBits(MyImageData);
            }
            else
            {
                MyImageData = myImage.LockBits(lockdata, ImageLockMode.ReadWrite, PixelFormat.Format48bppRgb, MyImageData); // Prepare for data copy.


                System.Runtime.InteropServices.Marshal.Copy(rgbImage, 0, MyImageData.Scan0, (width * height * 3 * 2)); // copy RGB Date.
                myImage.UnlockBits(MyImageData);

            }
            myImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
            myImage.Save(path, ImageFormat.Jpeg);
        }


    }
}
