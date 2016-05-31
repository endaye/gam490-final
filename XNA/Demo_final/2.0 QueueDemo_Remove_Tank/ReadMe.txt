QueueDemo_02_remove_Tank

Goal: Get rid of most of the tank stuff

0) copy QueueDemo_01 to QueueDemo_02

1) delete the tgas from project
* Tank.tga
* Turret.tga

2) Added GreenBird tga to project
* cut and past GreenBird.tga to content

3) Tank.cs

*  Remove TankTurnRate
*  Remove TankTurnRate
*  Remove TankRotation
*  Remove TurretRotation
*  Remove TurretInput 
*  Remove TurretTexture

*  Replace TankTexture with GreenBird.tga


*  Take a Chainsaw to the Update() function
     *  Remove the targetFoward
     *  Remove the TurnToFace (TankRotation...)
     *  Remove the TurnToFace (TurretRotation...)
     *  Remove the facingForward dot

     *  Keep last 3 lines (original)

     *  Refactor to 
            * tankForward = new Vector2(TankInput.X,-TankInput.Y);
            * Velocity += tankForward * TankSpeed;

*  Rework the Draw function
     *  Remove the rotation of the tank
     *  Remove the draw of the turret altogether

4) PeerToPeerGame.cs

*  UpdateLocalGamer(LocalNetworkGamer gamer)
     *  Remove packetWriter.Write(localTank.TankRotation);
     *  Remove packetWriter.Write(localTank.TurretRotation);
     *  leave the Tank Poistion

*  ReadIncomingPackets(LocalNetworkGamer gamer)
     *  Remove remoteTank.TankRotation = packetReader.ReadSingle();
     *  Remove remoteTank.TurretRotation = packetReader.ReadSingle();

* search and destroy turretInput

5) RENAME

* Tank.cs to Bird.cs
     * Use the tool to auto adjust every reference


