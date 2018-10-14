#!/usr/bin/python3

import sys

def readAll(path):
    with open(path, 'r') as f:
        content = f.read()

        return content

def writeAll(path, content):
    with open(path, 'w') as f:
        f.write(content)

if len(sys.argv) != 4:
    print("There are %d arguments" % (len(sys.argv)-1))
    print("Arguments should be")
    print("1-ApiVersion.cs file path")
    print("2-ApiVersion.txt file path")
    print("3-Build number")
else:
    csPath = sys.argv[1]
    versionPath = sys.argv[2]
    buildNumber = sys.argv[3]

    print ('C# file:  %s' % (csPath))
    print ('Text file:  %s' % (versionPath))
    print ('Build Number:  %s' % (buildNumber))

    csContent = readAll(csPath)
    txtContent = readAll(versionPath).strip()
    
    # Filter out weird unicode characteres:
    csContent = "".join([x for x in csContent if ord(x)<256])
    txtContent = "".join([x for x in txtContent if ord(x)<256])

    print('C# content:')
    print(csContent)
    print('Text content:')
    print(txtContent)
    print('First Character:')
    print(ord(txtContent[0])

    fullVersion = txtContent + "." + buildNumber

    print('Full Version:  %s' % (fullVersion))

    csContent = csContent.replace('FULL_VERSION', fullVersion)

    print('Altered C# content:')
    print(csContent)

    writeAll(csPath, csContent)