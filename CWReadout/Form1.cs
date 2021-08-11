using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TwinCAT.Ads;


namespace BRSReadout
{
    public partial class Form1 : Form
    {
        public bool twinCatBool = ("true" == ConfigurationManager.AppSettings.Get("twinCat"));
        public string cameraType = ConfigurationManager.AppSettings.Get("camera");
        public static int camWidth = int.Parse(ConfigurationManager.AppSettings.Get("cameraWidth"));

        public static Form2 graphWindow =null;

        public bool firstFrame = true;
        public int firstValueCounter = 0;
        public double zeroValue = 0;
        public double refZeroValue = 0;
        public double refValue = 0;

        Camera myCamera;
        DataConsumerDelegate[] consumerd;

        Stopwatch myStopwatch;
        DataWriting myDataWriter;

        int startIndexRightRef = 1800; //Beginning of right pattern
        int startIndexLeftRef = 0; //Beginning of right pattern

        bool dampOverride = false;
        volatile bool quittingBool = false;
        public volatile bool recordBool = false;

        double sampFreq = Math.Pow(10, 6) / double.Parse(ConfigurationManager.AppSettings.Get("cameraExposureTime"))/ double.Parse(ConfigurationManager.AppSettings.Get("frameAverageNum"));

        public volatile int Frameco = 0;        
        ushort[] refFrame = new ushort[camWidth];
        public static ushort[] frame = new ushort[camWidth];
        public static ushort[] flipFrame = new ushort[camWidth];

        volatile bool dataWritingThreadBool;
        Thread dataWritingThread;
        Thread cameraThread;
        private Queue<PeakQueueItem> dataWritingQueue;
        public volatile SyncEvents dataWritingSyncEvent;

        public static object graphLock = new object();
        public static Queue<graphData> graphQueue = new Queue<graphData>();
        public static AutoResetEvent graphSignal = new AutoResetEvent(false);
        public static Thread graphThread;
        public double graphSum;
        
        double dayFrameCo0;

        static double refGain = double.Parse(ConfigurationManager.AppSettings.Get("refGain"));
        static double angleGain = double.Parse(ConfigurationManager.AppSettings.Get("angleGain"));
        static double velGain = double.Parse(ConfigurationManager.AppSettings.Get("velGain"));
        static double driftGain = double.Parse(ConfigurationManager.AppSettings.Get("driftGain"));
        static double driftOffset = double.Parse(ConfigurationManager.AppSettings.Get("driftOffset"));
        static double driftOverGain = double.Parse(ConfigurationManager.AppSettings.Get("driftOverGain"));
        static double driftOverOffset = double.Parse(ConfigurationManager.AppSettings.Get("driftOverOffset"));


        double voltagewrite = 0;
        static int splitPixel = int.Parse(ConfigurationManager.AppSettings.Get("splitPixel"));
        static int threshold = int.Parse(ConfigurationManager.AppSettings.Get("threshold"));
        static int pixelMargin = int.Parse(ConfigurationManager.AppSettings.Get("pixelMargin"));
        int patternLength = int.Parse(ConfigurationManager.AppSettings.Get("patternLength")); //Length of patterns

        static int bufferSize = int.Parse(ConfigurationManager.AppSettings.Get("bufferSize"));
        public static double[,] newdata = new double[bufferSize, 2];

        RawData currentData = new RawData(bufferSize);

        static TcAdsClient tcAds = new TcAdsClient();
        static AdsStream ds = new AdsStream(16);

        public double refLastValue = 0;
        public double angleLastValue = splitPixel;

        double x = 0;
        double xSquar = 0;
        double xCube = 0;
        double xFourth = 0;

        double[] yHighPass = new double[3];
        double[] xHighPass = new double[3];
        double[] yBandHighPass = new double[3];
        double[] xBandHighPass = new double[3];
        double[] yBandLowPass = new double[3];
        double[] xBandLowPass = new double[3];
        double[] highCoeff = new double[5];
        double[] bandLowCoeff = new double[5];
        double[] bandHighCoeff = new double[5];

        double[] yLowPass1 = new double[3];
        double[] xLowPass1 = new double[3];
        double[] yLowPass2 = new double[3];
        double[] xLowPass2 = new double[3];
        double[] lowCoeff = new double[5];

        public static string curDirec;

        int lightSourceStatus;
        int cameraStatus = 1;

        public DateTime initTime;
        System.Windows.Forms.Timer plotTimer = new System.Windows.Forms.Timer();
        public struct graphData
        {
            public ushort[] frame;
            public double angle;
            public graphData(ushort[] inFrame, double inAngle)
            {
                angle = inAngle;
                frame = inFrame;
            }
        }

