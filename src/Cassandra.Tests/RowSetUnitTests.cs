﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cassandra.Tests
{
    [TestFixture]
    public class RowSetUnitTests
    {
        [Test]
        public void RowIteratesThroughValues()
        {
            var rs = CreateStringsRowset(4, 1);
            var row = rs.First();
            //Use Linq's IEnumerable ToList: it iterates and maps to a list
            var cellValues = row.ToList();
            Assert.AreEqual("row_0_col_0", cellValues[0]);
            Assert.AreEqual("row_0_col_1", cellValues[1]);
            Assert.AreEqual("row_0_col_2", cellValues[2]);
            Assert.AreEqual("row_0_col_3", cellValues[3]);
        }

        [Test]
        public void RowSetIteratesTest()
        {
            var rs = CreateStringsRowset(2, 3);

            //Use Linq's IEnumerable ToList to iterate and map it to a list
            var rowList = rs.ToList();
            Assert.AreEqual(3, rowList.Count);
            Assert.AreEqual("row_0_col_0", rowList[0].GetValue<string>("col_0"));
            Assert.AreEqual("row_1_col_1", rowList[1].GetValue<string>("col_1"));
            Assert.AreEqual("row_2_col_0", rowList[2].GetValue<string>("col_0"));
        }

        [Test]
        public void RowSetCallsFetchNextTest()
        {
            //Create a rowset with 1 row
            var rs = CreateStringsRowset(1, 1, "a_");
            //It has paging state, stating that there are more pages
            rs.PagingState = new byte[] { 0 };
            //There is a subscriber to the event to fetch next
            rs.FetchNextPage += (pagingState) =>
            {
                return CreateStringsRowset(1, 1, "b_");
            };

            //use linq to iterate and map it to a list
            var rowList = rs.ToList();
            Assert.AreEqual(2, rowList.Count);
            Assert.AreEqual("a_row_0_col_0", rowList[0].GetValue<string>("col_0"));
            Assert.AreEqual("b_row_0_col_0", rowList[1].GetValue<string>("col_0"));
        }

        /// <summary>
        /// Ensures that in case there is an exception while retrieving the next page, it propagates.
        /// </summary>
        [Test]
        public void RowSetFetchNextPropagatesExceptionTest()
        {
            var rs = CreateStringsRowset(1, 1);
            //It has paging state, stating that there are more pages.
            rs.PagingState = new byte[] { 0 };
            //Throw a test exception when fetching the next page.
            rs.FetchNextPage += (pagingState) =>
            {
                throw new TestException();
            };

            //use linq to iterate and map it to a list
            try 
            {
                //The row set should throw an exception when getting the next page.
                var rowList = rs.ToList();
                Assert.Fail("It should throw a TestException");
            }
            catch (TestException)
            {
                //An exception of type TestException is expected.n  
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected exception of type TestException, got: " + ex.GetType());
            }
        }

        /// <summary>
        /// Creates a rowset.
        /// The columns are named: col_0, ..., col_n
        /// The rows values are: row_0_col_0, ..., row_m_col_n
        /// </summary>
        public RowSet CreateStringsRowset(int columnLength, int rowLength, string valueModifier = null)
        {
            var columns = new List<CqlColumn>();
            var columnIndexes = new Dictionary<string, int>();
            for (var i = 0; i < columnLength; i++)
            {
                var c = new CqlColumn()
                {
                    Index = i,
                    Name = "col_" + i,
                    TypeCode = ColumnTypeCode.Text,
                    Type = typeof(string)
                };
                columns.Add(c);
                columnIndexes.Add(c.Name, c.Index);
            }
            var rs = new RowSet();
            for (var j = 0; j < rowLength; j++)
            {
                var rowValues = new List<byte[]>();
                foreach (var c in columns)
                {
                    var value = valueModifier + "row_" + j + "_col_" + c.Index;
                    rowValues.Add(Encoding.UTF8.GetBytes(value));
                }
                rs.AddRow(new Row(rowValues.ToArray(), columns.ToArray(), columnIndexes));
            }
            return rs;
        }

        public class TestException : Exception { }
    }
}