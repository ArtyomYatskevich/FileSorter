The input contains a large text file, where each line is of the form Number. String For example: 

    415. Apple 
    30432. Something something something 
    1. Apple 
    32. Cherry is the best 
    2. Banana is yellow 

Both parts can be repeated within the file. You need to get another file at the output, where all the lines  are sorted. Sort criterion: the first part of String is compared, if it matches, then Number. 
Those in the example above you should get 

    1. Apple 
    415. Apple 
    2. Banana is yellow 
    32. Cherry is the best 
    30432. Something something something 

You need to write two programs: 
    1. A utility for creating a test file of a given size. The result of the work should be a text file of the type  described above. There must be some number of strings with the same String part. 
    2. The sorter itself. An important point, the file can be very large. For testing, ~ 100Gb size will be used. 
    When evaluating the completed task, we will first of all look at the result (correct generation/sorting  and work time), secondly, at how the candidate writes the code. Programming language: C#.
