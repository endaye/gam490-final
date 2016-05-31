using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net;

namespace PeerToPeer
{
    public enum Queue_type
    {
        QUEUE_BIRD,
        QUEUE_SND,  // future use, just an example
        QUEUE_BLAH  // future use, just an example
    }

    public struct QueueHdr
    {
        public int inSeqNum;
        public int outSeqNum;
        public Queue_type type;
        public object data;
    }

    public class InputQueue
    {
// Bang 6
        public void pullFromNetwork(LocalNetworkGamer localGamer)
        {
            while (localGamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                localGamer.ReceiveData(packetReader, out sender);

                // Write the tank state into a network packet.
                Bird_Data qBird;
                QueueHdr qH;

                qH.inSeqNum = packetReader.ReadInt32();
                qH.outSeqNum = packetReader.ReadInt32();
                qH.type = (Queue_type)packetReader.ReadInt32();

                qBird.type = (Bird_Type)packetReader.ReadInt32();
                qBird.x = packetReader.ReadSingle();
                qBird.y = packetReader.ReadSingle();
                qBird.gamerIndex = packetReader.ReadInt32();

                Bird.insertInQueue(qBird, qH.outSeqNum);
            }
        }

        public void process()
        {
            // Number of elements in queue
            int count = inQ.Count;

            // Loop through and process them
            for (int i = 0; i < count; i++)
            {
                // Read the header
                QueueHdr qH = inQ.Dequeue();

                switch (qH.type)
                {
                    case Queue_type.QUEUE_BIRD:

                        // Read the correct type of data
                        Bird_Data qBird = (Bird_Data)qH.data;

                        //Debug.WriteLine(" inQ -->  inSeqNum:{0,4} outSeqNum:{1} ", qH.inSeqNum, qH.outSeqNum);

                        // Call the update on the correct tank
                        pBird[qBird.gamerIndex].Update(qBird);
                        break;
                }
            }
        }

        // DATA: ---------------------------------------------------------------------------------
        PacketReader packetReader = new PacketReader(); 
        static public System.Collections.Generic.Queue<QueueHdr> inQ = new System.Collections.Generic.Queue<QueueHdr>();
        static public int seqNumGlobal = 3111;
        static public Bird[] pBird = new Bird[2];   // Holds 2 pointers to Bird objects
    }

// Bang 6
    //class Bird_inQueue
    //{
    //    public static void add(Bird_Data d, int outSeqNum)
    //    {
    //        QueueHdr qH;
    //        qH.type = Queue_type.QUEUE_BIRD;
    //        qH.inSeqNum = InputQueue.seqNumGlobal;
    //        qH.outSeqNum = outSeqNum;
    //        qH.data = d;
//
    //        InputQueue.seqNumGlobal++;
//
    //        // add the to input Queue
    //        InputQueue.inQ.Enqueue(qH);
    //    }
    //}



}
