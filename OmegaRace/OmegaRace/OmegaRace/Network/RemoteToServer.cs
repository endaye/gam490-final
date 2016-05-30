using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollisionManager;
using Microsoft.Xna.Framework;

namespace OmegaRace
{
    enum ActionType
    {
        SHIP_ROTATION_RIGHT,    // D
        SHIP_ROTATION_LEFT,     // A
        SHIP_IMPULSE,           // W

    }

    struct RemoteToServerData
    {
        public ActionType type;
        public Single value;
    }

    class RemoteToServer
    {
        private RemoteToServerData data;

        public RemoteToServer()
        {

        }

        public RemoteToServer(RemoteToServerData data)
        {
            this.data = data;
        }

        public RemoteToServer(ActionType type, Single value = 0.0f)
        {
            this.data.type = type;
            this.data.value = value;
        }

        ~RemoteToServer()
        {
            // do nothing
        }

        public RemoteToServerData getData()
        {
            return data;
        }

        public void doWork(ref Player player)
        {
            switch (data.type)
            {
                case ActionType.SHIP_ROTATION_RIGHT:
                    player.playerShip.physicsObj.body.Rotation += data.value;
                    break;
                case ActionType.SHIP_ROTATION_LEFT:
                    player.playerShip.physicsObj.body.Rotation -= data.value;
                    break;
                case ActionType.SHIP_IMPULSE:
                    Ship Player1Ship = player.playerShip;
                    Vector2 direction = new Vector2((float)(Math.Cos(Player1Ship.physicsObj.body.GetAngle())), (float)(Math.Sin(Player1Ship.physicsObj.body.GetAngle())));
                    direction.Normalize();
                    direction *= data.value;
                    Player1Ship.physicsObj.body.ApplyLinearImpulse(direction, Player1Ship.physicsObj.body.GetWorldCenter());
                    break;
                default:
                    break;
            }
        }
    }


}
