using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Annstore.Core.Common
{
    public sealed class AssemblyHelper : IAssemblyHelper
    {
        private static string Annstore = "ANNSTORE";

        public IEnumerable<Assembly> GetAnnstoreAssemblies()
        {
            var annstoreAssemblies = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     where assembly.FullName.ToUpper().Contains("ANNSTORE")
                                     select assembly;

            return annstoreAssemblies;
        }
    }
}
