# Virtual Control System

This repo is the start of creating a VC4 program that can be monitored by a CI/CD Tool.

When the CI/CD Tool sees an update on the program, it will utilize VC4's API to load the new version of the program to all the rooms running the same program.

VC4 is a linux based control system that runs on a RHEL Server.  It eliminates the need to purchase a blackbox per room, by allowing you to "virtualize" your rooms on one server.
