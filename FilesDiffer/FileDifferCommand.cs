using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using EnvDTE;
using System.Linq;
using EnvDTE80;
using System.Windows.Forms;
using System.IO;

namespace FilesDiffer
{
    internal sealed class FileDifferCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("6ec3b22e-943a-41fe-84bb-d2e84ae06194");
        private readonly Package package;
        private FileDifferCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }   
        public static FileDifferCommand Instance
        {
            get;
            private set;
        }
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        

       

        public static void Initialize(Package package)
        {
            Instance = new FileDifferCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            string file1, file2;

            if(canFilesBeCompared(dte, out file1, out file2))
            { 
                dte.ExecuteCommand("Tools.DiffFiles", $"\"{file1}\" \"{file2}\"");
            }



        }
        

        private static bool canFilesBeCompared(DTE2 dte, out string file1, out string file2)
        {
            var items = GetSelectedFiles(dte);

            file1 = items.ElementAtOrDefault(0);
            file2 = items.ElementAtOrDefault(1);

            if (items.Count() == 1)
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = Path.GetDirectoryName(file1);
                dialog.ShowDialog();

                file2 = dialog.FileName;
            }
            

            return !string.IsNullOrEmpty(file1) && !string.IsNullOrEmpty(file2);
        }

         public static IEnumerable<string> GetSelectedFiles(DTE2 dte)
        {
            var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;

            return from item in items.Cast<UIHierarchyItem>()
                   let pi = item.Object as ProjectItem
                   select pi.FileNames[1];
        }           
       
                    
    }
}
