@echo off
set v=9.1.0
tool\EditVersion dir="%cd%" v=%v% a="Assembly.cs"