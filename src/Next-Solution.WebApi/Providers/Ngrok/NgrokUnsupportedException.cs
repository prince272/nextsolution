// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System.Runtime.InteropServices;

namespace Next_Solution.WebApi.Providers.Ngrok
{
    public class NgrokUnsupportedException : NotSupportedException
    {
        public NgrokUnsupportedException() : base($"Platform not supported by Ngrok {RuntimeInformation.OSDescription}-{RuntimeInformation.ProcessArchitecture}")
        {
        }
    }
}