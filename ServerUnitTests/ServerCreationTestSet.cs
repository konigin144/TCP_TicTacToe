using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ClassLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ServerUnitTests
{
    [TestClass]
    public class ServerCreationTestSet
    {
        [TestMethod]
        public void WrongPortNumber()
        {
            try
            {
                Server server = new ServerAsync("127.0.0.1", 17);
                Assert.Fail();
            }
            catch(AssertFailedException e)
            {
                Assert.Fail();
            }
            catch(Exception e)
            {

            }
        }

        [TestMethod]
        public void WrongIPAddress()
        {
            try
            {
                Server server = new ServerAsync("127.0.0", 2048);
                Assert.Fail();
            }
            catch (AssertFailedException e)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public void CorrectPortNumberAndIPAddress()
        {
            try
            {
                Server server = new ServerAsync("127.0.0.1", 2048);
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }
    }
}