        public Form1()
        {
            try
            {
                initTime = DateTime.Now;
                //Calls calculation method for filters
                highCoeff = filterCoeff(double.Parse(ConfigurationManager.AppSettings.Get("angleHighPass")), sampFreq / bufferSize, "High");
                lowCoeff = filterCoeff(double.Parse(ConfigurationManager.AppSettings.Get("angleLowPass")), sampFreq / bufferSize, "Low");
                bandHighCoeff = filterCoeff(double.Parse(ConfigurationManager.AppSettings.Get("velocityHighPass")), sampFreq / bufferSize, "High");
                bandLowCoeff = filterCoeff(double.Parse(ConfigurationManager.AppSettings.Get("velocityLowPass")), sampFreq / bufferSize, "Low");//2*10^-2

                InitializeComponent(); // Initializes the visual components
                SetSize(); // Sets up the size of the window and the corresponding location of the components
                Frameco = 0;
                dayFrameCo0 = DayNr.GetDayNr(DateTime.Now);
                
                //Initialization of TwinCAT connection. 851 is the port for PLC runtime 1

                if (twinCatBool)
                {                    
                    tcAds.Connect(851);
                }

                myStopwatch = new Stopwatch();
                dataWritingSyncEvent = new SyncEvents();

                consumerd = new DataConsumerDelegate[2];
                consumerd[0] = new DataConsumerDelegate(new DataConsumerDelegate.outputDelegate(showStatistics), true);
                consumerd[1] = new DataConsumerDelegate(new DataConsumerDelegate.outputDelegate(Pattern), true);
                consumerd[1].myThread.Priority = ThreadPriority.Highest;
                for (int i = 0; i < consumerd.Length; i++)
                {
                    consumerd[i].myThread.Start();
                }
                dataWritingQueue = new Queue<PeakQueueItem>();

                dataWritingThreadBool = true;
                dataWritingThread = new Thread(dataWrite);
                dataWritingThread.Priority = ThreadPriority.Highest;
                dataWritingThread.Start();
                Thread.Sleep(10);

                cameraThread = new Thread(initCamera);
                cameraThread.Priority = ThreadPriority.Highest;
                cameraThread.Start();
                curDirec = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                System.Diagnostics.Process.Start(curDirec + "\\AffinitySet.bat");
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception ex)
            {
                EmailError.emailAlert(ex);
                throw (ex);
            }
        }
        // Local data writing
        void dataWrite()
        {
            int f;
            PeakQueueItem curItem;
            long curTimeStamp;
        
            double[] lowPassedData;
            double[,] values;

            values = new double[10000, 2];
            curItem = new PeakQueueItem();
            while (dataWritingThreadBool)
            {
                if (quittingBool)
                {
                    return;
                }
                while ((WaitHandle.WaitAny(dataWritingSyncEvent.EventArray) != 1))
                {
                    if (quittingBool)
                    {
                        return;
                    }
                    lock (((ICollection)dataWritingQueue).SyncRoot)
                    {
                        curItem = dataWritingQueue.Dequeue();
                        if (dataWritingQueue.Count >= 1)
                        {
                            dataWritingSyncEvent.NewItemEvent.Set();
                        }
                    }
                    curTimeStamp = 0;
                    for (f = 0; f < bufferSize; f++)
                    {
                        curTimeStamp = curItem.TimeStamp[f];
                        values[f, 0] = curItem.Data[f, 0]; //Diff
                        values[f, 1] = curItem.Data[f, 1]; //Ref
                    }
                    lowPassedData = lp(values, bufferSize);
                    dataSend(lowPassedData);

                    f = f - 1;
                    if (recordBool)
                    {
                        myDataWriter.Write(curTimeStamp / sampFreq / 3600 / 24 + dayFrameCo0, lowPassedData);
                    }
                    if (graphWindow!=null && !graphWindow.IsDisposed)
                    {
                        int sumN = 0;
                        for (int i = 0; i < newdata.GetLength(0); i++)
                        {
                            if (newdata[i, 0] > 0)
                            {
                                graphSum += newdata[i, 0];
                                sumN ++;
                            }
                        }
                        graphSum = graphSum / ((double)sumN);
                        graphData outData = new graphData(frame, graphSum);
                        lock (graphLock)
                        {
                            graphQueue.Enqueue(outData);
                        }
                        graphSignal.Set();
                        graphSum = 0;                        
                    }
                }
            }
        }
        /*
         *  Takes a two dimensional array of doubles and int and returns an array of doubles
         *  Low pass filter for data
         */
        double[] lp(double[,] data, int frames)
        {
            double[] output = new double[bufferSize];
            double mainSum = 0;
            double refSum = 0;
            for (int i = 0; i < bufferSize; i++)
            {
                xLowPass1[0] = data[i, 0];

                yLowPass1[0] = lowCoeff[3] * yLowPass1[1] + lowCoeff[4] * yLowPass1[2] + lowCoeff[0] * xLowPass1[0] + lowCoeff[1] * xLowPass1[1] + lowCoeff[2] * xLowPass1[2];

                xLowPass1[2] = xLowPass1[1];
                xLowPass1[1] = xLowPass1[0];
                yLowPass1[2] = yLowPass1[1];
                yLowPass1[1] = yLowPass1[0];

                xLowPass2[0] = data[i, 1];

                yLowPass2[0] = lowCoeff[3] * yLowPass2[1] + lowCoeff[4] * yLowPass2[2] + lowCoeff[0] * xLowPass2[0] + lowCoeff[1] * xLowPass2[1] + lowCoeff[2] * xLowPass2[2];

                xLowPass2[2] = xLowPass2[1];
                xLowPass2[1] = xLowPass2[0];
                yLowPass2[2] = yLowPass2[1];
                yLowPass2[1] = yLowPass2[0];

                mainSum += yLowPass1[0];
                refSum += yLowPass2[0];
            }
            output[0] = mainSum / bufferSize;
            output[1] = refSum / bufferSize;
            return output;
        }
        // This method takes new data from the camera and inserts it, with proper timing, into the queue to be processed by the consumerd
        // the data consumer delegate that processes and displays the data
        public void onNewData(ushort[] data)
        {
            int i;
            int curFrame;
            Frameco++;
            if (quittingBool)
            {
                return;
            }
            curFrame = currentData.AddData(data, Frameco); // Returns the number of frames in the RawData object
            if (curFrame == bufferSize)
            {
                try
                {
                    for (i = 0; i < consumerd.Length; i++)
                    {
                        lock (((ICollection)consumerd[i].myQueue).SyncRoot)
                        {
                            consumerd[i].myQueue.Enqueue(currentData);
                        }
                        consumerd[i].mySyncEvent.NewItemEvent.Set();
                    }
                }
                catch (System.Threading.ThreadAbortException) { }
                catch (Exception ex)
                {
                    EmailError.emailAlert(ex);
                    throw (ex);
                }
                currentData = new RawData(bufferSize);
            }
        }
        //Camera initialization
        public void initCamera()
        {
            try
            {
                myCamera = new Camera();
                myCamera.cameraInit(cameraType);
                Camera.dataDelegate dd = new Camera.dataDelegate(onNewData);
                myCamera.startFrameGrab(0x8888, 0, dd, cameraType);
                cameraStatus = 1;
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception ex)
            {
                cameraStatus = 0;
                //Camera status bit
                ds = new AdsStream(4);
                BinaryWriter bw = new BinaryWriter(ds);
                bw.Write(cameraStatus);
                if (twinCatBool)
                {
                    tcAds.Write(0x4020, 40, ds);
                }
                EmailError.emailAlert(ex);
                throw (ex);
            }
        }

