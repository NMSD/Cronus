﻿//using Machine.Specifications;
//using Microsoft.Extensions.Options;
//using System;

//namespace Elders.Cronus.Multitenancy
//{
//    [Subject("Tenants")]
//    public class When_parsing_tenantlist__from__IConfiguration__where_values_have_invalid_collection_set
//    {
//        Establish context = () =>
//        {
//            options = new TenantsOptionsMonitorMock("Maaaaaa@_!Vvvvv");
//        };

//        Because of = () => exception = Catch.Exception(() => new Tenants(options));

//        It should_throw_exception = () => exception.ShouldNotBeNull();

//        It should_throw__ArgumentException__ = () => exception.ShouldBeOfExactType<ArgumentException>();

//        It should_have_doc_in_exception_message = () => exception.Message.ShouldContain("Configuration.md");

//        static Exception exception;
//        static IOptionsMonitor<TenantsOptions> options;
//    }
//}
