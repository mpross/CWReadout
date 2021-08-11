using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


    //
    // This class defines the delegate process for data operations
    //
    class DataConsumerDelegate
    {
        public delegate void outputDelegate(RawData dat);

        public volatile SyncEvents mySyncEvent;
        public volatile Queue<RawData> myQueue;
        public Thread myThread;

        private Queue<RawData> _queue;
        private SyncEvents _syncEvents;
        private outputDelegate _outputDeletgate;
        private bool _tight;


        public DataConsumerDelegate(outputDelegate dd, bool tight)
        {
            mySyncEvent = new SyncEvents();
            myQueue = new Queue<RawData>();
            myThread =  new Thread(this.ThreadRun);
            _tight = tight;
            _queue=myQueue;
            _syncEvents=mySyncEvent;
            _outputDeletgate = dd;
        }

        public DataConsumerDelegate(Queue<RawData> q, SyncEvents e, outputDelegate dd)
        {
            _queue = q;
            _syncEvents = e;
            _outputDeletgate = dd;
        }
    // Consumer.ThreadRun
    public void ThreadRun()
    {
        int count = 0;
        RawData temI; 
       
       while ((WaitHandle.WaitAny(_syncEvents.EventArray) != 1)) // || (oquelen > 1))
       {
           lock (((ICollection)_queue).SyncRoot)
           {
               temI = _queue.Dequeue();
               temI.QueueLen = _queue.Count();

               //if (temI.QueueLen > 5) 
               //{
               //    temI = _queue.Dequeue();
               //    temI.QueueLen--;
               //} 
                              
           }
           temI.QueueLen = _queue.Count();
           _outputDeletgate(temI);

            if (temI.QueueLen > 1)
            {
                mySyncEvent.NewItemEvent.Set();
            }
           temI = null;

           count++;
           Thread.Sleep(0);

       }  
    }
}