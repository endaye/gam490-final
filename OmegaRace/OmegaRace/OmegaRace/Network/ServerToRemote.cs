using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollisionManager;

namespace OmegaRace
{
    enum ObjectType
    {
        SHIP,
        MISSILE,
        BOMB,
        WALL,
        SCORE,
    }

    struct ServerToRemoteData
    {
        public ObjectType type;
        public Object data;

    }

    class ServerToRemote
    {
        private ServerToRemoteData data;

        public ServerToRemote()
        {

        }

        public ServerToRemote(ServerToRemoteData data)
        {
            this.data = data;
        }

        ~ServerToRemote()
        {
            // do nothing
        }

        public ServerToRemoteData getData()
        {
            return data;
        }

        public void doWork(ref Player player)
        {
            switch (data.type)
            {
                case ObjectType.SHIP:
                    player.playerShip.Update((ShipData)data.data);
                    break;
                default:
                    break;
            }
        }
    }
}
