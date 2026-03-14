#!/bin/sh
# Runs Stryker.NET mutation testing in incremental mode.
# Sets STRYKER_MUTATING to disable the OpenAPI source generator's interceptors
# which are incompatible with Stryker's Roslyn compilation.
# Track: https://github.com/stryker-mutator/stryker-net/issues/3402

export STRYKER_MUTATING=true
dotnet stryker --since:main --reporter cleartext
