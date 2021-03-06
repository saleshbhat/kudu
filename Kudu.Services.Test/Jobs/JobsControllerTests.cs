﻿using Kudu.Contracts.Jobs;
using Kudu.Contracts.Tracing;
using Kudu.Core.Infrastructure;
using Kudu.Services.Jobs;
using Kudu.TestHarness.Xunit;
using Moq;
using System;
using System.IO.Abstractions;
using System.Net;
using System.Net.Http;
using Xunit;
using Kudu.TestHarness;

namespace Kudu.Services.Test.Jobs
{
    [KuduXunitTestClass]
    public class JobsControllerTests
    {
        [Fact]
        public void InvokeTriggeredJob_ShouldReturn202()
        {
            var controller = new JobsController(
                Mock.Of<ITriggeredJobsManager>(),
                Mock.Of<IContinuousJobsManager>(),
                Mock.Of<ITracer>());

            controller.Request = new HttpRequestMessage();

            HttpResponseMessage resMsg = controller.InvokeTriggeredJob("foo");
            Assert.Equal(HttpStatusCode.Accepted, resMsg.StatusCode);
        }

        [Fact]
        public void InvokeARMTriggeredJob_ShouldReturn200()
        {
            var controller = new JobsController(
                Mock.Of<ITriggeredJobsManager>(),
                Mock.Of<IContinuousJobsManager>(),
                Mock.Of<ITracer>());

            controller.Request = new HttpRequestMessage();

            // Add header to simulate ARM request
            controller.Request.Headers.Add(Arm.ArmUtils.GeoLocationHeaderKey, "East US");

            HttpResponseMessage resMsg = controller.InvokeTriggeredJob("foo");
            Assert.Equal(HttpStatusCode.OK, resMsg.StatusCode);
        }

        [Fact]
        public void InvokeTriggeredJob_ReadOnlyFileSystem_ShouldReturn503()
        {
            var triggeredJobsManagerMock = new Mock<ITriggeredJobsManager>();

            // simulate read-only enviroment
            triggeredJobsManagerMock
                .Setup(t => t.InvokeTriggeredJob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws<UnauthorizedAccessException>();

            var controller = new JobsController(
                triggeredJobsManagerMock.Object,
                Mock.Of<IContinuousJobsManager>(),
                Mock.Of<ITracer>());

            var fileSystem = new Mock<IFileSystem>();
            var dirBase = new Mock<DirectoryBase>();
            fileSystem.Setup(f => f.Directory).Returns(dirBase.Object);
            dirBase.Setup(d => d.CreateDirectory(It.IsAny<string>())).Throws<UnauthorizedAccessException>();
            FileSystemHelpers.Instance = fileSystem.Object;

            controller.Request = new HttpRequestMessage();

            using (KuduUtils.MockAzureEnvironment())
            {
                HttpResponseMessage resMsg = controller.InvokeTriggeredJob("foo");
                Assert.Equal(HttpStatusCode.ServiceUnavailable, resMsg.StatusCode);
            }
        }
    }
}