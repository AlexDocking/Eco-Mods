#!/bin/bash
VERSION="v1.0.0"
ECO_VERSION=$1
PACKAGE_NAME=$2
if [ -d "./${PACKAGE_NAME}" ]; then rm -r "./${PACKAGE_NAME}"; fi
cp -r "./src" "./${PACKAGE_NAME}"