using System.Diagnostics;
using System.Reflection;
using CookInformationViewer.Models.Db.Loader;
using NUnit.Framework;

namespace TestProject.Models.Db.Loader
{
    public class SqlLoaderTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            using var loader = new SqlLoader("LatestRecipes.xml", "TestProject.Resources.SQL", Assembly.GetExecutingAssembly());
            loader.SetQuery("test1");
            loader.SetParameter("ID", "0");

            var act = loader.ToString();
            var exp = "SELECT\r\n    *\r\nFROM\r\n    TABLE\r\nWHERE\r\n    ID = 0\r\n";

            Assert.AreEqual(exp, act);
        }

        [Test]
        public void Test2()
        {
            using var loader = new SqlLoader("LatestRecipes.xml", "TestProject.Resources.SQL", Assembly.GetExecutingAssembly());
            loader.SetQuery("test2");
            loader.SetParameter("ID", "0");
            loader.SetParameter("NAME", "TEST");
            loader.SetStatement("addCond");

            var act = loader.ToString();
            var exp = "SELECT\r\n    *\r\nFROM\r\n    TABLE\r\nWHERE\r\n    ID = 0\r\n    , NAME = TEST\r\n";

            Assert.AreEqual(exp, act);
        }
    }
}