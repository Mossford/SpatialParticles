
## Ideas for storing data for behaviors of particles

* ### Void pointer

        Each particle can have its own data allocated to itself
        Easy access as stored in self
        
        Bad for cache as locallity is poor
        Good for memory space as per particle allocation


* ### Array pools - List<T[]>

        Every particle gets a variable for a custom behavior
        Can use already stored id for the particle as same positioning
        Requires moving all state and variables to array
        
        Good for cache as locallity is maximum possible
        Bad for memory space as each particle gets a variable that it might not need
        Deletion and Creation is not performed and variables are only assigned