        //GUI display of queue length, time, and capacitor voltage
        private void showStatistics(RawData data)
        {
                double ti;
                ti = data.TimeStamp(0) * 1.0 / sampFreq;
                setTextBox1(ti.ToString("F1"));
                setTextBox4(String.Format("{0:0.00}", (voltagewrite)));                
        }
        //Calculates filter coefficients for a second order Butterworth of passed type
        double[] filterCoeff(double cutFreq, double sampFreq, string type)
        {
            double a1, a2, a3, b1, b2, c;
            double[] output = new double[5];
            if (type.Equals("Low") == true)
            {
                c = Math.Pow(Math.E, -2 * Math.PI * cutFreq / (sampFreq));
                a1 = Math.Pow((1.0 - c), 2);
                a2 = 0;
                a3 = 0;
                b1 = 2 * c;
                b2 = -Math.Pow(c, 2);
            }
            else if (type.Equals("High") == true)
            {
                c = Math.Pow(Math.E, -2 * Math.PI * cutFreq / (sampFreq));
                a1 = Math.Pow((1.0 + c) / 2, 2);
                a2 = -2 * a1;
                a3 = a1;
                b1 = 2 * c;
                b2 = -Math.Pow(c, 2);
                ;
            }
            else
            {
                a1 = 1; a2 = 0; a3 = 0; b1 = 0; b2 = 0;
            }
            output[0] = a1; output[1] = a2; output[2] = a3; output[3] = b1; output[4] = b2;
            return output;
        }
        // This functions processes a pattern, obtains the data and send it out to be written
        private void Pattern(RawData data)
        {
            PeakQueueItem quI;
            int ql;
            long[] timestamps;
            double fitLength = 15; //Amount of pixels to be used in fit of correlation 
            int halflength = (int)Math.Floor(fitLength / 2) + 1;  // increased by 1 pixel to allow two fits
            int length = patternLength; //Length of patterns
            double[] crossCor = new double[(int)fitLength + 2];   // increased by 2 pixels to allow two fits
            int startIndexRight = splitPixel; //Beginning of left pattern
            int startIndexLeft = 0; //Beginning of left pattern
            int endIndexRight = camWidth;
            int pixshift = 1;  // Direction of shift required to estimate slope of fit correction


            float sum = 0;
            double[] offset = new double[1];
            double[] fit = new double[4];
            double mu = 0;
            double mu1 = 0;
            double mu2 = 0;
            double N = fitLength;
            double y = 0;
            double xy = 0;
            double xxy = 0;
            double b, c, D, Db, Dc;// a,b, and c are the values we solve for then derive mu,sigma, and A from them.

            if (quittingBool)
            {
                return;
            }
            
            timestamps = new long[bufferSize];
            newdata = new double[bufferSize, 2];

            for (int frameNo = 0; frameNo < bufferSize; frameNo++)
            {
                try
                {
                    frame = data.getData(frameNo);
                    if (firstFrame == true)
                    {
                        refFrame = frame;
                        firstFrame = false;
                        for (int j = splitPixel; j < frame.Length; j++)
                        {
                            if (frame[j] > threshold)
                            {
                                startIndexRightRef = j - pixelMargin;
                                break;
                            }
                        }
                        for (int j = 0; j < frame.Length; j++)
                        {
                            if (frame[j] > threshold)
                            {
                                startIndexLeftRef = j - pixelMargin;
                                break;
                            }
                        }
                        x = 0;
                        xSquar = 0;
                        xCube = 0;
                        xFourth = 0;
                        for (int j = 0; j < fitLength; j++)
                        {
                            x += j;
                            xSquar += j * j;
                            xCube += j * j * j;
                            xFourth += j * j * j * j;
                        }
                    }
                    //Finds beginning of pattern using a threshold
                    
                    for (int j = splitPixel; j < frame.Length; j++)
                    {
                        if (frame[j] > threshold)
                        {
                            startIndexRight = j - pixelMargin;
                            break;
                        }
                        if (j == frame.Length-1)
                        {
                            startIndexRight = 0;
                        }
                    }
                    Array.Copy(frame, flipFrame, frame.Length);
                    Array.Reverse(flipFrame);

                    for (int j = 0; j < flipFrame.Length; j++)
                    {
                        if (flipFrame[j] > threshold)
                        {
                            endIndexRight = flipFrame.Length-j+pixelMargin;
                            break;
                        }
                        if (j == frame.Length - 1)
                        {
                            endIndexRight = flipFrame.Length;
                        }
                    }

                    if (startIndexRight >= frame.Length || startIndexRight <= splitPixel || (endIndexRight-startIndexRight) < length)                    
                    {
                        timestamps[frameNo] = data.TimeStamp(frameNo);
                        newdata[frameNo, 0] = angleLastValue;
                        newdata[frameNo, 1] = refLastValue;
                    }
                    else
                    {
                        //Cuts length of pattern down if the pattern extends beyond the frame
                        while (length + startIndexRight + halflength + 1 >= frame.Length)
                        {
                            length = (int)Math.Round(length / 1.1);
                        }
                        //Calcualtes the crosscorrelation between the two patterns at shifts ; first time
                        for (int k = -halflength; k <= halflength; k++)
                        {
                            sum = 0;
                            for (int m = 0; m < length; m++)
                            {
                                if ((m + startIndexRight + k) > 0 && (m + startIndexRightRef) > 0)
                                {
                                    if ((m + startIndexRight + k) < frame.Length && (m + startIndexRightRef) < refFrame.Length)
                                    {
                                        sum += frame[m + startIndexRight + k] * refFrame[m + startIndexRightRef];
                                    }
                                }
                            }
                            crossCor[k + halflength] = sum;
                        }
                        //Sums x,x^2,x^3,x^4,ln(y),x ln(y),x^2 ln(y)
                        y = 0;
                        xy = 0;
                        xxy = 0;
                        for (int j = 0; j < fitLength; j++)
                        {
                            y += Math.Log(crossCor[j + 1]);
                            xy += j * Math.Log(crossCor[j + 1]);
                            xxy += j * j * Math.Log(crossCor[j + 1]);
                        }
                        //Solves system of equations using Cramer's rule
                        D = N * (xSquar * xFourth - xCube * xCube) - x * (x * xFourth - xCube * xSquar) + xSquar * (x * xCube - xSquar * xSquar);
                        //Da = y * (xSquar * xFourth - xCube * xCube) - x * (xy * xFourth - xCube * xxy) + xSquar * (xy * xCube - xSquar * xxy);
                        Db = N * (xy * xFourth - xCube * xxy) - y * (x * xFourth - xCube * xSquar) + xSquar * (x * xxy - xy * xSquar);
                        Dc = N * (xSquar * xxy - xy * xCube) - x * (x * xxy - xy * xSquar) + y * (x * xCube - xSquar * xSquar);
                        //a = Da / D;
                        b = Db / D;
                        c = Dc / D;

                        mu1 = -b / (2 * c);

                        // If fit-center is to left of center of crosscor pattern, shift cross-cor pattern by 1 pixel to right or vice versa
                        if (mu1 < (halflength - 1))
                        {
                            pixshift = -1;
                        }
                        else
                        {
                            pixshift = 1;
                        }

                        //Redo the fit
                        y = 0;
                        xy = 0;
                        xxy = 0;
                        for (int j = 0; j < fitLength; j++)
                        {
                            y += Math.Log(crossCor[j + 1 + pixshift]);
                            xy += j * Math.Log(crossCor[j + 1 + pixshift]);
                            xxy += j * j * Math.Log(crossCor[j + 1 + pixshift]);
                        }
                        D = N * (xSquar * xFourth - xCube * xCube) - x * (x * xFourth - xCube * xSquar) + xSquar * (x * xCube - xSquar * xSquar);
                        Db = N * (xy * xFourth - xCube * xxy) - y * (x * xFourth - xCube * xSquar) + xSquar * (x * xxy - xy * xSquar);
                        Dc = N * (xSquar * xxy - xy * xCube) - x * (x * xxy - xy * xSquar) + y * (x * xCube - xSquar * xSquar);
                        b = Db / D;
                        c = Dc / D;

                        mu2 = -b / (2 * c);


                        mu = (halflength - 1) - (mu1 - (halflength - 1)) * pixshift / (mu2 - mu1);

                        newdata[frameNo, 0] = mu + startIndexRight;
                        timestamps[frameNo] = data.TimeStamp(frameNo);

                        angleLastValue = mu + startIndexRight;



                        y = 0;
                        xy = 0;
                        xxy = 0;
                        //Finds beginning of pattern using a threshold
                        if (frameNo == 0)
                        {
                            for (int j = 0; j < frame.Length; j++)
                            {
                                if (frame[j] > threshold)
                                {
                                    startIndexLeft = j - pixelMargin;
                                    lightSourceStatus = 1;
                                    break;
                                }

                                if (j == frame.Length - 1)
                                {
                                    lightSourceStatus = 0;
                                }
                            }
                            if (startIndexLeft < 0)
                            {
                                startIndexLeft = 0;
                            }
                        }

                        //Calcualtes the crosscorrelation between the two patterns at shifts 
                        for (int k = -halflength; k <= halflength; k++)
                        {
                            sum = 0;
                            for (int m = 0; m < length; m++)
                            {
                                if ((m + startIndexLeft + k) > 0 && (m + startIndexLeftRef) > 0)
                                {
                                    sum += frame[m + startIndexLeft + k] * refFrame[m + startIndexLeftRef];
                                }
                            }
                            crossCor[k + halflength] = sum;
                        }
                        //Sums x,x^2,x^3,x^4,ln(y),x ln(y),x^2 ln(y)

                        for (int j = 0; j < fitLength; j++)
                        {
                            y += Math.Log(crossCor[j + 1]);
                            xy += j * Math.Log(crossCor[j + 1]);
                            xxy += j * j * Math.Log(crossCor[j + 1]);
                        }
                        //Solves system of equations using Cramer's rule
                        D = N * (xSquar * xFourth - xCube * xCube) - x * (x * xFourth - xCube * xSquar) + xSquar * (x * xCube - xSquar * xSquar);
                        //Da = y * (xSquar * xFourth - xCube * xCube) - x * (xy * xFourth - xCube * xxy) + xSquar * (xy * xCube - xSquar * xxy);
                        Db = N * (xy * xFourth - xCube * xxy) - y * (x * xFourth - xCube * xSquar) + xSquar * (x * xxy - xy * xSquar);
                        Dc = N * (xSquar * xxy - xy * xCube) - x * (x * xxy - xy * xSquar) + y * (x * xCube - xSquar * xSquar);
                        //a = Da / D;
                        b = Db / D;
                        c = Dc / D;

                        mu1 = -b / (2 * c);

                        // If fit-center is to left of center of crosscor pattern, shift cross-cor pattern by 1 pixel to right or vice versa
                        if (mu1 < (halflength - 1))
                        {
                            pixshift = -1;
                        }
                        else
                        {
                            pixshift = 1;
                        }

                        //Redo the fit
                        y = 0;
                        xy = 0;
                        xxy = 0;
                        for (int j = 0; j < fitLength; j++)
                        {
                            y += Math.Log(crossCor[j + 1 + pixshift]);
                            xy += j * Math.Log(crossCor[j + 1 + pixshift]);
                            xxy += j * j * Math.Log(crossCor[j + 1 + pixshift]);
                        }
                        D = N * (xSquar * xFourth - xCube * xCube) - x * (x * xFourth - xCube * xSquar) + xSquar * (x * xCube - xSquar * xSquar);
                        Db = N * (xy * xFourth - xCube * xxy) - y * (x * xFourth - xCube * xSquar) + xSquar * (x * xxy - xy * xSquar);
                        Dc = N * (xSquar * xxy - xy * xCube) - x * (x * xxy - xy * xSquar) + y * (x * xCube - xSquar * xSquar);
                        b = Db / D;
                        c = Dc / D;

                        mu2 = -b / (2 * c);

                        mu = (halflength - 1) - (mu1 - (halflength - 1)) * pixshift / (mu2 - mu1);

                        newdata[frameNo, 1] = mu + startIndexLeft;
                        refValue = mu + startIndexLeft;
                        newdata[frameNo, 0] = newdata[frameNo, 0] - refValue;
                        refLastValue = refValue;
                    }
                }
                catch (System.Threading.ThreadAbortException) { }
                catch (Exception ex)
                {
                    EmailError.emailAlert(ex);
                    timestamps[frameNo] = data.TimeStamp(frameNo);
                    newdata[frameNo, 0] = angleLastValue;
                    newdata[frameNo, 1] = refLastValue;
                    throw ex;
                }

                sum = frame.Select(x => (int)x).Sum();
                Debug.WriteLine(sum);
                if (sum == 0)
                {
                    lightSourceStatus = 0;
                }
                else
                {
                    lightSourceStatus = 1;
                }
            }

            quI = new PeakQueueItem(timestamps, newdata);
            lock (((ICollection)dataWritingQueue).SyncRoot)
            {
                while (dataWritingQueue.Count>10)
                {
                    dataWritingQueue.Dequeue();
                }
                dataWritingQueue.Enqueue(quI);
                ql = dataWritingQueue.Count;
            }
            dataWritingSyncEvent.NewItemEvent.Set();
            setTextBox2(data.QueueLen.ToString());
            setTextBox3(ql.ToString());

            if (quittingBool)
            {
                return;
            }

        }

