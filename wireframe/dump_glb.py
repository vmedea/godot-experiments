#!/usr/bin/python3
import sys

import pygltflib
import numpy

x=pygltflib.GLTF2().load(sys.argv[1])

for acc in x.accessors:
    print(acc)

for acc_id in [2,3,4]: # UV0, UV1, UV2
    print(acc_id)
    acc = x.accessors[acc_id]
    bv = x.bufferViews[acc.bufferView]
    data = x.get_data_from_buffer_uri(x.buffers[bv.buffer].uri)
    buf = numpy.frombuffer(data[bv.byteOffset:bv.byteOffset + bv.byteLength], dtype=numpy.float32)
    print(buf)
    #buf = numpy.frombuffer(data[bv.byteOffset:bv.byteOffset + bv.byteLength], dtype=numpy.int16)
    #print(buf)
