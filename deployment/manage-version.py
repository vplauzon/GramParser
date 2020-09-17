#!/usr/bin/python3

import sys

def readAll(path):
    with open(path, 'r') as f:
        content = f.read()

        return content

def writeAll(path, content):
    with open(path, 'w') as f:
        f.write(content)

def getPartialVersion(content):
    import re
    
    m = re.search('\d+\.\d+', content)
    
    return m.group(0)

#   Does the following:
#   A.  Fetch partial version from ApiVersion.txt
#   B.  Create full version by adding build number to it
#   C.  Inject full version in code
#   D.  Write full version down a specified path
#   E.  Output full version as a Az Dev Ops variable
if len(sys.argv) != 5:
    print("There are %d arguments" % (len(sys.argv)-1))
    print("Arguments should be")
    print("1-ApiVersion.txt file path")
    print("2-ApiVersion.cs file path")
    print("3-Build number")
    print("4-Full Version path")
else:
    partialVersionPath = sys.argv[1]
    codeVersionPath = sys.argv[2]
    buildNumber = sys.argv[3]
    fullVersionPath = sys.argv[4]

    print ('Text file path:  %s' % (partialVersionPath))
    print ('Code Version path:  %s' % codeVersionPath)
    print ('Build Number:  %s' % (buildNumber))
    print ('Full Version path:  %s' % (fullVersionPath))

    txtContent = readAll(partialVersionPath)
    
    print('Text content:  "%s"' % txtContent)

    #   Extract partial version from file
    #   Even if the file content has funny characters, it will pick it up
    partialVersion = getPartialVersion(txtContent)

    print('Partial Version:  "%s"' % partialVersion)

    #   Create full version
    fullVersion = partialVersion + "." + buildNumber

    print('Full Version:  "%s"' % (fullVersion))

    #   Inject full version in code
    code = readAll(codeVersionPath)

    print("Code:")
    print(code)

    alternateCode = code.replace('FULL_VERSION', fullVersion)

    print("Alternate code:")
    print(alternateCode)

    writeAll(codeVersionPath, alternateCode)

    #   Write full version to file
    writeAll(fullVersionPath, fullVersion)

    #   Output variable
    print('Set the full version in Azure DevOps variable:')

    print('##vso[task.setvariable variable=full-version;]%s' % (fullVersion))