        //Prepares data to be sent to TwinCAT software
        private void dataSend(double[] data)
        {
            double tilt;
            double drift = 0;
            double velocity = 0;
            double angle = 0;
            double refAng = 0;
            angle = data[0];
            refAng = refGain * (data[1] - refZeroValue);
            //Has a DC subtraction to help filters. Should be about the center of the signal.
            if (firstValueCounter < 40)
            {
                firstValueCounter++;
                zeroValue = data[0];
                refZeroValue = data[1];
            }
            xHighPass[0] = angle - zeroValue;
            xBandLowPass[0] = angle - zeroValue;

            //Drift signal calculation, just scaled signal
            if (dampOverride)
            {
                drift = driftOverGain * (angle - driftOverOffset);
            }
            else
            {
                drift = driftGain * (angle - driftOffset);
            }
            //Drift rail logic
            if (Math.Abs(drift) >= 32760)
            {
                drift = Math.Sign(drift) * 32760;

            }

            //Tilt signal calculations, high pass at 10^-3 Hz then scaling.
            yHighPass[0] = highCoeff[3] * yHighPass[1] + highCoeff[4] * yHighPass[2] + highCoeff[0] * xHighPass[0] + highCoeff[1] * xHighPass[1] + highCoeff[2] * xHighPass[2];
            tilt = angleGain * yHighPass[0];

            //Capacitor signal calculations, low passed at Hz then high passed at Hz then differentiated and scaled
            yBandLowPass[0] = bandLowCoeff[3] * yBandLowPass[1] + bandLowCoeff[4] * yBandLowPass[2] + bandLowCoeff[0] * xBandLowPass[0] + bandLowCoeff[1] * xBandLowPass[1] + bandLowCoeff[2] * xBandLowPass[2];
            xBandHighPass[0] = yBandLowPass[0];
            yBandHighPass[0] = bandHighCoeff[3] * yBandHighPass[1] + bandHighCoeff[4] * yBandHighPass[2] + bandHighCoeff[0] * xBandHighPass[0] + bandHighCoeff[1] * xBandHighPass[1] + bandHighCoeff[2] * xBandHighPass[2];
            velocity = velGain* Math.Round((double)200 / bufferSize) * (yBandHighPass[0] - yBandHighPass[1]);

           
            if (velocity > 0)
            {
                velocity = Math.Sqrt(velocity);
            }
            else
            {
                velocity = -Math.Sqrt(-velocity);
            }

            if (Math.Abs(velocity) >= 30760)
            {
                velocity = Math.Sign(velocity) * 30760;
            }
            if (twinCatBool)
            {
                ds = new AdsStream(28);
                BinaryWriter bw = new BinaryWriter(ds);
                //Tilt signal. TwinCAT variable tilt at MW0.
                bw.Write((int)tilt);
                //Drift signal. TwinCAT variable drift at MW1
                bw.Write((int)drift);
                //Velocity signal. TwinCAT variable cap at MW2
                bw.Write((int)velocity);
                voltagewrite = velocity / 3276;
                //Reference signal. TwinCAT variable ref at MW3
                bw.Write((int)refAng);
                //C# pulse. Sets TwinCAT variable cPulse=1 at MW4
                bw.Write((int)1);
                //Light source status bit at MW5
                bw.Write((int)lightSourceStatus);
                //Camera status bit at MW6
                bw.Write((int)cameraStatus);
                tcAds.Write(0x4020, 0, ds);
            }

            xHighPass[2] = xHighPass[1];
            xHighPass[1] = xHighPass[0];
            yHighPass[2] = yHighPass[1];
            yHighPass[1] = yHighPass[0];
            xBandLowPass[2] = xBandLowPass[1];
            xBandLowPass[1] = xBandLowPass[0];
            yBandLowPass[2] = yBandLowPass[1];
            yBandLowPass[1] = yBandLowPass[0];
            xBandHighPass[2] = xBandHighPass[1];
            xBandHighPass[1] = xBandHighPass[0];
            yBandHighPass[2] = yBandHighPass[1];
            yBandHighPass[1] = yBandHighPass[0];
        }


