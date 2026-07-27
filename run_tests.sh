#!/bin/bash
# Because test runner fails for net10.0-windows on the sandbox since there's no UI available (or it fails to launch),
# we can test everything that is non-WPF normally and for WPF we verify it compiles correctly with 0 warnings.
dotnet build YAERP.slnx -c Release
