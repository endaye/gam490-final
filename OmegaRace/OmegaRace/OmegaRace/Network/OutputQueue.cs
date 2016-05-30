using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace OmegaRace
{
    struct NetworkData
    {
        public int seqNum;
        public Type type;
        public Object data;
    }

    class OutputQueue
    {
        private static OutputQueue instance;

        private static System.Collections.Generic.Queue<NetworkData> _q = new System.Collections.Generic.Queue<NetworkData>();
        
        private static int seqNum = 0;

        private static int getSeqNum()
        {
            return seqNum++;
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

        public void add(Object data)
        {
            NetworkData outputData;
            outputData.seqNum = getSeqNum();
            outputData.type = data.GetType();
            outputData.data = data;
            _q.Enqueue(outputData);
        }

        public void process()
        {
            while (_q.Count() != 0) {
                NetworkData tmpData = _q.Dequeue();
                InputQueue.Instance.receive(tmpData);
                Debug.WriteLine("Send -> SeqNum {0}, Type: {1}", tmpData.seqNum, tmpData.type);
            }  
        }
    }
}
