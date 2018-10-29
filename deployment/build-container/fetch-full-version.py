#!/usr/bin/python3

import sys

def readAll(path):
    with open(path, 'r') as f:
        content = f.read()

        return content

if len(sys.argv) != 3:
    print("There are %d arguments" % (len(sys.argv)-1))
    print("Arguments should be")
    print("1-ApiVersion.txt file path")
    print("2-Build number")
else:
    versionPath = sys.argv[1]
    buildNumber = sys.argv[2]

    print ('Text file:  %s' % (versionPath))
    print ('Build Number:  %s' % (buildNumber))

    txtContent = readAll(versionPath).strip()
    
    print('Text content:')
    print(txtContent)

    fullVersion = txtContent + "." + buildNumber

    print('Full Version:  %s' % (fullVersion))

    print('Set the full version in Azure DevOps variable:')

    print('##vso[task.setvariable variable=full-version;]%s' % (fullVersion))
