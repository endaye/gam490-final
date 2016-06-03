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

        private static System.Collections.Generic.Queue<Message> _q = new System.Collections.Generic.Queue<Message>();

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

        public void add(Message _data)
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

                int inSeqNum = packetReader.ReadInt32();
                int outSeqNum = packetReader.ReadInt32();
                QueueType type = (QueueType)packetReader.ReadInt32();

                switch (type)
                {
                    case QueueType.SHIP_RS:
                        Ship_RS qShipRS = new Ship_RS();
                        qShipRS.inSeqNum = inSeqNum;
                        qShipRS.outSeqNum = outSeqNum;
                        qShipRS.type = type;
                        qShipRS.playerId = (CollisionManager.PlayerID)packetReader.ReadInt32();
                        qShipRS.rotation = packetReader.ReadSingle();
                        qShipRS.impulse = packetReader.ReadSingle();
                        qShipRS.missle = packetReader.ReadBoolean();
                        qShipRS.bomb = packetReader.ReadBoolean();
                        this.add(qShipRS);
                        break;

                    case QueueType.PHYSICS_SR:
                        int count = packetReader.ReadInt32();
                        PhysicsBuffer[] pBuffer = new PhysicsBuffer[count];
                        Physics_SR qPhysSR = new Physics_SR(ref pBuffer);
                        qPhysSR.inSeqNum = inSeqNum;
                        qPhysSR.outSeqNum = outSeqNum;
                        qPhysSR.type = type;
                        for (int i = 0; i < count; i++)
                        {
                            qPhysSR.pBuffer[i] = new PhysicsBuffer();
                            qPhysSR.pBuffer[i].id = packetReader.ReadInt32();
                            qPhysSR.pBuffer[i].rot = packetReader.ReadSingle();
                            Vector2 v = new Vector2();
                            v.X = 1.0f;
                            v.Y = 2.0f;
                            qPhysSR.pBuffer[i].pos.X = packetReader.ReadSingle();
                            qPhysSR.pBuffer[i].pos.Y = packetReader.ReadSingle();
                            //qPhysSR.pBuffer[i].pos = v;
                        }
                        this.add(qPhysSR);
                        break;

                    //case QueueType.GAMEOBJ_SR:
                    //    GameObjMsg_SR qGameSR;
                    //    qGameSR.gameObjId = packetReader.ReadInt32();
                    //    qGameSR.state = (CollisionManager.GameObjSRState)packetReader.ReadInt32();
                    //    qH.data = qGameSR;
                    //    this.add(qH);
                    //    break;
                         
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
                Message qH = _q.Dequeue();
                Debug.WriteLine("Recv -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}", qH.inSeqNum, qH.outSeqNum, qH.type);
                qH.execute();
                //switch (qH.type)
                //{
                //    case QueueType.SHIP_RS:
                //        if (localGamer.IsHost)
                //        {
                //            // Read the correct type of data
                //            ShipMsg_RS qShipRS = (ShipMsg_RS)qH.data;
                //            player = PlayerManager.Instance().getPlayer(qShipRS.playerId);
                //            player.playerShip.Update(qShipRS);
                //            Debug.WriteLine("Recv -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, Player {4}, rot {5}, imp {6}",
                //                qH.inSeqNum, qH.outSeqNum, qH.type, qShipRS.GetType(), qShipRS.playerId, qShipRS.rotation, qShipRS.impulse);
                //        }
                //        break;

                //    case QueueType.Physics_SR:
                //        // Read the correct type of data
                //        ShipMsg_SR qShipSR = (ShipMsg_SR)qH.data;
                //        player = PlayerManager.Instance().getPlayer(qShipSR.playerId);
                //        player.playerShip.Update(qShipSR);

                //        //Debug.WriteLine("PlayerID {0}",qShipSR.playerId.ToString());
                //        Debug.WriteLine("Recv -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, Player {4}, rot {5}, pos [{6}, {7}]",
                //            qH.inSeqNum, qH.outSeqNum, qH.type, qShipSR.GetType(), qShipSR.playerId, qShipSR.rot, qShipSR.x, qShipSR.y);
                //        break;

                //    case QueueType.GAMEOBJ_SR:
                //        GameObjMsg_SR qGameSR = (GameObjMsg_SR)qH.data;
                //        GameObjManager.Instance().RevieveFromeInQ(qGameSR);
                //        Debug.WriteLine("Recv -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}->{3}, GameObjID {4} #{5}",
                //            qH.inSeqNum, qH.outSeqNum, qH.type, qGameSR.GetType(), qGameSR.gameObjId, qGameSR.state);
                //        break;
                //    default:
                //        break;
                        
                //}
            }
        }
    }
}
