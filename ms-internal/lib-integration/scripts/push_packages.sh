#!/bin/bash
## Upload CarbonAware nuget packages to https://int.nugettest.org/
set -x

PACKAGE_SRC=$1
API_KEY=$2
if [[ -z $PACKAGE_SRC ] || [ -z $API_KEY ]]
then
    printf "Missing parameter. Usage: $0 PACKAGE_DIR API_KEY\n"
    printf "Example: $0 /mypackages o32efgadulwil5bdpcoa24ejwvnmdg4uld6cwo7g1xdqke"
    exit 1
fi

for file in `find $PACKAGE_DIR -name "CarbonAware*.nupkg"` 
do
    dotnet nuget push $file --api-key $API_KEY --source https://apiint.nugettest.org/v3/index.json
done
