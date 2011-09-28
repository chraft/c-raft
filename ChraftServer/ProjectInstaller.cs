using System;
using System.Collections;
using System.ComponentModel;


namespace ChraftServer
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            Context.Parameters["assemblypath"] = String.Format("\"{0}\" -service", Context.Parameters["assemblypath"]);
            Console.WriteLine("Installing with Assembly Path: {0}", Context.Parameters["assemblypath"]);
            base.OnBeforeInstall(savedState);
        }
    }
}
