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

        private void pullFromNetwork(LocalNetworkGamer localGamer)
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
                        CollisionManager.PlayerID id = (CollisionManager.PlayerID)packetReader.ReadInt32();
                        float rot = packetReader.ReadSingle();
                        float imp = packetReader.ReadSingle();
                        int missle = packetReader.ReadInt32();
                        int bomb = packetReader.ReadInt32();
                        Ship_RS qShipRS = new Ship_RS(id, rot, imp, missle, bomb);
                        qShipRS.inSeqNum = inSeqNum;
                        qShipRS.outSeqNum = outSeqNum;
                        qShipRS.type = type;
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
                            qPhysSR.pBuffer[i].pos.X = packetReader.ReadSingle();
                            qPhysSR.pBuffer[i].pos.Y = packetReader.ReadSingle();
                        }
                        this.add(qPhysSR);
                        break;

                    case QueueType.COL_EVENT_SR:
                        int GameObjA_ID = packetReader.ReadInt32();
                        int GameObjB_ID = packetReader.ReadInt32();
                        float x = packetReader.ReadSingle();
                        float y = packetReader.ReadSingle();
                        Vector2 ColPos = new Vector2(x, y);
                        Col_Event_SR qColEvent_SR = new Col_Event_SR(GameObjA_ID, GameObjB_ID, ColPos);
                        this.add(qColEvent_SR);
                        break;
                         
                    default:
                        break;
                }
            }
        }

        public void process(LocalNetworkGamer localGamer)
        {
            this.pullFromNetwork(localGamer);

            // Number of elements in queue
            int count = _q.Count;

            // Loop through and process them
            for (int i = 0; i < count; i++)
            {
                // Read the header
                Message qH = _q.Dequeue();
                Debug.WriteLine("Recv -> InSeqNum {0,6}, OutSeqNum {1,6}, {2}", qH.inSeqNum, qH.outSeqNum, qH.type);
                qH.execute();
            }
        }
    }
}
