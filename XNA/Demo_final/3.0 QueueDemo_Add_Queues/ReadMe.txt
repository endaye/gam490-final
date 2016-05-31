
Goal: add queues

*  Add the inputQueue
    *  Add file
    *  Add inQueue in Game
    *  Add Queue_type
    *  Add QueueHdr
    *  Add InputQueue->process() method
          * a way to update the Bird
          * add gamerIndex to distiguish between Bird
          * Bird[2] holds only the references
	      * Update in creation (where new Bird is located)
    *  Add Static Class/method Bird_inQueue
          *  a way to insert to the queue

*  Add the outputQueue
    *  Add file
    *  Add inQueue in Game
    *  Add Queue_type
    *  Add QueueHdr
    *  Add OutputQueue->PushToNetwork()
          *  Push the data from the output queue to the network
          *  Initially to the inputQueue
    *  Add Static Class/method Bird_outQueue
          *  a way to insert to the queue



*  Remove ReadIncomingPackets()
*  Add to ReadBirdInputs()
    * outQueue data  <----------- most important
    * Killing BirdInput

*  GreenBird.cs
    * add Bird_type
    * add Bird_data
    * Add gamerIndex (needed to figure out which Bird is in queue) 
                     (used in Array of Birds reference)
    * Killing BirdInput (using the outputQueue instead)
    * Rework update() to be data driven update( Bird_Data )


* PeerToPeerGame.cs

    * Refactor  GamerJoinedEventHandler()
         * Add Bird[] reference (gamerIndex)
         * InputQueue.pBird[gamerIndex] = e.Gamer.Tag as Bird; 

    *  Rework UpdateNetworkSession
        * UpdateLocalGamer()
             * Remove any localBird.update()
             * Remove any sending of data
        * outQueue.PushToNetwork()
        * inQueue.process()
        * house keeping


