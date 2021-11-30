using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Configuration;

/*
 *  Camera Class aquires data coming for the ccd line camera
 */
class Camera
{

    public delegate void frameCallbackDelegate(int ftype, int row, int col, ref ProcessedData DataProperty, uint BufferPtr);
    public delegate void dataDelegate(ushort[] data);

    dataDelegate masterDataDelegate;
    static bool cameraRunning = false;
    static bool frameGrabbing = false;
    static long Noframes = 0;
    ushort[] data;
    frameCallbackDelegate frameDelegate;

    [StructLayout(LayoutKind.Explicit)]
    public struct ProcessedData
    {
        [FieldOffset(0)]
        public int CameraID;
        [FieldOffset(4)]
        public int ExposureTime;
        [FieldOffset(8)]
        public int TimeStamp;
        [FieldOffset(12)]
        public int TriggerOccurred;
        [FieldOffset(16)]
        public int TriggerEventOccurred;
        [FieldOffset(20)]
        public int OverSaturated;
        [FieldOffset(24)]
        public int LightShieldPixelAverage;
    }

    [DllImport("CCD_USBCamera_SDK_stdcall.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)] //@@ Change dlls to work with new camera
    private static extern int CCDUSB_InitDevice();

    [DllImport("CCD_USBCamera_SDK_stdcall.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_UnInitDevice();


    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_AddDeviceToWorkingSet(int DeviceID);

    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_StartCameraEngine(IntPtr ParentHandle);

    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
        EntryPoint = "CCDUSB_SetCameraWorkMode",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_SetCameraWorkMode(int DeviceID, int WorkMode);


    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
        EntryPoint = "CCDUSB_StartFrameGrab",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_StartFrameGrab(int TotalFrames);

    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
        EntryPoint = "CCDUSB_StopFrameGrab",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_StopFrameGrab();

    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_InstallFrameHooker(int FrameType, Delegate FrameCallBack);

    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint CCDUSB_GetCurrentFrame(int Device, IntPtr FramePtr);

    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_SetExposureTime(int DeviceID, int exposureTime, int Store);

    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern int CCDUSB_ShowFactoryControlPanel(int deviceID, String password);

    [DllImport("CCD_USBCamera_SDK_stdcall.dll",
        EntryPoint = "CCDUSB_HideFactoryControlPanel", CallingConvention = CallingConvention.StdCall)]
    private static extern int CCDUSB_HideFactoryControlPanel();

    public Camera()
    {
    }

    public int cameraInit(IntPtr id, int exp)
    {
        // int a;
        int errNum;
        int camNum;
        //   init();
        camNum = init();
        errNum = addCamera(camNum);
        errNum = addToEngine(id);
        errNum = setExposure(exp);
        return errNum;
    }

    private int init()
    {
        return CCDUSB_InitDevice();
    }

    private int unInit()
    {
        return CCDUSB_UnInitDevice();
    }

    private int addCamera(int nr)
    {
        return CCDUSB_AddDeviceToWorkingSet(nr);
    }

    private int addToEngine(IntPtr id)
    {
        IntPtr parent_wnd;
        parent_wnd = id;
        cameraRunning = true;
        return CCDUSB_StartCameraEngine(parent_wnd);
    }

    public void grabbingFrameCallback(int ftype, int row, int col, ref ProcessedData DataProperty, uint BufferPtr)
    {
        uint i;
        uint frameSize;
        ushort newd;

        uint hibi, lobi;

        Noframes++;
        unsafe
        {
            byte* frameptr;
            frameSize = (uint)((row * col));
            data = new ushort[frameSize];
            frameptr = (byte*)BufferPtr;
            for (i = 0; i < frameSize; i++)
            {
                lobi = *frameptr;
                frameptr++;
                hibi = *frameptr;
                frameptr++;
                newd = (ushort)(hibi * 256 + lobi);
                //data[i] = data[i] + newd;
                data[i] = newd;
            }
        }
        
        masterDataDelegate(data);
    }





    public int startFrameGrab(int nr, int trigmode, dataDelegate dd)  // trigmode=0 internal
    {

        int err1, err2, err3;
        if (frameGrabbing == true) stopFrameGrab();
        frameDelegate = new frameCallbackDelegate(grabbingFrameCallback);
        masterDataDelegate = dd;

        if (trigmode == 0)
        {
            err3 = CCDUSB_SetCameraWorkMode(1, 0);
        }
        else
        {
            err3 = CCDUSB_SetCameraWorkMode(1, 1);   // external triggermode
        }

        err2 = CCDUSB_InstallFrameHooker(1, frameDelegate); // I get raw data in this example.
        err1 = CCDUSB_StartFrameGrab(nr);
        frameGrabbing = true;
        if (err1 == -1) return -1;
        if (err2 == -1) return -2;
        if (err3 == -1) return -3;
        return 0;
    }

    public int stopFrameGrab()
    {

        int err1, err2;
        err2 = CCDUSB_StopFrameGrab();
        err1 = CCDUSB_InstallFrameHooker(0, null);
        if (err1 == -1) return -1;
        if (err2 == -1) return -2;
        frameGrabbing = false;
        return 0;

    }

    private int getCurrentFrame(ushort[] Data)
    {
        IntPtr _pImage = new IntPtr();  //image pointer
        IntPtr ptr;
        unsafe
        {
            _pImage = Marshal.AllocHGlobal(1 * 2056);
            ptr = (IntPtr)CCDUSB_GetCurrentFrame(1, _pImage);
            short[] tmp = new short[2056];
            Marshal.Copy(ptr, tmp, 0, 2056);
            Marshal.FreeHGlobal(_pImage);
            System.Buffer.BlockCopy(tmp, 0, Data, 0, 2056 * 2);
        }
        return 2056;
    }

    public int setExposure(int ex)
    {
        int a;
        if (cameraRunning)
        {

            if ((ex > 0) && (ex < 100000))
            {
                a = CCDUSB_SetExposureTime(1, ex, 0);
                return a;
            }
            else return -1;
        }
        else return -1;
    }


    public void showPanel()
    {
        int a;
        a = CCDUSB_ShowFactoryControlPanel(1, "123456");

    }

    public void hidePanel()
    {
        CCDUSB_HideFactoryControlPanel();
    }


}



