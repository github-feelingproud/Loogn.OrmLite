﻿using Loogn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceTesting
{
    class Program
    {

        static void init()
        {
            CRL.SettingConfig.GetDbAccess = (type) =>
            {
                return new CoreHelper.SqlHelper(Utils.ConnStr);
            };
        }

        static void Main(string[] args)
        {
            init();

            //MappingTester.Test();
            //QueryTester.Test();
            SingleContextQueryTester.Test();
        }
    }
}
