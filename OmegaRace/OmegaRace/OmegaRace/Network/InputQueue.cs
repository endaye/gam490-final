using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CollisionManager;

namespace OmegaRace
{
    class InputQueue
    {
        private static InputQueue instance;

        private static System.Collections.Generic.Queue<NetworkData> _q = new System.Collections.Generic.Queue<NetworkData>();

        private static int seqNum = 0;

        private static int getSeqNum()
        {
            return seqNum++;
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

        public void receive(Object data)
        {
            NetworkData inputData = (NetworkData) data;
            _q.Enqueue(inputData);
        }

        public void process(ref Player player)
        {
            NetworkData inputData = new NetworkData();
            int count = _q.Count;
           
            // receive data from 
            for (int i = 0; i < count; i++)
            {
                inputData = _q.Dequeue();
                Debug.WriteLine("Recv <- SeqNum {0}, Type: {1}", inputData.seqNum, inputData.type);
                if (inputData.type.Equals(typeof(RemoteToServer)))
                {
                    RemoteToServer rs = (RemoteToServer) inputData.data;
                    rs.doWork(ref player);
                }
            }
        }
    }
}
