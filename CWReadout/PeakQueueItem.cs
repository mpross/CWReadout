using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



public class PeakQueueItem
{
    double[,] data;
    long[] timeStamp;

    public PeakQueueItem() 
    { 
    }
    public PeakQueueItem(long[] time, double[,] newdata)
    {
        data = newdata;
        timeStamp = time;
    }

    public long[] TimeStamp { get { return timeStamp; } }
    public double[,] Data { get {return data;}} 
    public int FramesNo { get { return timeStamp.Length; } }
}

