# PSMat

<p align="left">
<img src="./docs/images/vecteezy_royal-typewriter_73201.png" alt="Typewriter image sourced from vecteezy.com" >
</p>

PSMat is simple text editor built for educational purposes to supplement bachelor thesis about data structures and algorithms used in text editors. The name is shortcut from "Pisaci Stroj autoMat" which you can loosely translate to English as "type writer automata".

## Table of Contents
- [Introduction](#introduction)
- [Installation](#installation)
- [Usage](#usage)
- [Features](#features)
- [Contributing](#contributing)
- [License](#license)

## Introduction
The purpose of this program is to educate oneself about data structures and algorithms needed to build a "notepad with syntax highlighting", and try how hard (or easy) it is to create such app. 
The code is written in C# but no strict rules were followed when it comes to code architecture or programming paradigm / style. Originally there was no plan to put this code on GitHub and so no need for it to be international. As a result, naming of variables and functions in code may be a bit confusing for reader.

## Installation
Application was built and tested on Windows 11 and there is no guarantee below works on other systems too.

1. Download and install .NET 10 SDK from [https://dotnet.microsoft.com/en-us/download/dotnet/10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) 

2. Clone the repository or download zip from latest published release page [https://github.com/madamjak/psmat/releases/latest](https://github.com/madamjak/psmat/releases/latest) 
``` PowerShell
# Clone the repository
git clone https://github.com/madamjak/psmat.git
```

3. Build and install using the ps script in repo. You may get error about not digitally signed script.
``` PowerShell
# Navigate into the /scripts folder
cd psmat/scripts

# Run install script there, use -Overwrite parameter to overwrite files in existing PSMat folder
.\install-psmat.ps1 -ProgramPath "C:\Tools" -Scope User -Rebuild -Overwrite

# If execution fails can try for example Bypass policy
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope CurrentUser
```

4. Unless you change directory, the script should create PSMat directory in `C://Tools/PSMat` and add given directory to the PATH environment variable.
After that can execute the command `psmat <file in current directory OR absolute path to file>` from any location to open the editor, or run it without parameters to create new file and input the file location when prompted.
Example `userconfig.json` file allows you to change language for editor info messages or configure "dark mode".
``` PowerShell/
# Open an example file
psmat c://tools/psmat/config/userconfig.json
```

## Features
The editor offers basic text editing features you would expect from a text editor, supplemented by configurable syntax highlighting and search feature. Text editing operations were designed in an attempt to be standard and usual for most users. Some of the keyboard shortcuts may be conflicting with Windows Terminal, and it may be needed to change Terminal settings to be able to use them with editor. 
Personally I needed to change for example [Ctrl] + [V] for paste or [Ctrl] + [Shift] + [Home] to select text.

Syntax highlight is configurable via `jazyk.json` file located in ```/Config``` folder in your installation location. Example simple config shown bellow. The file allows to configure multiple languages, and the appropriate is selected 
based on file extension. Config allows to configure symbols for the source code comments, strings and list of regular expressions to match programming language keywords or other token types supported by editor. 
Editor uses own naive implementation of regex engine and only basic regex operations (concatenation/alternation/closure) are supported, NULL at the end of regex expressions is required for this to work.
``` JSON
{
    "Jazyky" : [
        {
            "Pripona": ".cs",
            "JednoriadkovyKomentar": "//",
            "ZaciatokKomentara": "/*",
            "KoniecKomentara": "*/",
            "ZaciatokRetazca": "\"",
            "KoniecRetazca": "\"",
            "Pravidla":[
                {
                    "TypTokenu":0,
                    "Regex":"((n.u.l.l)|(i.f)|(e.l.s.e)|(f.o.r.e.a.c.h)|(v.a.r)|(b.o.o.l)|(g.e.t)|(s.e.t)).\u0000"
                }
            ]
        }
    ]
}
```

Searching is allowed via editor CLI which can be displayed using [Ctrl] + [W] and closed using [Esc], quick search (`fnext`) is also possible using [Ctrl] + [F]. 
Strings being searched should be escaped in quotes. In addition to full-text search there also is "experimental" regex search, possible to be used with all search commands apart from `fprev`.
Similarly to language config, also this only supports basic operations, suplemented by simple implementation of keywords `\w` `\d` `\s` to match alpha-numeric chars, digits or white space char respectively.

Full list of search commands below. 
``` PowerShell
# find all occurences of string
fall "string" 

# find all strings matching pattern '\w*ing'
fall re{\w*ing}

# find next occurence of string
fnext "string" 

# find previous occurence of string
fprev "string" 

# replace first occurence of string1 with string2
rfrst "string1" "string2" 

# replace all occurences of string1 with string2
rall "string1" "string2" 
```

Edited file can be saved using [Ctrl] + [S] shortcut, which triggers editor command ```saas "file path"```. Alternatively can use [Ctrl] + [N] shortcut for "save as new filename" function.

## Contributing
If you wish to contribute to this repository, then create issue to track your change and then create PR.
``` PowerShell/
# Create branch
git checkout -b example-issue

# Commit
git commit -m 'Example issue'

# Push and create PR
git push -u origin example-issue
```

## License
The project is unlicensed, see the [LICENSE](LICENSE) file for details. 
