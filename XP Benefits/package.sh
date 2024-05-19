#!/bin/bash
VERSION="v1.2.2"
ECO_VERSION=$1
PACKAGE_NAME=$2
if [ -d "./${PACKAGE_NAME}" ]; then rm -r "./${PACKAGE_NAME}"; fi
cp -r "./src" "./${PACKAGE_NAME}"
sed -i "1s/.*/# XP Benefits ${VERSION} for Eco ${ECO_VERSION}/" "./${PACKAGE_NAME}/Mods/UserCode/XP Benefits/README.md"
cp "./LICENSE" "./${PACKAGE_NAME}/Mods/UserCode/XP Benefits/LICENSE"