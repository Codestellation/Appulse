#!/bin/bash
set -e

function cut {
  # Workaround for https://github.com/appveyor/ci/issues/1956
  /bin/cut $@
}

echo "====================================="
echo "           Versioning                "
echo "====================================="
DESCRIBE=`git describe --abbrev=7 --first-parent --long --dirty --always`
TAG=`echo $DESCRIBE | cut -d '-' -f 1 | cut -c 2-`
PATCH=`echo $DESCRIBE | cut -d '-' -f 2`
HASH=`echo $DESCRIBE | cut -d '-' -f 3 | cut -c 2-`

VERSION="$TAG.$PATCH"
FULLVERSION="$VERSION-$HASH"
echo "Version: $FULLVERSION"

echo "====================================="
echo "              Build                  "
echo "====================================="
dotnet build ./src/ --configuration Release \
                    /property:Version=$VERSION \
                    /property:InformationalVersion=$FULLVERSION

echo "====================================="
echo "           Run tests                 "
echo "====================================="
dotnet test ./src/*.Tests/ --configuration Release \
                           --no-build

echo "====================================="
echo "        Create nuget packet          "
echo "====================================="
dotnet pack ./src/ --configuration Release \
                   --no-build \
                   /property:PackageVersion=$VERSION