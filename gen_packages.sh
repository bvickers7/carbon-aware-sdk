#!/bin/bash
set -x

PREFIX="0.0.16"
DEST_PACKAGES=/workspaces/ca_nuget_packages

dotnet remove package CarbonAware
dotnet remove package CarbonAware.LocationSources
dotnet remove package CarbonAware.Aggregators
dotnet remove package CarbonAware.Tools.WattTimeClient
rm -rf ~/.nuget/packages/carbonaware*
rm -rf bin obj

# Remove existing packages with the same PREFIX
find $DEST_PACKAGES -name "*.nupkg" -exec rm {} \;
# cd src
dotnet pack ../src/CarbonAwareSDK.sln -o $DEST_PACKAGES \
    -p:VersionPrefix=$PREFIX \
    -p:VersionSuffix=beta \
    -p:SourceRevisionId=c20572dccb64b3bd7e585ddbef8a4c68255d0dd8 \
    -p:RepositoryUrl=https://github.com/microsoft/carbon-aware-sdk \
    -p:RepositoryType=git \
    -p:RepositoryBranch=dev \
    -p:Description="Green Software Foundation SDK" \
    -p:PackageLicenseExpression=MIT

# Local Feed
# Create new dotnet project/console/lib, then add packages to the project as:
dotnet add package CarbonAware -s $DEST_PACKAGES --prerelease
dotnet add package CarbonAware.LocationSources -s $DEST_PACKAGES --prerelease
dotnet add package CarbonAware.Aggregators -s  $DEST_PACKAGES --prerelease
dotnet add package CarbonAware.Tools.WattTimeClient -s $DEST_PACKAGES --prerelease

dotnet build
find . -name "*.json"
# ....

# Clean nuget packages (see more dotnet nuget locals -l all)
# dotnet nuget locals -c global-packages

# ISSUE: How to find the location files from the nuget package - DONE
# ISSUE: How to pull dependency packages (Microsoft Packages) from CarbonAware.*
# ISSUE: To have a very good dev starting guide on how to start
# using the service extension to import the aggregator, how to pass 
# configuration information (using envs, or using own configuration map)
# and how to construct the CarbonAwareParams in case we don't provide
# a better mechanism to deal with.
# reference: https://blog.tonysneed.com/2021/12/04/copy-nuget-content-files-to-output-directory-on-build/


