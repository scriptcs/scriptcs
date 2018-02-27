#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net

  .paket/paket.bootstrapper.exe prerelease
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi

  .paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi

  packages/FAKE/tools/FAKE.exe $@ --nocache --fsiargs build.fsx 
else
  # use mono
  mono .paket/paket.bootstrapper.exe prerelease
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi

  mono .paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi
  mono packages/FAKE/tools/FAKE.exe $@ --nocache --fsiargs -d:MONO build.fsx
fi