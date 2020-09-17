#!/usr/bin/python3

import sys
import os
import xml.dom.minidom as md 

def writeXml(document, path):
    with open(path, "w" ) as fs:  
        fs.write(document.toxml() ) 

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

    #   Load XML and print out
    document = md.parse(path)
    print('XML content:')
    print(document.toxml())

    #   Set GeneratePackageOnBuild to true
    generate=document.getElementsByTagName('GeneratePackageOnBuild')[0]
    generate.firstChild.nodeValue = "true"

    #   Add patch to the version
    version=document.getElementsByTagName('Version')[0]
    version.firstChild.nodeValue += "." + patchNumber

    #   Output gold
    print('Gold content:')
    print(document.toxml())
    writeXml(document, goldPath)

    #   Add '-alpha' to the version
    version.firstChild.nodeValue += "-alpha"

    #   Output alpha
    print('Alpha content:')
    print(document.toxml())
    writeXml(document, alphaPath)
