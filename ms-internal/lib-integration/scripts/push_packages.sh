#!/bin/bash
## Upload CarbonAware nuget packages to https://int.nugettest.org/
set -x

DEST_PACKAGES=/workspaces/ca_nuget_packages
API_KEY=$1
if [[ -z $API_KEY ]]
then
    printf "Missing paramater. Usage: $0 API_KEY\n"
    exit 1
fi
for file in `find $DEST_PACKAGES -name "CarbonAware*.nupkg"` 
do
    dotnet nuget push $file --api-key $API_KEY --source https://apiint.nugettest.org/v3/index.json
done
