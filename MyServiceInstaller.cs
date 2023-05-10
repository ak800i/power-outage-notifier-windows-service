using System.ComponentModel;
using System.Configuration.Install;

[RunInstaller(true)]
public class MyServiceInstaller : Installer
{
    public MyServiceInstaller()
    {
        var serviceInstaller = new System.ServiceProcess.ServiceInstaller();
        var serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();

        // Configure the service installer
        serviceInstaller.ServiceName = "PowerOutageNotifier";
        serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;

        // Configure the service process installer
        serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;

        // Add the installers to the installer collection
        Installers.Add(serviceInstaller);
        Installers.Add(serviceProcessInstaller);
    }
}
