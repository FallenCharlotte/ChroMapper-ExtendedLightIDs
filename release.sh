#!/bin/bash

PROJECT="$(basename "$(pwd)")"
CHANGELOG="Changelog.txt"
ver=$1
short=$1

while [ "$(echo "$ver" | tr -dc '.' | awk '{ print length; }')" -lt "3" ]
do
	ver="${ver}.0"
done

sed -i 's/AssemblyVersion(.*)/AssemblyVersion("'$ver'")/' $PROJECT/Properties/AssemblyInfo.cs
sed -i 's/AssemblyFileVersion(.*)/AssemblyFileVersion("'$ver'")/' $PROJECT/Properties/AssemblyInfo.cs
sed -i 's/"version": ".*",/"version": "v'$short'",/' $PROJECT/manifest.json

mv "$CHANGELOG"{,.old}
echo "v${short}" > "$CHANGELOG"
git log $(git describe --tags --abbrev=0)..HEAD --pretty=format:'	%s' >> "$CHANGELOG"
echo -e "\n" >> "$CHANGELOG"
cat "$CHANGELOG".old >> "$CHANGELOG"
$EDITOR "$CHANGELOG"
_status=$?
if [[ $_status != 0 ]]; then
	echo "Aborting..."
	rm "$CHANGELOG"
	mv "$CHANGELOG"{.old,}
	exit
fi
rm "$CHANGELOG".old

git add $PROJECT/Properties/AssemblyInfo.cs
git add $PROJECT/manifest.json
git add Changelog.txt
git commit -m "v${short}"
git tag "v${short}"
msbuild
