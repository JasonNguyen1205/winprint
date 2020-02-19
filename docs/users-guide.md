# WinPrint User's Guide

## Installing

**On Windows**: Download and run the setup file. Once installed, WinPrint can be started from the Start menu or command line. The installer adds WinPrint to the `PATH` so that typing `winprint` from any command prompt will run the app.

**On Linux**: Good luck. Start by cloning the WinPrint repo, installing .NET Core 3, and buildng. Then I have some C++ libraries I'm using under the covers that you'll have to grab and build too. You'll need JUST THE RIGHT version of `libgdiplus`. It might help if you re-compile your kernel. You know, all the stuff reqruied to make anything work on one of several Linux distros. Did I mention how much [I hate linux](https://ceklog.kindel.com/2011/10/21/i-sincerely-tried-but-i-still-hate-linux/)? Seriously, it does work on Linux. But until someone begs me for it, I'm not spending another second on trying to build an installer. Submit an Issue (or Pull Request!) if you really want help.

**On Mac**: I haven't even tried as my old Macbook Air died and Apple wont fix it. I refuse to buy anything from Apple (except for for my family who are all all-in with Apple...sigh). I'll buy beer for someone who contributes to getting the Mac version working. It should not be hard given I've proven the stupid things works on Linux already.

## Command Line Interface

Examples:

Print Program.cs in landscape mode:

    winprint --landscape Program.cs

Print all .cs files on a specific printer with a specific paper size:

    winprint --printer "Fabricam 535" --paper-size A4 *.cs

Print the first two pages of Program.cs:

    winprint --from-sheet 1 --to-sheet 2 Program.cs

Print Program.cs using the 2 Up sheet defintion:

    winprint --sheet "2 Up" Program.cs

* `-s`, `--sheet` - Sheet defintion to use for formatting. Use sheet ID or friendly name.

* `-l`, `--landscape` - Force landscape orientation.

* `-r`, `--portrait` - Force portrait orientation.

* `-p`, `--printer` - Printer name.

* `-z`, `--paper-size` - Paper size name.

* `-f`, `--from-sheet` - (Default: 0) Number of first sheet to print (may be used with `--to-sheet`).

* `-t`, `--to-sheet` - (Default: 0) Number of last sheet to print (may be used with `--from-sheet`).

* `-c`, `--count-sheet` - (Default: false) Exit code is set to numer of sheet that would be printed. Use `--verbose` to diplsay the count.

* `-e`, `--content-type-engine` - Name of the Content Type Engine to use for rendering (`text/plain`, `text/html`, or `<language>`).

* `-v`, `--verbose` - (Default: false) Verbose console output (log is always verbose).

* `-d`, `--debug` - (Default: false) Debug-level console & log output.

* `-g`, `--gui` - (Default: false) Show WinPrint GUI (to preview or change sheet settings).

* `--help` - Display this help screen.

* `--version` - Display version information.

* `<files>` - Required. One or more files to be printed.

## Graphical User Interface

When run as a Windows app (`winprintgui.exe`), WinPrint provides an easy to use GUI for previewing how a file will be printed and changing many settings.

The **File button** opens a File Open Dialog for choosing the file to preview and/or print. The GUI app can print a single file at a time. Use the console verion (`winprint.exe`) to print multiple files at once.

The **Print button** prints the currently selected file.

The **Settings (⚙) button** will open `WinPrint.Config.json` in your favorite text editor. Changes made to the file will be reflected in the GUI automatically.

## Sheet Definitions

Font choices, header/footer options, and other print-job settings are defined in WinPrint as *Sheet Definitions*. In the WinPrint world a **Sheet** is a side of a sheet of paper. Depending on how its configured, WinPrint will print one or more **Pages** on each **Sheet**.

This is called "n-up" printing. The most common form of "n-up" printing is "2-up" where the page orientation is set to landscape and there are two columns of pages.

The layout and format of the **Sheet** is defined by a set of configuration settings called a **Sheet Definition**. Out of the box WinPrint comes with two: `Default 1 Up` and `Default 2 Up`.

**Sheet Definitions** are defined and stored in the `WinPrint.Config.json` configuration file found in `%appdata%\Kindel Systems\WinPrint`.

### Headers & Footers Macros

The format for header & footer specifiers is:

    <left part>|<center part>|<right part>

where `<left part>`,`<center part>`, and `<right part>` can be composed of text and any of the **Macros** described below. For example:

    {DateRevised:D}|{FullyQualifiedPath}|{FileType}

* `{NumPages}` - The total number of **Sheets** in the file.

* `{Page}` - The current **Sheet** number.

* `{FileExtension}` - The file extension of the file.

