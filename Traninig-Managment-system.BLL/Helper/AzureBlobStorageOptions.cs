using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Helper
{
    public class AzureBlobStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string PublicContainerName { get; set; } = string.Empty;
        public string PrivateContainerName { get; set; } = string.Empty;
    }
}
