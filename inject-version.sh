#	Fetch first parameter
version=$1

#	Simple find / replace
sed -i -e 's/BUILD_VALUE/$version/g' PasWebApi/ApiVersion.cs