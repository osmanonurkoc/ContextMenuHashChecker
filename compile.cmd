@echo off
:: Set encoding to UTF-8
chcp 65001 >nul

echo Cleaning up artifacts from previous build...
:: Perform cleanup first (Prevents cache issues)
if exist bin rd /s /q bin
if exist obj rd /s /q obj

echo Compiling process starting...
dotnet publish -c Release -r win-x64 ^
    -p:Version=1.0.0.0 ^
    -p:FileVersion=1.0.0.0 ^
    -p:AssemblyVersion=1.0.0.0 ^
    -p:Company="Osman Onur Koc" ^
    -p:Product="Context Menu Hash Checker" ^
    -p:AssemblyTitle="Windows Context Menu Hash Checker" ^
    -p:Description="Calculates and compares MD5 SHA1 SHA256 SHA384 SHA512 hashes from context menu" ^
    -p:Copyright="www.osmanonurkoc.com" ^
    --self-contained true ^
    -p:PublishSingleFile=true

echo.
echo Program compiled successfully!
pause
