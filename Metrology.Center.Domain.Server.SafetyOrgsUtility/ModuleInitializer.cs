using DryIoc;
using Metrology.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Server.SafetyOrgsUtility
{
    public class ModuleInitializer : IModuleInitializer
    {
        private readonly IContainer _Container;

        public ModuleInitializer(IContainer container)
        {
            _Container = container;
        }

        public void Initialize()
        {
            _Container.Register<ISafetyOrgRepository, SafetyOrgRepository>();
            _Container.Register<ISafetyOrgsService, SafetyOrgsService>();
        }
    }
}
