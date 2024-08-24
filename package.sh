#!/bin/bash
ECO_VERSION="$1"
COPY_TESTS=false
SERVER_DIRECTORY=""
PACKAGE_NAME="package"
ROOT_DIRECTORY=$(pwd)

#Remove previous build
if [ -d "./${PACKAGE_NAME}" ]; then rm -r "./${PACKAGE_NAME}"; fi

# From Medium.com
# Function to display script usage
usage() {
 echo "Usage: $0 [OPTIONS]"
 echo "Options:"
 echo " -h, --help      Display this help message"
 echo " -t, --tests     Include tests in the output"
 echo " -d, --directory Directory to copy the output to"
}

has_argument() {
    [[ ("$1" == *=* && -n ${1#*=}) || ( ! -z "$2" && "$2" != -*)  ]];
}

extract_argument() {
  echo "${2:-${1#*=}}"
}

# Function to handle options and arguments
handle_options() {
  while [ $# -gt 0 ]; do
    case "$1" in
      -h | --help)
        usage
        exit 0
        ;;
      -t | --tests)
        COPY_TESTS=true
        ;;
      -d | --directory*)
        if ! has_argument "$@"; then
          echo "Directory not specified." >&2
          usage
          exit 1
        fi
        SERVER_DIRECTORY=$(extract_argument "$@")
        shift
        ;;
      *)
        echo "Invalid option: $1" >&2
        usage
        exit 1
        ;;
    esac
    shift
  done
}

#The first argument (the eco version) has already been read
shift
# Main script execution
handle_options "$@"


#Package up projects and copy them into a joint folder in the root directory
for MOD_PROJECT_DIRECTORY in "XP Benefits" "Ecompatible" "Replacement Interactions"
do
	cd "./${MOD_PROJECT_DIRECTORY}"
	./package.sh $VERSION $ECO_VERSION $PACKAGE_NAME
	cp -RT "./${PACKAGE_NAME}" "${ROOT_DIRECTORY}/${PACKAGE_NAME}"
	cd "$ROOT_DIRECTORY"
done
echo "Combined mods into ${ROOT_DIRECTORY}/${PACKAGE_NAME}"

if [ "$COPY_TESTS" = true ]
then
	cp -RTu "./XP Benefits/tests" "./${PACKAGE_NAME}"
	cp -RTu "./Ecompatible/tests" "./${PACKAGE_NAME}"
	cp -RTu "./Replacement Interactions/tests" "./${PACKAGE_NAME}"
	cp -RTu "./Eco Test Tools/tests" "./${PACKAGE_NAME}"
fi

#Copy the mods to the local Eco server 
if [[ -d "${SERVER_DIRECTORY}" ]]
then
	cp -RTu "./${PACKAGE_NAME}" "${SERVER_DIRECTORY}"
	echo "Copied mods to server"
fi
