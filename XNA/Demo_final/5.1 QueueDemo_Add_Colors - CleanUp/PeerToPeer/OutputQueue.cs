using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net;

namespace PeerToPeer
{
    class OutputQueue
    {
        PacketWriter packetWriter = new PacketWriter();
    
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

                        // Always push to network (wether it's local or external)
                        // Write the tank state into a network packet.
                        packetWriter.Write(qH.inSeqNum);
                        packetWriter.Write(qH.outSeqNum);
                        packetWriter.Write((int)qH.type);
                        packetWriter.Write((int)qBird.type);
                        packetWriter.Write(qBird.x);
                        packetWriter.Write(qBird.y);
                        packetWriter.Write(qBird.gamerIndex);

                        // Send the data to everyone in the session.
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);

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
