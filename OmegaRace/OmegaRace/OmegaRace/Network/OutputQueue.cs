using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net;
using CollisionManager;

namespace OmegaRace
{
    public enum QueueType
    {
        SHIP_RS,        // remote to server Ship action state
        SHIP_SR,        // server to remote Ship pos & rot
        OTHER,
    }

    struct NetworkData
    {
        public int  inSeqNum;
        public int outSeqNum;
        public QueueType type;
        public Object data;
    }

    class OutputQueue
    {
        #region Fields

        private static OutputQueue instance;

        private static System.Collections.Generic.Queue<NetworkData> _q = new System.Collections.Generic.Queue<NetworkData>();

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

        public void add(Object _data)
        {
            NetworkData outData;
            outData.outSeqNum = getOutSeqNum();
            outData.inSeqNum = -1;
            if (_data.GetType().Equals(typeof(ShipData_RS))) 
            {
                outData.type = QueueType.SHIP_RS;
            }
            else if (_data.GetType().Equals(typeof(ShipData_SR))) 
            {
                outData.type = QueueType.SHIP_SR;
            } 
            else
            {
                outData.type = QueueType.OTHER;
            }
            outData.data = _data;
            _q.Enqueue(outData);
        }

        public void pushToNetwork(LocalNetworkGamer localGamer)
        {
            int count = _q.Count();
            for (int i = 0; i < count; i++)
            {
                // Read the header
                NetworkData qH = _q.Dequeue();

                switch (qH.type)
                {
                    case QueueType.SHIP_RS:
                        // Read the correct type of data
                        ShipData_RS qShipRS = (ShipData_RS)qH.data;

                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, Player {4}", 
                            qH.inSeqNum, qH.outSeqNum, qH.type, qShipRS.GetType(), qShipRS.playerId);
                        
                        // Always push to network (wether it's local or external)
                        // Write the tank state into a network packet.
                        packetWriter.Write(qH.inSeqNum);
                        packetWriter.Write(qH.outSeqNum);
                        packetWriter.Write((int)qH.type);
                        packetWriter.Write((int)qShipRS.playerId);
                        packetWriter.Write(qShipRS.rotation);
                        packetWriter.Write(qShipRS.impulse);

                        // Send our input data to the server.
                        //localGamer.SendData(packetWriter, SendDataOptions.InOrder, host);
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);
                        break;

                    case QueueType.SHIP_SR:
                        // Read the correct type of data
                        ShipData_SR qShipSR = (ShipData_SR)qH.data;
                        Debug.WriteLine("Send -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, Player {4}", 
                            qH.inSeqNum, qH.outSeqNum, qH.type, qShipSR.GetType(), qShipSR.playerId);
                        
                        // Always push to network (wether it's local or external)
                        // Write the tank state into a network packet.
                        packetWriter.Write(qH.inSeqNum);
                        packetWriter.Write(qH.outSeqNum);
                        packetWriter.Write((int)qH.type);
                        packetWriter.Write((int)qShipSR.playerId);
                        packetWriter.Write(qShipSR.x);
                        packetWriter.Write(qShipSR.y);
                        packetWriter.Write(qShipSR.rot);

                        // Send the data to everyone in the session.
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);

                        break;

                    default:
                        break;
                }
            }
        }
    }
}
