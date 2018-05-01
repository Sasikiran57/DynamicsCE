﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Dynamics365.UIAutomation.Browser;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Dynamics365.UIAutomation.Api
{
    /// <summary>
    /// Xrm Grid Page
    /// </summary>
    /// <seealso cref="Microsoft.Dynamics365.UIAutomation.Api.XrmPage" />
    public class Grid
        : XrmPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        /// <param name="browser">The browser.</param>
        public Grid(InteractiveBrowser browser)
            : base(browser)
        {
            SwitchToContent();

            browser.Driver.WaitUntilVisible(By.Id(Elements.ElementId[Reference.Frames.ViewFrameId]),
                                            new TimeSpan(0, 0, 1),
                                            x=> { SwitchToView(); });
        }

        /// <summary>
        /// Opens the view picker.
        /// </summary>
        public BrowserCommandResult<Dictionary<string, Guid>> OpenViewPicker(int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("Open View Picker"), driver =>
            {
                var dictionary = new Dictionary<string, Guid>();

                driver.WaitUntilClickable(By.XPath(Elements.Xpath[Reference.Grid.ViewSelector]),
                                         new TimeSpan(0,0,20),
                                         d=> { d.ClickWhenAvailable(By.XPath(Elements.Xpath[Reference.Grid.ViewSelector])); },
                                         d=> { throw new Exception("Unable to click the View Picker"); });                

                driver.WaitUntilVisible(By.ClassName(Elements.CssClass[Reference.Grid.ViewContainer]),
                                        new TimeSpan(0, 0, 20),
                                        null,
                                        d => 
                                        {
                                            //Fix for Firefox not clicking the element in the event above. Issue with the driver. 
                                            d.ClickWhenAvailable(By.XPath(Elements.Xpath[Reference.Grid.ViewSelector]));
                                            driver.WaitUntilVisible(By.ClassName(Elements.CssClass[Reference.Grid.ViewContainer]), new TimeSpan(0, 0, 3), null, e => { throw new Exception("View Picker menu is not avilable"); });

                                        });

                var viewContainer = driver.FindElement(By.ClassName(Elements.CssClass[Reference.Grid.ViewContainer]));
                var viewItems = viewContainer.FindElements(By.TagName("li"));

                foreach (var viewItem in viewItems)
                {
                    if (viewItem.GetAttribute("role") != null && viewItem.GetAttribute("role") == "menuitem")
                    {
                        var links = viewItem.FindElements(By.TagName("a"));

                        if (links != null && links.Count > 1)
                        {
                            var title = links[1].GetAttribute("title");
                            Guid guid;

                            if (Guid.TryParse(viewItem.GetAttribute("id"), out guid))
                            {
                                //Handle Duplicate View Names
                                //Temp Fix
                                if(!dictionary.ContainsKey(title))
                                    dictionary.Add(title, guid);
                            }
                        }
                    }
                }

                return dictionary;
            });
        }

        /// <summary>
        /// Switches the view.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.SwitchView("Active Accounts");</example>
        public BrowserCommandResult<bool> SwitchView(string viewName, int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("Switch View"), driver =>
            {
                var views = OpenViewPicker().Value;
                Thread.Sleep(2000);
                if (!views.ContainsKey(viewName))
                {
                    throw new InvalidOperationException($"No view with the name '{viewName}' exists.");
                }

                var viewId = views[viewName];

                // Get the LI element with the ID {guid} for the ViewId.
                var viewContainer = driver.WaitUntilAvailable(By.Id(viewId.ToString("B").ToUpper()));
                var viewItems = viewContainer.FindElements(By.TagName("a"));

                foreach (var viewItem in viewItems)
                {
                    if (viewItem.Text == viewName)
                    {
                        viewItem.Click();
                    }
                }

                return true;
            });
        }


        /// <summary>
        /// Opens the chart.
        /// </summary>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.OpenChart();</example>
        public BrowserCommandResult<bool> OpenChart(int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("OpenChart"), driver =>
            {
                driver.ClickWhenAvailable(By.XPath(Elements.Xpath[Reference.Grid.OpenChart]));

                return true;
            });
        }

        /// <summary>
        /// Closes the chart.
        /// </summary>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.CloseChart();</example>
        public BrowserCommandResult<bool> CloseChart(int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("CloseChart"), driver =>
            {
                driver.ClickWhenAvailable(By.XPath(Elements.Xpath[Reference.Grid.CloseChart]));

                return true;
            });
        }

        /// <summary>
        /// Pins this instance.
        /// </summary>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.Pin();</example>
        public BrowserCommandResult<bool> Pin(int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("Pin"), driver =>
            {
                driver.ClickWhenAvailable(By.XPath(Elements.Xpath[Reference.Grid.DefaultViewIcon]));

                return true;
            });
        }

        /// <summary>
        /// Searches the specified search criteria.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.Search("Test API");</example>
        public BrowserCommandResult<bool> Search(string searchCriteria, int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("Search"), driver =>
            {
                driver.WaitUntilClickable(By.XPath(Elements.Xpath[Reference.Grid.FindCriteria]));
                driver.FindElement(By.XPath(Elements.Xpath[Reference.Grid.FindCriteria])).SendKeys(searchCriteria);
                driver.FindElement(By.XPath(Elements.Xpath[Reference.Grid.FindCriteria])).SendKeys(Keys.Enter);

                return true;
            });
        }

        /// <summary>
        /// Sorts the specified column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.Sort("Account Name");</example>
        public BrowserCommandResult<bool> Sort(string columnName,int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions($"Sort by {columnName}"), driver =>
            {
                var sortCols = driver.FindElements(By.ClassName(Elements.CssClass[Reference.Grid.SortColumn]));
                var sortCol = sortCols.FirstOrDefault(x => x.Text == columnName);
                if (sortCol == null)
                    throw new InvalidOperationException($"Column: {columnName} Does not exist");
                else
                    sortCol.Click();
                return true;
            });
        }

        /// <summary>
        /// Opens the grid record.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.OpenRecord(0);</example>
        public BrowserCommandResult<bool> OpenRecord(int index,int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("Open Grid Record"), driver =>
            {
                var rowType = driver.FindElement(By.XPath(Elements.Xpath[Reference.Grid.FirstRow])).GetAttribute("otypename");

                var itemsTable = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.Grid.GridBodyTable]));
                var links = itemsTable.FindElements(By.XPath(Elements.Xpath[Reference.Grid.GridBodyTableRow]));

                var clicked = false;

                if (links.Any() && (index) < links.Count)
                {
                    // look for any span tag within the tr and click on that
                    var blankSpans = links[index].FindElements(By.XPath(Elements.Xpath[Reference.Grid.GridBodyTableRowSpan]));
                    if (blankSpans.Any())
                    {
                        driver.DoubleClick(blankSpans[0]);
                    }
                    else
                    {
                        // as a fall back click on the row
                        driver.DoubleClick(links[index]);
                    }
                    clicked = true;
                }
                else
                {
                    throw new InvalidOperationException($"No record with the index '{index}' exists.");
                }

                if (clicked)
                {
                    if (rowType != "report")
                    {
                        SwitchToContent();
                        driver.WaitForPageToLoad();
                        driver.WaitUntilClickable(By.XPath(Elements.Xpath[Reference.Entity.Form]),
                                                    new TimeSpan(0, 0, 30),
                                                    null,
                                                    d => { throw new Exception("CRM Record is Unavailable or not finished loading. Timeout Exceeded"); }
                                                );
                    }
                    return true;
                }
               else
               {
                   throw new InvalidOperationException($"No record with the index '{index}' exists.");
                }
            });
        }

        /// <summary>
        /// Selects the grid record.
        /// </summary>
        /// <param name="index">The index of the row you want to select. Index starts with 0.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.SelectRecord(1);</example>
        public BrowserCommandResult<bool> SelectRecord(int index, int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("Select Grid Record"), driver =>
            {
                //index parameter will be 0 based but the Xpath is 1 based. So we need to increment.
                index++;

                var select = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.Grid.RowSelect].Replace("[INDEX]", index.ToString())),
                                                        $"Row with index {index.ToString()} is not found");

                select?.Click();
                
                return false;
            });
        }

        /// <summary>
        /// Filters the by letter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.FilterByLetter('A');</example>
        /// <exception cref="System.InvalidOperationException">Filter criteria is not valid.</exception>
        public BrowserCommandResult<bool> FilterByLetter(char filter, int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            if (!Char.IsLetter(filter) && filter != '#')
                throw new InvalidOperationException("Filter criteria is not valid.");

            return this.Execute(GetOptions("Filter by Letter"), driver =>
            {
                var jumpBar = driver.FindElement(By.XPath(Elements.Xpath[Reference.Grid.JumpBar]));
                var letterCells = jumpBar.FindElements(By.TagName("TD"));

                foreach (var letter in letterCells)
                {
                    if (letter.Text == filter.ToString())
                        letter.Click();
                }
               
                return true;
            });
        }

        /// <summary>
        /// Filters the by all.
        /// </summary>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.FilterByAll();</example>
        public BrowserCommandResult<bool> FilterByAll(int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("Filter by All Records"), driver =>
            {
                var showAll = driver.FindElement(By.XPath(Elements.Xpath[Reference.Grid.ShowAll]));

                showAll?.Click();

                return true;
            });
        }

        /// <summary>
        /// Opens the filter.
        /// </summary>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.EnableFilter();</example>
        public BrowserCommandResult<bool> EnableFilter(int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            return this.Execute(GetOptions("Enable Filter"), driver =>
            {
                var filter = driver.WaitUntilAvailable(By.XPath(Elements.Xpath[Reference.Grid.Filter]),
                                                        "Filter option is not available");

                filter?.Click();

                return true;
            });
        }

        /// <summary>
        /// Switches the chart on the Grid.
        /// </summary>
        /// <param name="chartName">Name of the chart.</param>
        /// <param name="thinkTime">Used to simulate a wait time between human interactions. The Default is 2 seconds.</param>
        /// <example>xrmBrowser.Grid.SwitchChart("Accounts by Owner");</example>
        public BrowserCommandResult<bool> SwitchChart(string chartName, int thinkTime = Constants.DefaultThinkTime)
        {
            Browser.ThinkTime(thinkTime);

            if (!Browser.Driver.IsVisible(By.XPath(Elements.Xpath[Reference.Grid.ChartList])))
                OpenChart();

            Browser.ThinkTime(1000);

            return this.Execute(GetOptions("Switch Chart"), driver =>
            {
                driver.ClickWhenAvailable(By.XPath(Elements.Xpath[Reference.Grid.ChartList]));

                var dialog = driver.FindElement(By.XPath(Elements.Xpath[Reference.Grid.ChartDialog]));
                var menuItems = dialog.FindElements(By.TagName("a"));
                IWebElement selectedItem = null;

                foreach (var item in menuItems)
                {
                    if (item.GetAttribute("title") == chartName)
                        selectedItem = item;
                }

                if (selectedItem == null)
                    throw new InvalidOperationException($"Chart with name {chartName} does not exist");
                else
                    selectedItem.Click();

                return true;
            });
        }

    }
}