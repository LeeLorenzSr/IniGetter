# IniGetter

## Ini File Handler for .NET Standard and .NET Framework

 
Let's take a minute to configure the simple configuration file. It's been around in many forms over the past 60 or 70 years, the first files were probably simple lines of text, each line representing something special... but proprietary formats have to be documented, and suffer over time; they also are more inflexible and resist program enhancements. Pure binary configuration files are worse, requiring special programs to update, or components written to handle their creation. Eventually, programs began using simple key/value pairs and parsed the values. This allowed for human editable files and generic, reusable code could be created to provide access to these formats. As we entered the "Age of Microsoft" we saw the prototypical INI file format start to take shape. Key/value pairs could be broken into sections, and comments could be included in the file, to make human editing of the file easier and self-documented. The INI file has persisted from MS-DOS and lasted into the current age of operating systems, though it is, by all measures, very obsolete - replaced by JSON (and before that, XML) formats which are able to take complex data objects and serialize/deserialize them with ease.

Today, the INI file is mostly relegated to legacy systems; I'd recommend using JSON for any new projects, as it is relatively trivial to load and save in this format today. In some applications, INI files are written out by the application at runtime ("preferences"). Many enhancements to the INI file format were done to accommodate more complex data structures, often at the expense of human readability and editability.

IniGetter is a library intended to standardize access to this format and provide a robust interface to minimize potential issues and increase performance. Optionally, it can provide the ability to watch for changed files, values, and even write values back out.
## The basic format of an ini file is as follows:

    # Comments can begin with a pound sign
    ; ...or a semi-colon. Neither of these characters can be used in a section or key name
    
    # Global section is simply an unnamed section that begins the ini file. These values will live in section ""
    global=globalvalue
    
    # Section names can be alpha-numeric and may include spaces " ", underscores "_", dashes "-", exclamation points "!", at symbols "@", percent symbols "%", carats "^", periods ".", ampersands "&" and dollar signs "$"
    # Section and keynames will always be trimmed for whitespace at beginning and end
    [section]
    
    # Key names can include everything a section can. They can be quoted, but quotes will not be considered a part of the key name
    # values include everything after the equal sign, trimmed - unless quoted, in which case, the contents of the quotes are completely included
    # there can only be one unique key name per section. The last entry with a redundant key will "win" - warnings will be generated by the parser
    # values requiring "escaped" characters MUST be quoted
    key=value
    "another key"="another value  "
    longer key with space = some other value
    
    # data types
    # IniGetter will attempt to parse a value to whatever format you want, but boolean values are more robust.
    # Boolean can evaluate 0 or 1 (or any non-zero value), yes or no, on or off, true or false
    IsSomething=true
    IsAnotherThing=0
    IsNotAnything=no    


