#  Fetch first parameter
version=$(printenv BUILD_VERSION)

echo "Build Version:  $version"

#  Build sed command
sedcmd="s/BUILD_VALUE/$version/g"

echo "sed command:  $sedcmd"

#  Simple find / replace
sed -i -e $sedcmd PasWebApi/ApiVersion.cs