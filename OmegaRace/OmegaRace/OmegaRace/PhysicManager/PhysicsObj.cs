using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2D.XNA;
using CollisionManager;

namespace OmegaRace
{
    class PhysicsObj : ManLink
    {
        public Body body;
        public GameObject gameObj;

        public PhysicsObj()
        {

        }

        public PhysicsObj(GameObject _obj, Body _body)
        {
            gameObj = _obj;
            body = _body;
        }

        public override Enum getName()
        {
            throw new NotImplementedException();
        }

        public PhysicsBuffer getPhysicsBufferNode()
        {
            PhysicsBuffer physBuffNode = new PhysicsBuffer();
            physBuffNode.id = this.gameObj.id;
            physBuffNode.rot = this.body.Rotation;
            physBuffNode.pos = this.body.Position;
            return physBuffNode;
        }

        public void setPhysicsBufferNode(PhysicsBuffer physBuffNode)
        {
            this.body.Rotation = physBuffNode.rot;
            this.body.Position = physBuffNode.pos;
        }
    }
}
