#!/bin/bash
VERSION="v1.1.0"
MOD_NAME="Replacement Interactions"
ECO_VERSION="v0.11.1.3+"
PACKAGE_NAME=$1
if [ -d "./${PACKAGE_NAME}" ]; then rm -r "./${PACKAGE_NAME}"; fi
cp -r "./src" "./${PACKAGE_NAME}"
cp "./README.md" "./${PACKAGE_NAME}/Mods/UserCode/${MOD_NAME}/README.md"
cp "./LICENSE" "./${PACKAGE_NAME}/Mods/UserCode/${MOD_NAME}/LICENSE"
cp "./NOTICE" "./${PACKAGE_NAME}/Mods/UserCode/${MOD_NAME}/NOTICE"
sed -i "1s/.*/# ${MOD_NAME} ${VERSION} for Eco ${ECO_VERSION}/" "./${PACKAGE_NAME}/Mods/UserCode/${MOD_NAME}/README.md"