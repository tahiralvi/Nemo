﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nemo.Configuration
{
    public class ConfigurationFactory
    {
        private static Lazy<IConfiguration> _configuration = new Lazy<IConfiguration>(() => DefaultConfiguration.New(), true);

        internal static IConfiguration Configuration
        {
            get
            {
                return _configuration.Value;
            }
        }

        public static IConfiguration Configure(Func<IConfiguration> config = null)
        {
            if (!_configuration.IsValueCreated)
            {
                if (config != null)
                {
                    _configuration = new Lazy<IConfiguration>(config, true);
                }
                return _configuration.Value;
            }
            return null;
        }
    }
}
