﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.DomainModelling;


namespace NMSD.Cronus.Transports.Conventions
{
    public class CommandPipelinePerApplication : IPipelineNameConvention
    {
        public string GetPipelineName(Type commandType)
        {
            var assembly = commandType.Assembly;
            var boundedContext = assembly.GetAssemblyAttribute<BoundedContextAttribute>();
            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", assembly.FullName));

            return boundedContext.CommandsPipelineName;
        }
    }
}
