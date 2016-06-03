using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net;
using CollisionManager;

namespace OmegaRace
{
    class OutputQueue
    {
        #region Fields

        private static OutputQueue instance;

        private static System.Collections.Generic.Queue<Message> _q = new System.Collections.Generic.Queue<Message>();

        private PacketWriter packetWriter = new PacketWriter();

        private static int outSeqNumGlobal = 0;

        private static int getOutSeqNum()
        {
            return outSeqNumGlobal++;
        }

        private OutputQueue()
        {
            // use singleton
        }

        ~OutputQueue()
        {
            // do nothing
        }

        public static OutputQueue Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OutputQueue();
                }
                return instance;
            }
        }

        #endregion

        public void add(Message outData)
        {
            outData.outSeqNum = getOutSeqNum();
            outData.inSeqNum = -1;
            outData.type = outData.getQueueType();
            _q.Enqueue(outData);
        }

        public void pushToNetwork(LocalNetworkGamer localGamer)
        {
            int count = _q.Count();
            for (int i = 0; i < count; i++)
            {
                // Read the header
                Message qH = _q.Dequeue();

                // Always push to network (wether it's local or external)
                packetWriter.Write(qH.inSeqNum);
                packetWriter.Write(qH.outSeqNum);
                packetWriter.Write((int)qH.type);

                switch (qH.type)
                {
                    case QueueType.SHIP_RS:
                        // Read the correct type of data
                        Ship_RS qShipRS = (Ship_RS)qH;
                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, Player {4}, rot {5}, imp {6}", 
                            qH.inSeqNum, qH.outSeqNum, qH.type, qShipRS.type, qShipRS.playerId, qShipRS.rotation, qShipRS.impulse);
                        packetWriter.Write((int)qShipRS.playerId);
                        packetWriter.Write(qShipRS.rotation);
                        packetWriter.Write(qShipRS.impulse);
                        packetWriter.Write(qShipRS.missle);
                        packetWriter.Write(qShipRS.bomb);

                        // Send the data to everyone in the session.
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);
                        break;

                    case QueueType.PHYSICS_SR:
                        // Read the correct type of data
                        Physics_SR qPhysSR = (Physics_SR)qH;
                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, Count {4}", 
                            qH.inSeqNum, qH.outSeqNum, qH.type, qPhysSR.type, qPhysSR.count);
                        
                        packetWriter.Write(qPhysSR.count);
                        for (int j = 0; j < qPhysSR.count; j++ )
                        {
                            packetWriter.Write(qPhysSR.pBuffer[j].id);
                            packetWriter.Write(qPhysSR.pBuffer[j].rot);
                            packetWriter.Write(qPhysSR.pBuffer[j].pos.X);
                            packetWriter.Write(qPhysSR.pBuffer[j].pos.Y);
                        }

                        // Send the data to everyone in the session.
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);

                        break;

                    //case QueueType.COL_EVENT_SR:
                    //    GameObjMsg_SR qGameSR = (GameObjMsg_SR)qH.data;
                    //    Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, GameObjID {4} #{5}",
                    //        qH.inSeqNum, qH.outSeqNum, qH.type, qGameSR.GetType(), qGameSR.gameObjId, qGameSR.state);
                    //    // Always push to network (wether it's local or external)
                    //    // Write the tank state into a network packet.
                    //    packetWriter.Write(qH.inSeqNum);
                    //    packetWriter.Write(qH.outSeqNum);
                    //    packetWriter.Write((int)qH.type);
                    //    packetWriter.Write(qGameSR.gameObjId);
                    //    packetWriter.Write((int)qGameSR.state);
                    //    break;
                    default:
                        break;
                }
            }
        }
    }
}
