using Chraft.Plugins.IrcPlugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Chraft.Plugins.IrcPlugin.Tests
{
    
    
    /// <summary>
    ///This is a test class for HostMaskTest and is intended
    ///to contain all HostMaskTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HostMaskTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for HostMask Constructor
        ///</summary>
        [TestMethod()]
        public void HostMaskConstructorTest()
        {
            string mask = "SafeMask"; 
            HostMask target = new HostMask(mask);
            Assert.AreEqual(mask,target.Mask);
        }

        /// <summary>
        ///A test for Nickname
        ///</summary>
        [TestMethod()]
        public void SafeMaskNicknameTest()
        {
            string mask = "SafeMask";
            HostMask target = new HostMask(mask); 
            string actual;
            actual = target.Nickname;
            Assert.AreEqual(mask, actual);
        }

        [TestMethod()]
        public void UnSafeMaskNicknameTest()
        {
            string mask = "SafeMask!";
            HostMask target = new HostMask(mask); 
            string actual;
            actual = target.Nickname;
            Assert.AreEqual("SafeMask", actual);
        }

        /// <summary>
        ///A test for Server
        ///</summary>
        [TestMethod()]
        public void ServerTest()
        {
            string mask = "@Server!"; 
            HostMask target = new HostMask(mask); 
            string actual;
            actual = target.Server;
            Assert.AreEqual("Server", actual);
        }
    }
}
