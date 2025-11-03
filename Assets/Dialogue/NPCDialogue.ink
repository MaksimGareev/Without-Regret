INCLUDE Globals.ink

-> main

=== main ===
Hi my name is Irene
    * [This is the the good answer.]
        ~ Morality += 1
        -> goodResponse
    * [This is the bad answer.]
        ~ Morality -= 1
        -> badResponse
    * [This is the Nutral answer.]
        ~ Morality += 0
        -> neutralResponse
        
=== goodResponse ===
This means you picked the good response.
-> END

=== badResponse ===
This means you picked the bad response.
-> END

=== neutralResponse ===
This means you picked the nutral response.
-> END
 
