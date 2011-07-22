using Chraft.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Chraft;

namespace Chrash
{
    
    
    /// <summary>
    ///This is a test class for WindowItemsPacketTest and is intended
    ///to contain all WindowItemsPacketTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WindowItemsPacketTest
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
        ///A test for WindowItemsPacket Constructor
        ///</summary>
        [TestMethod()]
        public void WindowItemsPacketConstructorTest()
        {
            WindowItemsPacket target = new WindowItemsPacket();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Read
        ///</summary>
        [TestMethod()]
        public void ReadTest()
        {
            WindowItemsPacket target = new WindowItemsPacket(); // TODO: Initialize to an appropriate value
            BigEndianStream stream = null; // TODO: Initialize to an appropriate value
            target.Read(stream);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void WriteTest()
        {
            WindowItemsPacket target = new WindowItemsPacket(); // TODO: Initialize to an appropriate value
            BigEndianStream stream = null; // TODO: Initialize to an appropriate value
            target.Write(stream);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Items
        ///</summary>
        [TestMethod()]
        public void ItemsTest()
        {
            WindowItemsPacket target = new WindowItemsPacket(); // TODO: Initialize to an appropriate value
            ItemStack[] expected = null; // TODO: Initialize to an appropriate value
            ItemStack[] actual;
            target.Items = expected;
            actual = target.Items;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for WindowId
        ///</summary>
        [TestMethod()]
        public void WindowIdTest()
        {
            WindowItemsPacket target = new WindowItemsPacket(); // TODO: Initialize to an appropriate value
            sbyte expected = 0; // TODO: Initialize to an appropriate value
            sbyte actual;
            target.WindowId = expected;
            actual = target.WindowId;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
