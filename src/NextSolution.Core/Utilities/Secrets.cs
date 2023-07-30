using DeviceId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Utilities
{
    public class Secrets
    {
        public static string Key => AlgorithmHelper.GenerateHash(new DeviceIdBuilder().AddMachineName().AddOsVersion().AddUserName()
            .AddFileToken(Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, nameof(Key).ToLower())).ToString());
    }
}
