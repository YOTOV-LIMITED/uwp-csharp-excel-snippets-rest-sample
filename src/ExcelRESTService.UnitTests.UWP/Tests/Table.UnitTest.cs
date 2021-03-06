﻿/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using AsyncAssert = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.AppContainer.Assert;

using ExcelRESTService.UnitTests.UWP.Helpers;

using Microsoft.ExcelServices;

namespace ExcelRESTService.UnitTests.UWP
{
    [TestClass]
    public class TableTests
    {
        [TestMethod]
        public async Task ListTables()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();
            
            // Act
            var tables = await App.ExcelService.ListTablesAsync(item.Id);
            // Assert
            Assert.AreEqual(1, tables.Length, "Count of tables is not 1");
            Assert.AreEqual("LogEntries", tables[0].Name, "First table is not named 'LogEntries'");
        }

        [TestMethod]
        public async Task GetTable_NonExisting_ThrowsException()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();
            // Act
            await AsyncAssert.ThrowsException<Exception>(
                 async () =>
                 {
                     var table = await App.ExcelService.GetTableAsync(item.Id, "TableX");
                 }
            );
            // Assert
        }

        [TestMethod]
        public async Task GetTable()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();
            // Act
            var table = await App.ExcelService.GetTableAsync(item.Id, "LogEntries");
            // Assert
            Assert.AreEqual("LogEntries", table.Name, "Table name is not 'LogEntries'");
            Assert.AreEqual(true, table.ShowHeaders);
            Assert.AreEqual(false, table.ShowTotals);
            Assert.AreEqual("TableStyleMedium2", table.Style);
        }

        [TestMethod]
        public async Task AddTable()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();
            var sessionId = (await App.ExcelService.CreateSessionAsync(item.Id)).Id;

            // Act
            var table = await App.ExcelService.AddTableAsync(item.Id, "Sheet3!A1:B4", true, sessionId);

            // Assert
            Assert.AreEqual("Table2", table.Name, "Name of new table is now 'Table2'");

            var tables = await App.ExcelService.ListTablesAsync(item.Id, sessionId);

            await App.ExcelService.CloseSessionAsync(item.Id, sessionId);

            Assert.AreEqual(2, tables.Length, "Count of tables is not 2");
        }

        [TestMethod]
        public async Task UpdateTable()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();

            var updatedTable =
                new Table()
                {
                    Name = "UpdatedTable",
                    ShowHeaders = true,
                    ShowTotals = true,
                    Style = "TableStyleMedium2"
                };

            // Act
            var table = await App.ExcelService.UpdateTableAsync(item.Id, "LogEntries", updatedTable);
            // Assert
        }

        [TestMethod]
        public async Task AddTableRow()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();
            var sessionId = (await App.ExcelService.CreateSessionAsync(item.Id)).Id;

            var tableName = "LogEntries";

            var values =
                new object[]
                {
                    new object[] { (int)1, DateTime.Now.ToString(), App.UserAccount.Name, "Work", (Double)36000.5, null }
                };

            // Act
            var row = await App.ExcelService.AddTableRowAsync(item.Id, tableName, values, null, sessionId);
            // Assert
            Assert.AreEqual(((object[])(values[0]))[0], row.Values[0][0], $"First column is not {((object[])(values[0]))[0]}");
            Assert.AreEqual(((object[])(values[0]))[2], row.Values[0][2], $"Third column is not {((object[])(values[0]))[2]}");
            Assert.AreEqual(((object[])(values[0]))[3], row.Values[0][3], $"Fourth column is not {((object[])(values[0]))[3]}");
            Assert.AreEqual(((object[])(values[0]))[4], row.Values[0][4], $"Fifth column is not {((object[])(values[0]))[4]}");
            // TODO: Check value of calculated column

            var dataBodyRange = await App.ExcelService.GetTableDataBodyRangeAsync(item.Id, tableName, sessionId);

            await App.ExcelService.CloseSessionAsync(item.Id, sessionId);

            Assert.AreEqual(27, dataBodyRange.RowCount, "Table does not have 1 more row");
        }

        [TestMethod]
        public async Task AddMultipleTableRows()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();
            var sessionId = (await App.ExcelService.CreateSessionAsync(item.Id)).Id;

            var tableName = "LogEntries";

            // Act
            for (int i = 0; i < 10; i++)
            {
                var values =
                    new object[]
                    {
                        new object[] { (int)(i+1), DateTime.Now.ToString(), App.UserAccount.Name, "Work", (Double)(36000.5 + i * 100), null }
                    };

                var row = await App.ExcelService.AddTableRowAsync(item.Id, "LogEntries", values, null, sessionId);

                // Assert
                Assert.AreEqual(((object[])(values[0]))[0], row.Values[0][0], $"First column is not {((object[])(values[0]))[0]}");
                Assert.AreEqual(((object[])(values[0]))[2], row.Values[0][2], $"Third column is not {((object[])(values[0]))[2]}");
                Assert.AreEqual(((object[])(values[0]))[3], row.Values[0][3], $"Fourth column is not {((object[])(values[0]))[3]}");
                Assert.AreEqual(((object[])(values[0]))[4], row.Values[0][4], $"Fifth column is not {((object[])(values[0]))[4]}");
                // TODO: Check value of calculated column
            }

            // Assert
            var dataBodyRange = await App.ExcelService.GetTableDataBodyRangeAsync(item.Id, tableName, sessionId);

            await App.ExcelService.CloseSessionAsync(item.Id, sessionId);

            Assert.AreEqual(36, dataBodyRange.RowCount, "Table does not have 10 more rows");
        }

        [TestMethod]
        public async Task AddTableColumn()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();

            var table = await App.ExcelService.AddTableAsync(item.Id, "Sheet3!A1:B4", true);

            var values =
                new object[]
                {
                    new object[] { "New Column" },
                    new object[] { "D" },
                    new object[] { "E" },
                    new object[] { "F" }
                };

            // Act
            var column = await App.ExcelService.AddTableColumnAsync(item.Id, table.Name, values);
            // Assert
            Assert.AreEqual(((object[])(values[0]))[0], column.Values[0][0], $"First row is not {((object[])(values[0]))[0]}");
            Assert.AreEqual(((object[])(values[1]))[0], column.Values[1][0], $"Second row is not {((object[])(values[1]))[0]}");
            Assert.AreEqual(((object[])(values[2]))[0], column.Values[2][0], $"Third row is not {((object[])(values[2]))[0]}");
            Assert.AreEqual(((object[])(values[3]))[0], column.Values[3][0], $"Fourth row is not {((object[])(values[3]))[0]}");
            // TODO: Check that the table now has 1 more column
        }

        [TestMethod]
        public async Task GetTableRange()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();

            var values =
                new object[]
                {
                        new object[] { "ID", "Created", "Username", "Note", "Odometer", "Distance" }
                };

            // Act
            var range = await App.ExcelService.GetTableRangeAsync(item.Id, "LogEntries");
            // Assert
            Assert.AreEqual(27, range.RowCount, "RowCount is not correct");
            Assert.AreEqual(6, range.ColumnCount, "ColumnCount is not correct");

            Assert.AreEqual(((object[])(values[0]))[0], range.Values[0][0], $"First column is not {((object[])(values[0]))[0]}");
            Assert.AreEqual(((object[])(values[0]))[1], range.Values[0][1], $"Second column is not {((object[])(values[0]))[1]}");
            Assert.AreEqual(((object[])(values[0]))[2], range.Values[0][2], $"Third column is not {((object[])(values[0]))[2]}");
            Assert.AreEqual(((object[])(values[0]))[3], range.Values[0][3], $"Fourth column is not {((object[])(values[0]))[3]}");
            Assert.AreEqual(((object[])(values[0]))[4], range.Values[0][4], $"Fifth column is not {((object[])(values[0]))[4]}");
        }

        [TestMethod]
        public async Task GetTableHeaderRowRange()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();

            var values =
                new object[]
                {
                        new object[] { "ID", "Created", "Username", "Note", "Odometer", "Distance" }
                };

            // Act
            var row = await App.ExcelService.GetTableHeaderRowRangeAsync(item.Id, "LogEntries");
            // Assert
            Assert.AreEqual(1, row.RowCount, "RowCount is not correct");
            Assert.AreEqual(6, row.ColumnCount, "ColumnCount is not correct");

            Assert.AreEqual(((object[])(values[0]))[0], row.Values[0][0], $"First column is not {((object[])(values[0]))[0]}");
            Assert.AreEqual(((object[])(values[0]))[1], row.Values[0][1], $"Second column is not {((object[])(values[0]))[1]}");
            Assert.AreEqual(((object[])(values[0]))[2], row.Values[0][2], $"Third column is not {((object[])(values[0]))[2]}");
            Assert.AreEqual(((object[])(values[0]))[3], row.Values[0][3], $"Fourth column is not {((object[])(values[0]))[3]}");
            Assert.AreEqual(((object[])(values[0]))[4], row.Values[0][4], $"Fifth column is not {((object[])(values[0]))[4]}");
        }

        [TestMethod]
        public async Task GetTableHeaderRowRangeWithSelect()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();

            var values =
                new object[]
                {
                        new object[] { "ID", "Created", "Username", "Note", "Odometer", "Distance" }
                };

            // Act
            var row = await App.ExcelService.GetTableHeaderRowRangeAsync(item.Id, "LogEntries", "", "$select=values,rowcount,columncount");
            // Assert
            Assert.AreEqual(1, row.RowCount, "RowCount is not correct");
            Assert.AreEqual(6, row.ColumnCount, "ColumnCount is not correct");

            Assert.AreEqual(((object[])(values[0]))[0], row.Values[0][0], $"First column is not {((object[])(values[0]))[0]}");
            Assert.AreEqual(((object[])(values[0]))[1], row.Values[0][1], $"Second column is not {((object[])(values[0]))[1]}");
            Assert.AreEqual(((object[])(values[0]))[2], row.Values[0][2], $"Third column is not {((object[])(values[0]))[2]}");
            Assert.AreEqual(((object[])(values[0]))[3], row.Values[0][3], $"Fourth column is not {((object[])(values[0]))[3]}");
            Assert.AreEqual(((object[])(values[0]))[4], row.Values[0][4], $"Fifth column is not {((object[])(values[0]))[4]}");
        }

        [TestMethod]
        public async Task GetTableDataBodyRange()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();
            // Act
            var range = await App.ExcelService.GetTableDataBodyRangeAsync(item.Id, "LogEntries");
            // Assert
            Assert.AreEqual(26, range.RowCount, "Row count is not 26");
            Assert.AreEqual(6, range.ColumnCount, "Column count is not 6");
        }

        [TestMethod]
        public async Task GetTableDataBodyRangeWithSelect()
        {
            // Arrange
            var item = await TestHelpers.UploadFile();
            // Act
            var range = await App.ExcelService.GetTableDataBodyRangeAsync(item.Id, "LogEntries", "", "$select=rowCount, columnCount");
            // Assert
            Assert.AreEqual(26, range.RowCount, "Row count is not 26");
            Assert.AreEqual(6, range.ColumnCount, "Column count is not 6");
        }

    }
}
