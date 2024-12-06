#!/bin/bash

security find-identity -p codesigning

codesign -f -s "Apple Development: lihaochen910@hotmail.com (WV4J6EN6SW)" -d --entitlements "/Users/Kanbaru/Downloads/MyGame.app/Contents/entitlements.plist" "/Users/Kanbaru/Downloads/MyGame.app"

codesign -dv --verbose=4 "/Users/Kanbaru/Downloads/MyGame.app"
