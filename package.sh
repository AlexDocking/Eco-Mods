#!/bin/bash
ECO_VERSION=$1
SERVER_DIRECTORY=$2
PACKAGE_NAME="package"
ROOT_DIRECTORY=$(pwd)

#Remove previous build
if [ -d "./${PACKAGE_NAME}" ]; then rm -r "./${PACKAGE_NAME}"; fi

#Package up projects and copy them into a joint folder in the root directory
for MOD_PROJECT_DIRECTORY in "XP Benefits" "Compatible Tools" "Replacement Interactions"
do
	cd "./${MOD_PROJECT_DIRECTORY}"
	./package.sh $VERSION $ECO_VERSION $PACKAGE_NAME
	cp -RT "./${PACKAGE_NAME}" "${ROOT_DIRECTORY}/${PACKAGE_NAME}"
	cd "$ROOT_DIRECTORY"
done
echo "Combined mods into ${ROOT_DIRECTORY}/${PACKAGE_NAME}"

#Copy the mods to the local Eco server 
if [ -d "${SERVER_DIRECTORY}" ]
then
	cp -RTu "./${PACKAGE_NAME}" "${SERVER_DIRECTORY}"
	echo "Copied mods to server"
fi