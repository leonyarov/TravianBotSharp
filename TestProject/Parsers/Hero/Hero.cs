﻿using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Parsers
{
    [TestClass]
    public class Hero
    {
        [TestMethod]
        public void TTWarsGetHealth()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/TTWars.html");
            var value = TTWarsCore.Parsers.Hero.GetHealth(doc);
            Assert.AreEqual(100, value);
        }

        [TestMethod]
        public void TravianOfficialGetHealth()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/Travian.html");
            var value = TravianOfficialCore.Parsers.Hero.GetHealth(doc);
            Assert.AreEqual(100, value);
        }

        [TestMethod]
        public void TravianOfficialHeroGetHealth()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/TravianHeroUI.html");
            var value = TravianOfficialNewHeroUICore.Parsers.Hero.GetHealth(doc);
            Assert.AreEqual(100, value);
        }

        [TestMethod]
        public void TTWarsGetStatus()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/TTWars.html");
            var value = TTWarsCore.Parsers.Hero.GetStatus(doc);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void TravianOfficialGetStatus()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/Travian.html");
            var value = TravianOfficialCore.Parsers.Hero.GetStatus(doc);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void TravianOfficialHeroGetStatus()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/TravianHeroUI.html");
            var value = TravianOfficialNewHeroUICore.Parsers.Hero.GetStatus(doc);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void TTWarsGetAdventureNum()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/TTWars.html");
            var value = TTWarsCore.Parsers.Hero.GetAdventureNum(doc);
            Assert.AreEqual(3, value);
        }

        [TestMethod]
        public void TravianOfficialGetAdventureNum()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/Travian.html");
            var value = TravianOfficialCore.Parsers.Hero.GetAdventureNum(doc);
            Assert.AreEqual(13, value);
        }

        [TestMethod]
        public void TravianOfficialHeroGetAdventureNum()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/logo/TravianHeroUI.html");
            var value = TravianOfficialNewHeroUICore.Parsers.Hero.GetAdventureNum(doc);
            Assert.AreEqual(3, value);
        }

        [TestMethod]
        public void TTWarsGetItems()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/inventory/TTWars.html");
            var value = TTWarsCore.Parsers.Hero.GetItems(doc);
            Assert.AreEqual(5, value.Count);
        }

        [TestMethod]
        public void TravianOfficialGetItems()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/inventory/Travian.html");
            var value = TravianOfficialCore.Parsers.Hero.GetItems(doc);
            Assert.AreEqual(19, value.Count);
        }

        [TestMethod]
        public void TravianOfficialHeroGetItems()
        {
            var doc = new HtmlDocument();
            doc.Load("Parsers/Hero/inventory/TravianHeroUI.html");
            var value = TravianOfficialNewHeroUICore.Parsers.Hero.GetItems(doc);
            Assert.AreEqual(11, value.Count);
        }
    }
}