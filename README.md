# NodeSetDoctor
A Microsoft Word plug-in that converts an OPC/UA xml NodeSet file into standard OPC/UA NodeSet documentation.

This repository contains code for a plug-in to enhance the operation of Microsoft Word.

To build, you need Microsoft Visual Studio 2022. At present, installation is done by building
and then starting program execution. As of the initial release, there is no separate installer
but one could be created if there was interest.

This is a VSTO (Visual Studio Tools for Office) DLL that is built in C#. It does not rely
on any third-party tool, such as Add-In Express (of which I am a fan), so there should be
no obstacles to building and using this library yourself.

Release History
Version 1.0 -- March 4, 2025
Initial release whcih does most things right but does not provide much helpful error
handling. If you encounter problems, start the plugin under Visual Studio 2022 which
will automatically start Microsoft Word running and installs the plugin for you. When
installed, you should see a new ribbon bar, "NodeSet Doctor", in Microsoft Word.

You can troubleshoot by running the plugin under the debugge and looking inside
the Output Window (Debug) for messages logged. There is a set of four logging flags
within the Model file (UAModel.cs) that you can use to increase or decrease the
verbosity of error messages that you see.

