#!/usr/bin/python3

import sys
import os

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
    print("1-File path")
    print("2-Patch number")
else:
    path = sys.argv[1]
    patchNumber = sys.argv[2]
    alphaPath = os.path.dirname(path)+"/alpha-nuget.csproj"
    goldPath = os.path.dirname(path)+"/gold-nuget.csproj"

    print ('Text file:  %s' % (path))
    print ('Patch Number:  %s' % (patchNumber))
    print ('Alpha path:  %s' % (alphaPath))
    print ('Gold path:  %s' % (goldPath))

    txtContent = readAll(path)
    
    print('Text content:')
    print(txtContent)

    alphaContent = txtContent.replace('.0</Version>', '.'+patchNumber+'-alpha</Version>')
    goldContent = txtContent.replace('.0</Version>', '.'+patchNumber+'</Version>')

    print('Alpha content:')
    print(alphaContent)
    print('Gold content:')
    print(goldContent)

    writeAll(alphaPath, alphaContent)
    writeAll(goldPath, goldContent)