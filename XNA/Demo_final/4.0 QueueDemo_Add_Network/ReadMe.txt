
Goal: Add Networking

*  OutputQueue
    * PushToNetwork
        * Add localGamer to argument
        * add packetwriter to outputQueue
        * pack data, write data to everyone
             * since its going to itself, no need to internally pass it
             * comment out the inQueue short cut

* Add packetReader to game
    * updateNetworkSession()
        * Add reader from the network
        * Check localGamer, read data
            * data is passed into the inQueue <------------ important

