using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using PnP.Core.Test.Utilities;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableActualDataTestsGraph
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task TestQueryWebs_Graph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var query = context.Site.AllWebs
                            .Load(w => w.Id, w => w.Title, w => w.Description);

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count > 0);
            }
        }

        [TestMethod]
        public async Task TestQueryLists_Graph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var query = (from l in context.Web.Lists
                             select l)
                            .Load(l => l.Id, l => l.Title, l => l.Description);

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count >= 1);
            }
        }

        [TestMethod]
        public async Task TestQueryItems_Graph()
        {
            //TestCommon.Instance.Mocking = false;
            
            (string listName, int id, string itemTitle) = await TestAssets.CreateTestListItemAsync(0);

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                context.GraphFirst = true;

                var query = (from i in context.Web.Lists.GetByTitle(listName).Items
                             where i.Title == itemTitle
                             select i)
                             .Load(l => l.Id, l => l.Title);

                var queryResult = query.ToList();

                // Ensure that we have 1 list in the collection of lists
                Assert.AreEqual(1, context.Web.Lists.Length);

                // Ensure that we have 1 item in the result and that its title is the expected one
                Assert.IsNotNull(queryResult);
                Assert.AreEqual(1, queryResult.Count);
                Assert.AreEqual(itemTitle, queryResult[0].Title);
            }

            await TestAssets.CleanupTestDedicatedListAsync(2);
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultNoPredicateLINQ_Graph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = context.Web.Lists.FirstOrDefault();

                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultWithPredicateLINQ_Graph()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = (from l in context.Web.Lists
                              select l)
                             .Load(l => l.Id, l => l.Title)
                             .FirstOrDefault(l => l.Title == expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultNoPredicateOnQueryLINQ_Graph()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = (from l in context.Web.Lists
                              where l.Title == expected
                              select l).FirstOrDefault();

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByTitleLINQ_Graph()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = context.Web.Lists.GetByTitle(expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByTitleWithFieldsLINQ_Graph()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = context.Web.Lists.GetByTitle(expected,
                    l => l.Id,
                    l => l.Title
                    );

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
                Assert.AreNotEqual(Guid.Empty, actual.Id);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByIdLINQ_Graph()
        {
            //TestCommon.Instance.Mocking = false;

            (string listName, int id, string itemTitle) = await TestAssets.CreateTestListItemAsync();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
            {
                context.GraphFirst = true;

                var library = context.Web.Lists.GetByTitle(listName);
                var firstItem = library.Items.GetById(1, 
                    i => i.Id, 
                    i => i.Title);

                Assert.IsNotNull(firstItem);
                Assert.AreEqual(id, firstItem.Id);
                Assert.AreEqual(firstItem.Title, itemTitle);
            }

            await TestAssets.CleanupTestDedicatedListAsync(2);
        }
    }
}