using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SyncObjX.Service
{
    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "SyncObjX Integration Service";
            service.Description = "Hosts SyncObjX Integration Framework services.";
            Installers.Add(process);
            Installers.Add(service);
        }
    }
}