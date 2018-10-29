#!/usr/bin/python3

import sys

def readAll(path):
    with open(path, 'r') as f:
        content = f.read()

        return content

def writeAll(path, content):
    with open(path, 'w') as f:
        f.write(content)

if len(sys.argv) != 3:
    print("There are %d arguments" % (len(sys.argv)-1))
    print("Arguments should be")
    print("1-ApiVersion.cs file path")
    print("2-Full version")
else:
    csPath = sys.argv[1]
    fullVersion = sys.argv[2]

    print ('C# file:  %s' % (csPath))
    print('Full Version:  %s' % (fullVersion))

    csContent = readAll(csPath)
    
    print('C# content:')
    print(csContent)

    csContent = csContent.replace('FULL_VERSION', fullVersion)

    print('Altered C# content:')
    print(csContent)

    writeAll(csPath, csContent)