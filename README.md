GraphBipartition
================
Take a list of tree partitions and reconstruct the tree.

The algorithm works by finding the largest partition and then successively using smaller subsets to partition each subset further. 

For Example, given this list of partitions : "b/acde", "ba/cde", "bace/d", "bacd/e"

The algorithm starts with the initial partition : [ba]-[cde]    
Then using the partition b/acde we can further partition [ba] to [b]-[a].  
Similarly, we can apply the other two partitions to the subset [cde] to get:
```
[c]-[e].     
 |     
[d]   
```                                                                             
Which gives us the final partition set (where each subset is of size 1):
```
[b]-[a]-[c]-[e].    
         |   
        [d]
```
