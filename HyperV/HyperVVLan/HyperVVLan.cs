using System;
using System.Management.Automation;
using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Collections.Generic;
using System.IO;
using System.Data;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName = null;
        public string UserName = null;
        public string Password = null;

        public string VLanMode = null;


        public string VMName = null;
        public string VlanId = null;
        public string AllowedVlanIdList = null;
        public string NativeVlanId = null;
        public string PrimaryVlanId = null;
        public string SecondaryVlanId = null;
        public string SecondaryVlanIdList = null;

        public string VMNetworkAdapterName = null;
        private Dictionary<string, string> CreateParameters()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("VMName", VMName);
            parameters.Add("VLanMode", VLanMode);

            if (!string.IsNullOrEmpty(AllowedVlanIdList))
                parameters.Add("AllowedVlanIdList", AllowedVlanIdList);
            if (!string.IsNullOrEmpty(NativeVlanId))
                parameters.Add("NativeVlanId", NativeVlanId);
            if (!string.IsNullOrEmpty(PrimaryVlanId))
                parameters.Add("PrimaryVlanId", PrimaryVlanId);
            if (!string.IsNullOrEmpty(SecondaryVlanId))
                parameters.Add("SecondaryVlanId", SecondaryVlanId);
            if (!string.IsNullOrEmpty(SecondaryVlanIdList))
                parameters.Add("SecondaryVlanIdList", SecondaryVlanIdList);
            if (!string.IsNullOrEmpty(NativeVlanId))
                parameters.Add("NativeVlanId", NativeVlanId);
            if (!string.IsNullOrEmpty(VlanId))
                parameters.Add("VlanId", VlanId);
            if (!string.IsNullOrEmpty(VMNetworkAdapterName))
                parameters.Add("VMNetworkAdapterName", VMNetworkAdapterName);



            return parameters;
        }
        public ICustomActivityResult Execute()
        {
            string command = "Set-VMNetworkAdapterVlan";
            using (RemotePowershell rp = new RemotePowershell(UserName, Password, HostName))
            {
                PowerShell ps = rp.powerShell;
                var results = ps.AddCommand(command)
                    .AddParameters(CreateParameters())
                    .Invoke();
                if (rp.HasError)
                {
                    throw new Exception(rp.ErrorMessage);
                }

                return this.GenerateActivityResult(SuccessResult());
            }

        }

        private DataTable SuccessResult()
        {
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));
            dt.Rows.Add("Success");
            return dt;
        }


    }
    class RemotePowershell : IDisposable
    {
        private Runspace runspace;
        public PowerShell powerShell;
        public RemotePowershell(string username, string password, string host)
        {
            runspace = RunspaceFactory.CreateRunspace(CreateConnection(username, password, host));

            runspace.Open();
            powerShell = PowerShell.Create();
            powerShell.Runspace = runspace;
        }
        WSManConnectionInfo CreateConnection(string username, string password, string host)
        {
            SecureString securePassword = new SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }
            PSCredential creds = new PSCredential(username, securePassword);

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo();
            connectionInfo.ComputerName = host;
            connectionInfo.Credential = creds;
            connectionInfo.SkipCACheck = true;
            connectionInfo.SkipCNCheck = true;
            return connectionInfo;

        }
        public void Dispose()
        {
            try
            {
                powerShell.Dispose();
                runspace.Dispose();
            }
            catch
            {
            }

        }
        public bool HasError
        {
            get
            {
                return powerShell.Streams.Error.Count > 0;
            }
        }
        public string ErrorMessage
        {
            get
            {
                if (!HasError)
                {
                    return null;
                }
                StringWriter sw = new StringWriter();
                foreach (var error in powerShell.Streams.Error)
                {
                    sw.WriteLine(error.ToString());
                }
                return sw.ToString();
            }
        }
    }


}