* `{FileName}` - The name of file without the extension.

* `{FilePath}` - The path to the file without the filename or extension.

* `{FullyQualifiedPath}` - The full path to the file, inclidng the filename and extension.

* `{FileType}` - The file type (for `text/plain` and `text/html`) or language (for `sourcecode`) of the file. 

* `{DatePrinted}` - The current date & time (see formatting below).

* `{DateRevised}` - The date & time the file was last revised.

The `{DatePrinted}` and `{DateRevised}` macros support the full set of [standard .NET date and time formatting modifiers](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings). For example `{DateRevised:M}` will generate just the month and date (e.g. `September 10`) while `{DateRevised:t}` will generate just the `short time` (`4:55 PM`).

### Modifying Sheet Definitions

The WinPrint GUI can be used to change most Sheet Definition settings. All settings can be changed by editing the `WinPrint.Config.json` file. Note if `WinPrint.Config.json` is changed while the WinPrint GUI App is running, it will detect the change and re-flow the currently loaded file. In other-words, a text editor can be used as the UI for advanced settings.

### Creating new Sheet Definitions

WinPrint starts with two "built-in" Sheet Definitions: `Default 1-Up` and `Default 2-Up`. Additional Sheet Definitions can be created by editing `WinPrint.Config.json`, copying one of the existing Sheet Defintions and giving it a new unique `name` and unique `ID`.

## Content Types

WinPrint supports three types of files. Support for each is provided by a WinPrint Content Type Engine (CTE):

1. **`text/plain`** - This CTE knows only how to print raw `text/plain` files. The format of the printed text can be changed (e.g. to turn off line numbers or use a differnt font). Lines that are too long for a page are wrapped at character boundaries. `\r` (formfeed) characters can be made to cause following text to print on the next page (this is off by default). Settings for the `text/plain` can be changed by editing the `textFileSettings` section of a Sheet Definition in the `WinPrint.Config.json` file. 

2. **`text/html`** - This CTE can render html files. Any CSS specified inline in the HTML file will be honored. External CSS files must be local. For HTML without CSS, the default CSS used can be overridden by providing a file named `winprint.css` in the `%appdata%\Kindel Systems\WinPrint` folder. `text/html` does not support line numbers.

3. **`text/sourcecode`** - The sourcecode CTE supports syntax highlighting (pretty printing), with optional line numbering, of over 200 programming languages. The style of the printing can be changed by providing a file named `winprint-prism.css` in the `%appdata%\Kindel Systems\WinPrint` folder. The styles defined in this format shold match those defined for [PrismJS](https://prismjs.com). Any PrismJS style sheet will work with WinPrint.

The extension of the file being printed (e.g. `.cs`) is determines which Content Type rendering engine will be used. WinPrint has a built-in library of hundreds of file extension to content type/language mappings.

To associate a file extension with a particular Content Type Engine modify the `files.associations` section of `WinPrint.Config.json` appropriately. For example to associate files with a `.htm` extension with the `text/html` Content Type Engine add a line as shown below (the `WinPrint.Config.json` generated when WinPrint runs the first time already provides this example, as an example):

    "files.associations": {
      "*.htm": "text/html",
    }

For associating file extentions with a particular programming language using the `text/sourcecode` Content Type Engine see below.

The commandline option `-e`/`--content-type-engine` overrides content type and language detection.

## Language Associations

WinPrint's `text/sourcecode` Content Type Engine knows how to syntax highlight (pretty print) over 200 programming languages. It has a built-in file extension to language mapping that should work for most modern scenarios. For example it knows that `.cs` files hold `C#` and `.bf` files hold `brainfuck`.

### Adding or Changing `text/sourcecode` Language Associations

To associate a file extension with a language spported by `text/sourcecode` modify the `files.associations` and `languages` sections of `WinPrint.Config.json` appropriately. For example to associate files with a `.config` extension with the JSON langauge  add a line as shown below (the `WinPrint.Config.json` generated when WinPrint runs the first time already provides this example, as an example):

    "files.associations": {
      "*.config": "json"
    }

To determine the name to use (e.g. `json`) see the [PrismJS](https://prismjs.com/#supported-languages) list of languages.

A new langauge can be defined by aliasing it to an existing language by modifying the `languages` section of `WinPrint.Config.json`. 

For example to enable the [Icon Programming Language](https://en.wikipedia.org/wiki/Icon_%28programming_language%29) which is a very C-like language the `files.associations` and `languages` sections would look like the following:

    "files.associations": {
      "*.icon": "icon"
    },
    "languages": [
      {
        "id": "icon",
        "extensions": [
          ".icon"
        ],
        "aliases": [
          "clike"
        ]
      }
    ]