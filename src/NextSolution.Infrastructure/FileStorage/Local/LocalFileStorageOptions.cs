using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.FileStorage.Local
{
    public class LocalFileStorageOptions
    {
        public string RootPath { get; set; } = default!;

        public string WebRootPath { get; set; } = default!;
    }
}
