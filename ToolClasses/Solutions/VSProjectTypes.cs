﻿using System.Collections.Generic;
using System.Linq;

namespace ToolClasses.Solutions
{
    public class VSProjectTypes
    {
        public static Dictionary<string, VSProjectType> ProjectTypes;
        public static Dictionary<string, VSProjectType> ProjectTypesByName;

        static VSProjectTypes()
        {
            var projecTypeList = new List<VSProjectType>
            {
                // source : https://www.codeproject.com/Reference/720512/List-of-Visual-Studio-Project-Type-GUIDs
                // search: https://www.google.com/search?q=2150E333-8FDC-42A3-9474-1A3956D46DE8&ie=UTF-8&oe=

                new VSProjectType("ASP.NET Core", "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"),

                new VSProjectType("ASP.NET 5", "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}"),
                new VSProjectType("ASP.NET MVC 1", "{603C0E0B-DB56-11DC-BE95-000D561079B0}"),
                new VSProjectType("ASP.NET MVC 2", "{F85E285D-A4E0-4152-9332-AB1D724D3325}"),
                new VSProjectType("ASP.NET MVC 3", "{E53F8FEA-EAE0-44A6-8774-FFD645390401}"),
                new VSProjectType("ASP.NET MVC 4", "{E3E379DF-F4C6-4180-9B81-6769533ABE47}"),
                new VSProjectType("C#", "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"),
                new VSProjectType("C++", "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}"),
                new VSProjectType("Database", "{A9ACE9BB-CECE-4E62-9AA4-C7E7C5BD2124}"),
                new VSProjectType("Database(other project types)", "{4F174C21-8C12-11D0-8340-0000F80270F8}"),
                new VSProjectType("Deployment Cab", "{3EA9E505-35AC-4774-B492-AD1749C4943A}"),
                new VSProjectType("Deployment Merge Module", "{06A35CCD-C46D-44D5-987B-CF40FF872267}"),
                new VSProjectType("Deployment Setup", "{978C614F-708E-4E1A-B201-565925725DBA}"),
                new VSProjectType("Deployment Smart Device Cab", "{AB322303-2255-48EF-A496-5904EB18DA55}"),
                new VSProjectType("Distributed System", "{F135691A-BF7E-435D-8960-F99683D2D49C}"),
                new VSProjectType("Dynamics 2012 AX C# in AOT", "{BF6F8E12-879D-49E7-ADF0-5503146B24B8}"),
                new VSProjectType("F#", "{F2A71F9B-5D33-465A-A702-920D77279786}"),
                new VSProjectType("J#", "{E6FDF86B-F3D1-11D4-8576-0002A516ECE8}"),
                new VSProjectType("Legacy(2003) Smart Device(C#)", "{20D4826A-C6FA-45DB-90F4-C717570B9F32}"),
                new VSProjectType("Legacy (2003) Smart Device(VB.NET)", "{CB4CE8C6-1BDB-4DC7-A4D3-65A1999772F8}"),
                new VSProjectType("Micro Framework", "{b69e3092-b931-443c-abe7-7e7b65f2a37f}"),
                new VSProjectType("Mono for Android/Xamarin.Android", "{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}"),
                new VSProjectType("MonoTouch/Xamarin.iOS", "{6BC8ED88-2882-458C-8E55-DFD12B67127B}"),
                new VSProjectType("MonoTouch Binding", "{F5B4F3BC-B597-4E2B-B552-EF5D8A32436F}"),
                new VSProjectType("Portable Class Library", "{786C830F-07A1-408B-BD7F-6EE04809D6DB}"),
                new VSProjectType("Project Folders", "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}"),
                new VSProjectType("SharePoint(C#)", "{593B0543-81F6-4436-BA1E-4747859CAAE2}"),
                new VSProjectType("SharePoint(VB.NET)", "{EC05E597-79D4-47f3-ADA0-324C4F7C7484}"),
                new VSProjectType("SharePoint Workflow", "{F8810EC1-6754-47FC-A15F-DFABD2E3FA90}"),
                new VSProjectType("Silverlight", "{A1591282-1198-4647-A2B1-27E5FF5F6F3B}"),
                new VSProjectType("Smart Device(C#)", "{4D628B5B-2FBC-4AA6-8C16-197242AEB884}"),
                new VSProjectType("Smart Device (VB.NET)", "{68B1623D-7FB9-47D8-8664-7ECEA3297D4F}"),
                new VSProjectType("Solution Folder", "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"),
                new VSProjectType("Test", "{3AC096D0-A1C2-E12C-1390-A8335801FDAB}"),
                new VSProjectType("Universal Windows Class Library", "{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A}"),
                new VSProjectType("VB.NET", "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}"),
                new VSProjectType("Visual Database Tools", "{C252FEB5-A946-4202-B1D4-9916A0590387}"),
                new VSProjectType("Visual Studio 2015 Installer Project Extension", "{54435603-DBB4-11D2-8724-00A0C9A8B90C}"),
                new VSProjectType("Visual Studio Tools for Applications(VSTA)", "{A860303F-1F3F-4691-B57E-529FC101A107}"),
                new VSProjectType("Visual Studio Tools for Office(VSTO)", "{BAA0C2D2-18E2-41B9-852F-F413020CAA33}"),
                new VSProjectType("Web Application", "{349C5851-65DF-11DA-9384-00065B846F21}"),
                new VSProjectType("Web Site", "{E24C65DC-7377-472B-9ABA-BC803B73C61A}"),
                new VSProjectType("Windows Communication Foundation(WCF)", "{3D9AD99F-2412-4246-B90B-4EAA41C64699}"),
                new VSProjectType("Windows Phone 8/8.1 Blank / Hub / Webview App", "{76F1466A-8B6D-4E39-A767-685A06062A39}"),
                new VSProjectType("Windows Phone 8/8.1 App(C#)", "{C089C8C0-30E0-4E22-80C0-CE093F111A43}"),
                new VSProjectType("Windows Phone 8/8.1 App(VB.NET)", "{DB03555F-0C8B-43BE-9FF9-57896B3C5E56}"),
                new VSProjectType("Windows Presentation Foundation(WPF)",  "{60DC8134-EBA5-43B8-BCC9-BB4BC16C2548}"),
                new VSProjectType("Windows Store(Metro) Apps & Components",  "{BC8A1FFA-BEE3-4634-8014-F334798102B3}"),
                new VSProjectType("Workflow(C#)", "{14822709-B5A1-4724-98CA-57A101D1B079}"),
                new VSProjectType("Workflow(VB.NET)", "{D59BE175-2ED0-4C54-BE3D-CDAA9F3214C8}"),
                new VSProjectType("Workflow Foundation", "{32F31D43-81CC-4C15-9DE6-3FC5453562B6}"),
                new VSProjectType("XNA(Windows)", "{6D335F3A-9D43-41b4-9D22-F6F17C4BE596}"),
                new VSProjectType("XNA(XBox)", "{2DF5C3F4-5A5F-47a9-8E94-23B4456F55E2}"),
                new VSProjectType("XNA(Zune)", "{D399B71A-8929-442a-A9AC-8BEC78BB2433}")
            };

            ProjectTypes = projecTypeList.ToDictionary(x => x.ProjectTypeGuid);

            ProjectTypesByName = projecTypeList.ToDictionary(x => x.Name);
        }
    }
}
