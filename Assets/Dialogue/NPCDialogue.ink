INCLUDE Globals.ink

-> main

=== main ===
Hi my name is Sally
    * [This is the the good answer.]
        ~ Morality += 1
        -> goodResponse
    * [This is the bad answer.]
        ~ Morality -= 1
        -> badResponse
    * [This is the Neutral answer.]
        ~ Morality += 0
        -> neutralResponse
        
=== goodResponse ===
This means you picked the good response.
-> END

=== badResponse ===
This means you picked the bad response.
-> END

=== neutralResponse ===
This means you picked the neutral response.
-> END
 
