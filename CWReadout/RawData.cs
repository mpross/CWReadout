using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BRSReadout;

    /*
     * This object takes arrays of unsigned shorts and stores them.
     */
    class RawData
    {
        int Frames = 2048*2;
        ushort[,] data; // First index is the data count and second index is the actual data corresponding to that data count
        private long[] timeStamp;
        int length;
        int queueLen;
        int dataco; // The number of data frames in the Object

        public RawData(int Fr)
        {
            length = 0;
            dataco=0;
            Frames = Fr;

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        // ctor which takes in data
        public RawData(ushort[] inData, long ts, int Fr) : this(Fr)
        {
            AddData(inData,ts);
        }

        // Takes an array of unsigned shorts and a time stamp and puts them into the data and timestamp arrays
        public int AddData(ushort[] inData, long ts)
        {
            int i;
            if (dataco == 0)
            {
                timeStamp = new long[Frames];
                data = new ushort[Frames, inData.Length];
            }
            length = inData.Length;
            for (i = 0; i < length; i++) data[dataco, i] = inData[i];
            timeStamp[dataco] = ts;
            dataco++;
            return dataco;
        }

        public int FramesNo
        {
            get { return dataco; }
        }
        
        public int MaxFrames
        {
            get { return Frames; }
        }

        public int Length
        {
            get { return length; }
        }
        
        public ushort[] getData(int frame)
        {
            int i;
            ushort[] ret;
            ret = new ushort[length];
            for (i = 0; i < length; i++) ret[i] = data[frame, i];
            return ret;
        }

        public long TimeStamp(int frame)
        {
            return timeStamp[frame]; 
        }

        public double getelem(int frame,int i)
        {
            return data[frame,i];
        }

        public int QueueLen
        {
        get {return queueLen;}
            set { queueLen = value; }
    }
    }

