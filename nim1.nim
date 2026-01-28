import math
import sequtils
import algorithm  

proc BaseX(num: int, base: int): seq[int] =
    var output: seq[int] = @[]
    output.add(num)
    while output[^1] > base:
        output.add(floor(output[^1]/base).int)
        output[^2] = output[^2] - output[^1] * base
    return output.reversed()

proc Sort(size: int, base: int): seq[int] = 
    var num: int = 0
    for i in countup(1, size):
        num += pow(base, i)
    
    while true:
        BaseX(num, base)