        //===========================GUI=====================

        public void setTextBox1(string inString)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.BeginInvoke(
                   new MethodInvoker(
                   delegate () { setTextBox1(inString); }));
            }
            else
            {
                textBox1.Text = inString;
            }

        }

        public void setTextBox2(string inString)
        {
            if (textBox2.InvokeRequired)
            {
                textBox2.BeginInvoke(
                   new MethodInvoker(
                   delegate () { setTextBox2(inString); }));
            }
            else
            {
                textBox2.Text = inString;
            }
        }

        public void setTextBox3(string inString)
        {
            if (textBox3.InvokeRequired)
            {
                textBox3.BeginInvoke(
                   new MethodInvoker(
                   delegate () { setTextBox3(inString); }));
            }
            else
            {
                textBox3.Text = inString;
            }
        }
        public void setTextBox4(string inString)
        {
            if (textBox4.InvokeRequired)
            {
                textBox4.BeginInvoke(
                   new MethodInvoker(
                   delegate () { setTextBox4(inString); }));
            }
            else
            {
                textBox4.Text = inString;
            }
        }

        /*
         *  Sets the size of the box and the corresponding locations of all relevant graphical components
         */
        public void SetSize()
        {
            int iS, lay, coy;
            iS = 10;
            lay = 4;
            coy = 20;
            label1.Location = new Point(iS, lay);
            textBox1.Location = new Point(iS, coy); iS = iS + textBox1.Width;
            label2.Location = new Point(iS, lay);
            textBox2.Location = new Point(iS, coy); iS = iS + textBox2.Width;
            label3.Location = new Point(iS, lay);
            textBox3.Location = new Point(iS, coy); iS = iS + textBox3.Width;
            label4.Location = new Point(iS, lay);
            textBox4.Location = new Point(iS, coy); iS = iS + textBox4.Width;
            label5.Location = new Point(iS, lay);
            numericUpDown1.Location = new Point(iS, coy); iS = iS + numericUpDown1.Width;
            label6.Location = new Point(iS, lay);
            numericUpDown2.Location = new Point(iS, coy); iS = iS + numericUpDown2.Width;
            label7.Location = new Point(iS, lay);
            numericUpDown3.Location = new Point(iS, coy); iS = iS + numericUpDown3.Width;
            buRecord.Location = new Point(ClientRectangle.Width - buRecord.Size.Width - 10, 20);
            buGraph.Location = new Point(ClientRectangle.Width - buGraph.Size.Width - buRecap.Size.Width - 20, ClientRectangle.Height - buGraph.Size.Height - 20);
            buClear.Location = new Point(ClientRectangle.Width - buGraph.Size.Width - buRecap.Size.Width - 20, ClientRectangle.Height - buClear.Size.Height - 50);
            buRecap.Location = new Point(ClientRectangle.Width - buRecap.Size.Width - 10, ClientRectangle.Height - buRecap.Size.Height - 50);
            buDamp.Location = new Point(ClientRectangle.Width - buDamp.Size.Width - 10, ClientRectangle.Height - buDamp.Size.Height - 20);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            int i;
            quittingBool = true;
            cameraThread.Abort();
            myCamera.stopFrameGrab(cameraType);
            dataWritingSyncEvent.ExitThreadEvent.Set();
            for (i = 0; i < consumerd.Length; i++)
            {
                consumerd[i].mySyncEvent.ExitThreadEvent.Set();
            }

            for (i = 0; i < consumerd.Length; i++)
            {
                consumerd[i].myThread.Join();
            }
            dataWritingThreadBool = false;
            Environment.Exit(1);
        }

        private void Form2_Resize(object sender, System.EventArgs e)
        {
            graphWindow.imagePlot.Size = new Size((graphWindow.ClientRectangle.Width / 2)-40, graphWindow.ClientRectangle.Height - 40);
            graphWindow.anglePlot.Size = new Size((graphWindow.ClientRectangle.Width / 2) - 40, graphWindow.ClientRectangle.Height - 40);
            graphWindow.imagePlot.Location = new Point(20, 20);
            graphWindow.anglePlot.Location = new Point((graphWindow.ClientRectangle.Width / 2)-20, 20);
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            graphWindow.stopLoop();
            graphWindow.Hide();
            e.Cancel = true;
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            SetSize();
        }
        private void buGraph_Click(object sender, EventArgs e)
        {
            if (graphWindow == null) { 
                graphWindow = new Form2();
                graphWindow.FormClosing += new FormClosingEventHandler(Form2_FormClosing);
                graphWindow.ClientSizeChanged += new EventHandler(Form2_Resize);
                graphThread = new Thread(Program.Main2);
                graphThread.SetApartmentState(ApartmentState.STA);
                graphThread.Start();
                graphWindow.Show();
                plotTimer.Tick+= new EventHandler(plotTime_Tick);
                plotTimer.Interval = 5;
                plotTimer.Enabled = true;
            }
            else
            {
                graphWindow.Show();
                graphWindow.BringToFront();
            }
        }
        private void plotTime_Tick(object Sender, EventArgs e)
        {
            graphWindow.updateAxis();
        }
        private void buRecord_Click(object sender, EventArgs e)
        {
            if (recordBool == false)
            {
                Frameco = 0;
                dayFrameCo0 = DayNr.GetDayNr(DateTime.Now);
                buRecord.Text = "Stop Recording";
                myDataWriter = new DataWriting();
                recordBool = true;
            }
            else
            {

                buRecord.Text = "Record to Disk";
                recordBool = false;
                myDataWriter.stopit();
            }
        }
        private void buClear_Click(object sender, EventArgs e)
        {
            try {
                graphWindow.anglePlot.Series[0].Values.Clear();
                graphWindow.dataPointNum = 0;
                graphWindow.angleMax = 0;
                graphWindow.angleMin = Math.Pow(10, 100);
            }
            catch { }

        }
        private void buRecap_Click(object sender, EventArgs e)
        {
            firstFrame = true;
        }
        private void buDamp_Click(object sender, EventArgs e)
        {
            if (dampOverride)
            {
                buDamp.BackColor = System.Drawing.Color.LightGray;
                dampOverride = false;
            }
            else
            {
                buDamp.BackColor = System.Drawing.Color.Red;
                dampOverride = true;
            }

        }
        

    }
}
//}