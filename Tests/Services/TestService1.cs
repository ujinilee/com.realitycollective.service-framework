﻿// Copyright (c) xRealityLabs. All rights reserved.

using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;

namespace RealityToolkit.ServiceFramework.Tests.Services
{
    public class TestService1 : BaseServiceWithConstructor, ITestService
    {
        public const string TestName = "Test Service 1";

        public TestService1(string name = TestName, uint priority = 0)
            : base(name, priority)
        { }

        /// <inheritdoc />
        public bool IsEnabled { get; private set; }

        public override void Enable()
        {
            IsEnabled = true;
        }

        public override void Disable()
        {
            IsEnabled = false;
        }
    }
}