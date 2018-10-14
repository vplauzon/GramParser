#  Fetch first parameter
build=$(printenv BUILD_VERSION)

echo "Build:$build"

#  Fetch API version
apiVersion=$(cat test-version/ApiVersion.txt)

echo "API Version:$apiVersion"

#  Compose full version
fullVersion="$apiVersion.$build"

echo "Full Version:$fullVersion"

#  Build sed command
sedcmd="s/BUILD_VALUE/$fullVersion/g"

echo "sed command:$sedcmd"

#  Simple find / replace
sed -i -e $sedcmd test-version/ApiVersion.cs