# PSMat
PSMat is simple text editor built for educational purposes to supplement bachelor thesis about data structures and algorithms used in text editors.

## Introduction
The purpose of this program is to educate oneself about data structures and algorithms needed to build a "notepad with syntax highlighting", and how hard (or easy) it is to create such app. 

The code is written in C# and optimized for Windows 11 and no strict rules where followed when it comes to code architecture or programming style. Naming of variables and functions in code is a bit mix of english and slovak language, which might be a bit confusing for non-slovak speakers, but originally there was no plan to put this code on github and no need to be international, so it is what it is.

## Installation
Application was built and tested on Windows 11 and there is no guarantee below works on other systems too.

1. Download and install .NET 10 SDK from [https://dotnet.microsoft.com/en-us/download/dotnet/10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) 

2. Clone the repository or download as zip
``` PowerShell
# Clone the repository
git clone https://github.com/madamjak/psmat.git
```

3. Build and install using the ps script in repo.
``` PowerShell
# Navigate into the /scripts folder
cd psmat/scripts

# Run install script there
.\install-psmat.ps1 -ProgramPath "C:\Tools" -Scope User -Rebuild
```

## Usage and features
Ilustrated example installation should put the EXE into `C://Tools/PSMat` and add given directory to the PATH environment variable.
After that can execute the command `psmat <file in current directory OR absolute path to file>` from any location to open the editor.
It's possible to run the command without parameters too and input the file location when prompted.

``` PowerShell/
# Open any example file
psmat c://tools/psmat/config/lex/commands.json
```

The editor offers basic text editing features you would expect from a text editor, supplemented by configurable syntax highlighting and search feature.

## License and contributing
The project is unlicensed, see the [LICENSE](LICENSE) file for details. 
If you wish to contribute to this repository, then create issue to track your change and then create PR.
``` PowerShell/
# Create branch
git checkout -b example-issue

# Commit
git commit -m 'Example issue'

# Push and create PR
git push -u origin example-issue
```
