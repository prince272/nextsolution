using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.ViewRenderer
{
    public interface IViewRenderer
    {
        Task<string> RenderAsync(string name, object? model = null, CancellationToken cancellationToken = default);
    }
}