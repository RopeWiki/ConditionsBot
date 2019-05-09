@rem !! This file applies to Windows, adjust it to your platform if necessary
@rem !! Please, make sure that path to the Mono's "bin" directory is included in your Windows' PATH environment variable
@rem !! If it is not, uncomment the following line and include the correct path to the Mono's "bin" directory in it
@rem !! path=C:\Path_to_Mono\bin;%path%

@rem !! Uncomment the following line to recompile the "DotNetWikiBot.dll" library itself
@rem call gmcs -target:library -debug+ -optimize- -reference:"System.Xml.dll" -reference:"System.Web.dll" -reference:"System.Configuration.dll" DotNetWikiBot.cs

@rem !! If you use Page.ReviseInMSWord() function in your code, uncomment the following command
@rem !! to recompile the "DotNetWikiBot.dll" library with Microsoft Word interoperability enabled
@rem !! and make sure you've installed the proper PIA and the path to the PIA below is correct
@rem call gmcs -target:library -debug+ -optimize- -reference:"System.Xml.dll" -reference:"System.Web.dll" -reference:"System.Configuration.dll" -reference:"C:\Program Files\Microsoft.NET\Primary Interop Assemblies\Microsoft.Office.Interop.Word.dll" -define:MS_WORD_INTEROP DotNetWikiBot.cs

@rem !! Exclude "-debug+ -optimize-" and "--debug" options from the following lines to optimize bot performance after having debugged it
call gmcs -debug+ -optimize- -reference:DotNetWikiBot.dll BotScript.cs
mono --debug BotScript.exe
@pause