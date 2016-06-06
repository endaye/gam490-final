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
                        packetWriter.Write((int)qShipRS.playerId);
                        packetWriter.Write(qShipRS.rotation);
                        packetWriter.Write(qShipRS.impulse);
                        packetWriter.Write(qShipRS.missle);
                        packetWriter.Write(qShipRS.bomb);
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);
                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}, Player.{3}, rot {4}, imp {5}",
                            qH.inSeqNum, qH.outSeqNum, qH.type, qShipRS.playerId, qShipRS.rotation, qShipRS.impulse);
                        break;

                    case QueueType.SHIP_MISSILE_SR:
                        Ship_Missile_SR qShipMissSR = (Ship_Missile_SR)qH;
                        packetWriter.Write((int)qShipMissSR.playerId);
                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}, Player.{3} launchs a missile",
                           qH.inSeqNum, qH.outSeqNum, qH.type, qShipMissSR.playerId);
                        break;

                    case QueueType.SHIP_BOMB_SR:
                        Ship_Bomb_SR qShipBombSR = (Ship_Bomb_SR)qH;
                        packetWriter.Write((int)qShipBombSR.playerId);
                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}, Player.{3} launchs a bomb",
                           qH.inSeqNum, qH.outSeqNum, qH.type, qShipBombSR.playerId);
                        break;

                    case QueueType.PHYSICS_SR:
                        // Read the correct type of data
                        Physics_SR qPhysSR = (Physics_SR)qH;
                        packetWriter.Write(qPhysSR.count);
                        for (int j = 0; j < qPhysSR.count; j++ )
                        {
                            packetWriter.Write(qPhysSR.pBuffer[j].id);
                            packetWriter.Write(qPhysSR.pBuffer[j].rot);
                            packetWriter.Write(qPhysSR.pBuffer[j].pos.X);
                            packetWriter.Write(qPhysSR.pBuffer[j].pos.Y);
                        }
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);
                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}, Count {3}",
                           qH.inSeqNum, qH.outSeqNum, qH.type, qPhysSR.count);
                        break;

                    case QueueType.COL_EVENT_SR:
                        Col_Event_SR qColEvent_SR = (Col_Event_SR)qH;
                        packetWriter.Write(qColEvent_SR.GameObjA_ID);
                        packetWriter.Write(qColEvent_SR.GameObjB_ID);
                        packetWriter.Write(qColEvent_SR.ColPos.X);
                        packetWriter.Write(qColEvent_SR.ColPos.Y);
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);
                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}, A_ID#{3}, B_ID#{4}, Pos [{5,2}, {6,2}] ",
                            qH.inSeqNum, qH.outSeqNum, qH.type, qColEvent_SR.GameObjA_ID, qColEvent_SR.GameObjB_ID, qColEvent_SR.ColPos.X, qColEvent_SR.ColPos.Y);
                        break;
                        
                    default:
                        break;
                }
            }
        }
    }
}
