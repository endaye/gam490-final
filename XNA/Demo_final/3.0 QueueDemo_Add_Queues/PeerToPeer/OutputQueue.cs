using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Net;

namespace PeerToPeer
{

    class OutputQueue
    {
    
        public void pushToNetwork( LocalNetworkGamer localGamer )
        {
           
            int count = outQ.Count;
            for (int i = 0; i < count; i++)
            {
                // Read the header
                QueueHdr qH = outQ.Dequeue();
                
                switch (qH.type )
                {
                    case Queue_type.QUEUE_BIRD:
                        // Read the correct type of data
                        Bird_Data qBird = (Bird_Data)qH.data;
                      
                        //Debug.WriteLine("outQ -->  inSeqNum:{0,4} outSeqNum:{1}  {2}->{3}", qH.inSeqNum, qH.outSeqNum,qH.type, qTank.type);
                        
                        if (qBird.gamerIndex == 0)
                        {
                            // push to local input queue

                            // Reason it's output, local is controlling it
                            // so your are outputing stimulus to the system.
                            // ONLY way you can input data, it's coming into the inputQueue from network
                            // This data will be propagated to the local machine (inTankQueue) and transmitted to network

                            Bird_inQueue.add(qBird, qH.outSeqNum);
                        }

                      
                        // Future move data to the network here.

                        break;

                }
            }
        }

        static public System.Collections.Generic.Queue<QueueHdr> outQ = new System.Collections.Generic.Queue<QueueHdr> ();
        static public int seqNumGlobal = 9000;
    }


  class Bird_outQueue
    {

        public static void add(Bird_Data d)
        {
            QueueHdr qH;
            qH.type = Queue_type.QUEUE_BIRD;
            qH.outSeqNum = OutputQueue.seqNumGlobal;
            qH.inSeqNum = -1;
            qH.data = d;

            OutputQueue.seqNumGlobal++;

            // add the to input Queue
            OutputQueue.outQ.Enqueue(qH);
        }
    }



   
}
