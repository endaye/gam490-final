using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2D.XNA;
using CollisionManager;

namespace OmegaRace
{
    class PhysicsMan : Manager
    {
        private static PhysicsMan instance;
       
        private PhysicsMan()
        {

        }

        public static PhysicsMan Instance()
        {
            if (instance == null)
                instance = new PhysicsMan();
            return instance;
        }

        public void addPhysicsObj(GameObject _gameObj, Body _body)
        {
            PhysicsObj obj = new PhysicsObj(_gameObj, _body);
            _gameObj.physicsObj = obj;

            this.privActiveAddToFront((ManLink)obj, ref this.active);
        }


        public void Update()
        {
            ManLink ptr = this.active;
            PhysicsObj physNode = null;
            Body body = null;

            this.pullFromBuffer();

            while (ptr != null)
            {
                physNode = (PhysicsObj)ptr;
                body = physNode.body;

                physNode.gameObj.pushPhysics(body.GetAngle(), body.Position);

                ptr = ptr.next;
            }
        }

        public void pullFromBuffer()
        {
            if (Physics_SR.pBufferGlobal != null)
            {
                int count = Physics_SR.pBufferGlobal.count;
                PhysicsBuffer[] physBuff = Physics_SR.pBufferGlobal.pBuffer;
                for (int i = 0; i < count; i++)
                {
                    GameObjNode g = GameObjManager.Instance().findGameObj(physBuff[i].id);
                    if (g != null)
                    {
                        g.gameObj.physicsObj.setPhysicsBufferNode(physBuff[i]);
                    }
                }
            }
        }

        public void pushToBuffer()
        {
            ManLink ptr = this.active;
            PhysicsObj physNode = null;
            List<PhysicsBuffer> physBuffList = new List<PhysicsBuffer>();
            PhysicsBuffer[] physBuff;
            while (ptr != null)
            {
                physNode = (PhysicsObj)ptr;
                physBuffList.Add(physNode.getPhysicsBufferNode());
                ptr = ptr.next;
            }
            physBuff = physBuffList.ToArray();
            Physics_SR.pBufferGlobal = new Physics_SR(ref physBuff);
            OutputQueue.Instance.add(Physics_SR.pBufferGlobal);
        }

        public void removePhysicsObj(PhysicsObj _obj)
        {
            this.privActiveRemoveNode((ManLink)_obj, ref this.active);
        }

        protected override object privGetNewObj()
        {
            throw new NotImplementedException();
        }

    }
}
