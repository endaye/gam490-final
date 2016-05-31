using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CollisionManager;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework;

namespace OmegaRace
{

    class InputQueue
    {
        #region Fields

        private static InputQueue instance;

        private static System.Collections.Generic.Queue<NetworkData> _q = new System.Collections.Generic.Queue<NetworkData>();

        private PacketReader packetReader = new PacketReader();

        private static int inSeqNumGlobal = 0;

        private static int getInSeqNum()
        {
            return inSeqNumGlobal++;
        }

        private InputQueue()
        {
            // use singleton
        }

        ~InputQueue()
        {
            // do nothing
        }

        public static InputQueue Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InputQueue();
                }
                return instance;
            }
        }

        #endregion

        public void add(NetworkData _data)
        {
            _data.inSeqNum = getInSeqNum();
            _q.Enqueue(_data);
        }

        public void pullFromNetwork(LocalNetworkGamer localGamer)
        {
            while (localGamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                localGamer.ReceiveData(packetReader, out sender);

                // Write the tank state into a network packet.
                NetworkData qH;

                qH.inSeqNum = packetReader.ReadInt32();
                qH.outSeqNum = packetReader.ReadInt32();
                qH.type = (QueueType)packetReader.ReadInt32();
                switch (qH.type)
                {
                    case QueueType.SHIP_RS:
                        ShipData_RS qShipRS;
                        qShipRS.playerId = (CollisionManager.PlayerID)packetReader.ReadInt32();
                        qShipRS.rotation = packetReader.ReadSingle();
                        qShipRS.impulse = packetReader.ReadSingle();
                        qH.data = qShipRS;
                        this.add(qH);
                        break;

                    case QueueType.SHIP_SR:
                        ShipData_SR qShipSR;
                        qShipSR.playerId = (CollisionManager.PlayerID)packetReader.ReadInt32();
                        qShipSR.x = packetReader.ReadSingle();
                        qShipSR.y = packetReader.ReadSingle();
                        qShipSR.rot = packetReader.ReadSingle();
                        qH.data = qShipSR;
                        this.add(qH);
                        break;

                    default:
                        break;
                }
            }
        }

        public void process(LocalNetworkGamer localGamer)
        {
            // Number of elements in queue
            int count = _q.Count;

            // Loop through and process them
            for (int i = 0; i < count; i++)
            {
                // Read the header
                NetworkData qH = _q.Dequeue();
                Player player;
                switch (qH.type)
                {
                    case QueueType.SHIP_RS:
                        if (localGamer.IsHost)
                        {
                            // Read the correct type of data
                            ShipData_RS qShipRS = (ShipData_RS)qH.data;
                            player = PlayerManager.Instance().getPlayer(qShipRS.playerId);
                            player.playerShip.Update(qShipRS);
                            Debug.WriteLine("Recv -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, Player {4}",
                                qH.inSeqNum, qH.outSeqNum, qH.type, qShipRS.GetType(), qShipRS.playerId);
                        }
                       
                        break;
                    case QueueType.SHIP_SR:
                        // Read the correct type of data
                        ShipData_SR qShipSR = (ShipData_SR)qH.data;
                        
                        player = PlayerManager.Instance().getPlayer(qShipSR.playerId);
                        player.playerShip.Update(qShipSR);

                        //Debug.WriteLine("PlayerID {0}",qShipSR.playerId.ToString());
                        Debug.WriteLine("Recv -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, Player {4}",
                            qH.inSeqNum, qH.outSeqNum, qH.type, qShipSR.GetType(), qShipSR.playerId);
                        break;
                    default:
                        break;
                        
                }
            }
        }
    }
}
